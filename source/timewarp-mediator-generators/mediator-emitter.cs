#region Purpose
// Emits sealed Mediator (unscoped), per-TScope Sender/Publisher, and MS.DI registration.
#endregion

#region Design
// The host supplies IServiceProvider. Handlers and behaviors resolve from that provider in
// compile-time order, and [assembly: MediatorBehavior] is always woven into Dispatch_*.
// The CallSiteInlining prototype is the public static Dispatch_* method interceptors target.
// Send<TResponse> and Send<TRequest> (void) are type switches. Each arm returns the Dispatch_*
// ValueTask. Unsafe.As reinterprets ValueTask<TConcrete> as ValueTask<TResponse> only when those
// types are equal. A covariant IRequest<out T>, or a request that implements two IRequest<>
// closures, awaits the concrete ValueTask and casts; a mismatch throws InvalidCastException.
// Void arms adapt ValueTask<Unit> to ValueTask. A completed fault or cancellation stays faulted.
// Send(object) and Publish(object) are the only paths that erase the message to object.
// A null argument, NoHandlerException, and a handler that throws before it returns a ValueTask
// leave Send as a thrown exception. That exception is not captured in the returned task.
// With behaviors, Dispatch_* resolves a scoped Chain_* and invokes it. The chain creates its
// MessageHandlerDelegate steps once per scope. Each step resolves from that instance, so a
// behavior may await before next and two scopes may send the same request together.
// No behaviors: the dispatcher resolves a scoped HandlerCache once and reuses it. Each handler
// slot is filled on the first send of that request, so an unresolvable handler does not fail
// other sends and an unsent request does not construct its handler. A transient registration
// is resolved once per cache, which makes that path effectively scoped. Disposing the scope
// disposes the cache, and the next send throws ObjectDisposedException. Nothing is stored in
// a thread-static slot. Static Dispatch_* still resolves from the scope so a call site can
// pass the provider without the dispatcher instance.
// Publish<TNotification> type-tests the typed value and awaits each assignable handler
// sequentially. It does not cast to object. Publish(object) keeps that cast for late-bound hosts.
// Registration is an internal extension class in the host generated namespace. The class name is
// the namespace with '.' removed plus ServiceCollectionExtensions. It registers the dispatcher,
// scoped sender/publisher, concrete handlers and behaviors, a chain per request that has
// behaviors, and HandlerCache when a request has none. Handler and behavior interfaces are not
// registered; MediatorManifest carries that graph.
// The dispatcher stays transient. A singleton dispatcher was measured against this cached
// transient path on net10.0 (task 020, BenchmarkDotNet 0.15.8): singleton 9.776 ns,
// reused transient 10.320 ns, both 24 B, inside each other's noise.
// AddGeneratedMediator does not take a ServiceLifetime.
// Unscoped Mediator is the default pipeline (no [MediatorScope]). Each TScope gets its own
// Sender and Publisher with a disjoint type switch — no runtime "is this my message?" filter.
// Every emitted type is internal: each host emits the same type names, so
// public types collide (CS0436) in any compilation that references two hosts.
// Send(object) cases and Publish type tests are ordered most-derived first (TypeOrder).
// Publish delivers to every bound notification type the value is assignable to (MediatR
// contravariance). Separate if statements, not a switch that returns on the first match, so an
// unbound derived notification still reaches every bound supertype. Pattern locals are indexed
// (r0, n0), never derived from type names, and sanitized member names come from MemberNames so
// A.B_C and A_B.C cannot share Dispatch_*.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class MediatorEmitter
{
  internal static void Emit(List<GeneratedSource> sources, EmissionGraph graph)
  {
    MemberNames names = new(graph);
    sources.Add(new GeneratedSource("Mediator.g.cs", EmitMediator(graph, names)));

    foreach (TypeModel scope in DistinctScopes(graph))
    {
      string suffix = ScopeSuffix(scope, graph, names);
      sources.Add(new GeneratedSource("Sender_" + suffix + ".g.cs", EmitScopedSender(graph, names, scope, suffix)));
      sources.Add(new GeneratedSource("Publisher_" + suffix + ".g.cs", EmitScopedPublisher(graph, names, scope, suffix)));
    }

    sources.Add(new GeneratedSource("MediatorServiceCollectionExtensions.g.cs", EmitRegistration(graph, names)));
  }

  private static string EmitMediator(EmissionGraph graph, MemberNames names)
  {
    List<EmissionRequest> requests = RequestsFor(graph, scope: null);
    List<EmissionNotification> notifications = NotificationsFor(graph, scope: null);
    return EmitDispatcher(
        graph,
        names,
        className: "Mediator",
        interfaceList: "global::TimeWarp.Mediator.IMediator",
        requests,
        notifications,
        includeSend: true,
        includePublish: true);
  }

  private static string EmitScopedSender(EmissionGraph graph, MemberNames names, TypeModel scope, string suffix)
  {
    List<EmissionRequest> requests = RequestsFor(graph, scope);
    return EmitDispatcher(
        graph,
        names,
        className: "Sender_" + suffix,
        interfaceList: "global::TimeWarp.Mediator.ISender<" + Fq(scope) + ">",
        requests,
        notifications: new List<EmissionNotification>(),
        includeSend: true,
        includePublish: false);
  }

  private static string EmitScopedPublisher(EmissionGraph graph, MemberNames names, TypeModel scope, string suffix)
  {
    List<EmissionNotification> notifications = NotificationsFor(graph, scope);
    return EmitDispatcher(
        graph,
        names,
        className: "Publisher_" + suffix,
        interfaceList: "global::TimeWarp.Mediator.IPublisher<" + Fq(scope) + ">",
        requests: new List<EmissionRequest>(),
        notifications,
        includeSend: false,
        includePublish: true);
  }

  private static string EmitDispatcher(
      EmissionGraph graph,
      MemberNames names,
      string className,
      string interfaceList,
      List<EmissionRequest> requests,
      List<EmissionNotification> notifications,
      bool includeSend,
      bool includePublish)
  {
    string ns = graph.GeneratedNamespace;
    StringBuilder builder = new();
    builder.AppendLine("// <auto-generated/>");
    builder.AppendLine("#nullable enable");
    builder.AppendLine();
    if (className == "Mediator")
    {
      builder.Append("[assembly: global::TimeWarp.Mediator.GeneratedMediatorAttribute(typeof(global::");
      builder.Append(ns).AppendLine(".Mediator))]");
      builder.AppendLine();
    }

    builder.Append("namespace ").Append(ns).AppendLine(";");
    builder.AppendLine();

    builder.Append("internal sealed class ").Append(className).Append(" : ").AppendLine(interfaceList);
    builder.AppendLine("{");
    builder.AppendLine("    private readonly global::System.IServiceProvider ServiceProvider;");
    if (HasUnchainedRequest(requests))
    {
      builder.AppendLine("    private HandlerCache? CachedHandlers;");
    }

    builder.AppendLine();
    builder.Append("    public ").Append(className).AppendLine("(global::System.IServiceProvider serviceProvider)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (serviceProvider is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(serviceProvider));");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        ServiceProvider = serviceProvider;");
    builder.AppendLine("    }");

    builder.AppendLine();
    if (includeSend)
    {
      EmitInterfaceSend(builder, names, requests);
      EmitMonomorphicSend(builder, names, requests);
      EmitObjectSend(builder, names, requests);
      EmitDispatchMethods(builder, names, requests);
    }

    if (includePublish)
    {
      EmitPublish(builder, names, notifications);
    }

    if (includeSend)
    {
      builder.AppendLine();
      builder.AppendLine("    private static async global::System.Threading.Tasks.ValueTask<object?> Box<T>(global::System.Threading.Tasks.ValueTask<T> valueTask)");
      builder.AppendLine("    {");
      builder.AppendLine("        T result = await valueTask.ConfigureAwait(false);");
      builder.AppendLine("        return result;");
      builder.AppendLine("    }");
      if (HasUnitRequest(requests))
      {
        EmitUnitAdapters(builder);
      }

      if (HasResponseRequest(requests))
      {
        EmitResponseAdapter(builder);
      }

      if (HasUnchainedRequest(requests))
      {
        EmitHandlerCache(builder, names, requests);
      }
    }

    builder.AppendLine("}");
    return builder.ToString();
  }

  private static void EmitInterfaceSend(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    List<EmissionRequest> responses = new();
    List<EmissionRequest> units = new();
    foreach (EmissionRequest request in requests)
    {
      if (request.IsUnitResponse)
      {
        units.Add(request);
      }
      else
      {
        responses.Add(request);
      }
    }

    builder.AppendLine("    public global::System.Threading.Tasks.ValueTask<TResponse> Send<TResponse>(global::TimeWarp.Mediator.IRequest<TResponse> request, global::System.Threading.CancellationToken cancellationToken = default)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (request is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(request));");
    builder.AppendLine("        }");
    builder.AppendLine();
    EmitResponseSwitch(builder, names, responses);
    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    public global::System.Threading.Tasks.ValueTask Send<TRequest>(TRequest request, global::System.Threading.CancellationToken cancellationToken = default)");
    builder.AppendLine("        where TRequest : global::TimeWarp.Mediator.IRequest");
    builder.AppendLine("    {");
    builder.AppendLine("        if (request is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(request));");
    builder.AppendLine("        }");
    builder.AppendLine();
    EmitUnitSwitch(builder, names, units);
    builder.AppendLine("    }");
    builder.AppendLine();
  }

  private static void EmitResponseSwitch(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    if (requests.Count == 0)
    {
      builder.AppendLine("        throw new global::TimeWarp.Mediator.NoHandlerException(request.GetType());");
      return;
    }

    builder.AppendLine("        switch (request)");
    builder.AppendLine("        {");
    int index = 0;
    foreach (EmissionRequest request in OrderedRequests(requests))
    {
      string local = "r" + index;
      string responseType = Fq(request.ResponseType);
      builder.Append("            case ").Append(Fq(request.RequestType)).Append(' ').Append(local).AppendLine(":");
      builder.AppendLine("            {");
      builder.Append("                global::System.Threading.Tasks.ValueTask<").Append(responseType).Append("> pending = ");
      AppendInstanceCall(builder, names, request, local);
      builder.AppendLine(";");
      builder.Append("                if (typeof(TResponse) == typeof(").Append(responseType).AppendLine("))");
      builder.AppendLine("                {");
      builder.Append("                    return global::System.Runtime.CompilerServices.Unsafe.As<global::System.Threading.Tasks.ValueTask<");
      builder.Append(responseType).Append(">, global::System.Threading.Tasks.ValueTask<TResponse>>(ref pending);");
      builder.AppendLine();
      builder.AppendLine("                }");
      builder.AppendLine();
      builder.Append("                return AdaptResponse<").Append(responseType).AppendLine(", TResponse>(pending);");
      builder.AppendLine("            }");
      index++;
    }

    builder.AppendLine("            default:");
    builder.AppendLine("                throw new global::TimeWarp.Mediator.NoHandlerException(request.GetType());");
    builder.AppendLine("        }");
  }

  private static void EmitUnitSwitch(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    if (requests.Count == 0)
    {
      builder.AppendLine("        throw new global::TimeWarp.Mediator.NoHandlerException(request.GetType());");
      return;
    }

    builder.AppendLine("        switch (request)");
    builder.AppendLine("        {");
    int index = 0;
    foreach (EmissionRequest request in OrderedRequests(requests))
    {
      string local = "r" + index;
      builder.Append("            case ").Append(Fq(request.RequestType)).Append(' ').Append(local).AppendLine(":");
      builder.AppendLine("            {");
      builder.Append("                global::System.Threading.Tasks.ValueTask<global::TimeWarp.Mediator.Unit> pending = ");
      AppendInstanceCall(builder, names, request, local);
      builder.AppendLine(";");
      builder.AppendLine("                return AsVoid(pending);");
      builder.AppendLine("            }");
      index++;
    }

    builder.AppendLine("            default:");
    builder.AppendLine("                throw new global::TimeWarp.Mediator.NoHandlerException(request.GetType());");
    builder.AppendLine("        }");
  }

  private static void AppendInstanceCall(StringBuilder builder, MemberNames names, EmissionRequest request, string requestLocal)
  {
    if (request.ClosedBehaviors.Length == 0)
    {
      if (request.IsUnitResponse)
      {
        builder.Append("AdaptUnit(LiveCache().");
      }
      else
      {
        builder.Append("LiveCache().");
      }

      builder.Append(names.For(request.HandlerType)).Append(".Handle(").Append(requestLocal).Append(", cancellationToken)");
      if (request.IsUnitResponse)
      {
        builder.Append(')');
      }

      return;
    }

    AppendDispatchCall(builder, names, request, requestLocal);
  }

  private static void AppendDispatchCall(StringBuilder builder, MemberNames names, EmissionRequest request, string requestLocal)
  {
    builder.Append("Dispatch_").Append(names.For(request.RequestType)).Append('(');
    builder.Append("ServiceProvider, ");
    builder.Append(requestLocal).Append(", cancellationToken)");
  }

  private static List<EmissionRequest> OrderedRequests(List<EmissionRequest> requests)
  {
    Dictionary<TypeModel, EmissionRequest> firstByType = new(TypeModelKeyComparer.Instance);
    foreach (EmissionRequest request in requests)
    {
      if (!firstByType.ContainsKey(request.RequestType))
      {
        firstByType.Add(request.RequestType, request);
      }
    }

    List<EmissionRequest> ordered = new();
    foreach (TypeModel requestType in TypeOrder.MostDerivedFirst(firstByType.Keys))
    {
      ordered.Add(firstByType[requestType]);
    }

    return ordered;
  }

  // A struct request cannot be null; `request is null` on it is CS0037.
  private static void EmitRequestNullGuard(StringBuilder builder, TypeModel requestType)
  {
    if (requestType.IsValueType)
    {
      return;
    }

    builder.AppendLine("        if (request is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(request));");
    builder.AppendLine("        }");
    builder.AppendLine();
  }

  private static void EmitMonomorphicSend(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    foreach (EmissionRequest request in requests)
    {
      string requestType = Fq(request.RequestType);
      string responseType = Fq(request.ResponseType);
      builder.Append("    public global::System.Threading.Tasks.ValueTask<").Append(responseType).Append("> Send(");
      builder.Append(requestType).AppendLine(" request, global::System.Threading.CancellationToken cancellationToken = default)");
      builder.AppendLine("    {");
      EmitRequestNullGuard(builder, request.RequestType);
      builder.Append("        return ");
      AppendInstanceCall(builder, names, request, "request");
      builder.AppendLine(";");
      builder.AppendLine("    }");
      builder.AppendLine();
    }
  }

  private static void EmitObjectSend(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    builder.AppendLine("    public global::System.Threading.Tasks.ValueTask<object?> Send(object request, global::System.Threading.CancellationToken cancellationToken = default)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (request is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(request));");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        switch (request)");
    builder.AppendLine("        {");
    // Most-derived first: a base case before its derived case is CS8120. Locals are indexed so a
    // type named Request, Object, or Lock cannot shadow a parameter or hit a keyword.
    Dictionary<TypeModel, EmissionRequest> firstByType = new(TypeModelKeyComparer.Instance);
    foreach (EmissionRequest request in requests)
    {
      if (!firstByType.ContainsKey(request.RequestType))
      {
        firstByType.Add(request.RequestType, request);
      }
    }

    int index = 0;
    foreach (TypeModel requestType in TypeOrder.MostDerivedFirst(firstByType.Keys))
    {
      string local = "r" + index;
      builder.Append("            case ").Append(Fq(requestType)).Append(' ').Append(local).AppendLine(":");
      builder.Append("                return Box(");
      AppendInstanceCall(builder, names, firstByType[requestType], local);
      builder.AppendLine(");");
      index++;
    }

    builder.AppendLine("            default:");
    builder.AppendLine("                throw new global::TimeWarp.Mediator.NoHandlerException(request.GetType());");
    builder.AppendLine("        }");
    builder.AppendLine("    }");
    builder.AppendLine();
  }

}
