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
// transient path on net10.0 (task 020, BenchmarkDotNet 0.15.8) and was not faster, so
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

internal static class MediatorEmitter
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

  private static void EmitPublish(StringBuilder builder, MemberNames names, List<EmissionNotification> notifications)
  {
    builder.AppendLine("    public global::System.Threading.Tasks.ValueTask Publish<TNotification>(TNotification notification, global::System.Threading.CancellationToken cancellationToken = default)");
    builder.AppendLine("        where TNotification : global::TimeWarp.Mediator.INotification");
    builder.AppendLine("    {");
    builder.AppendLine("        if (notification is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(notification));");
    builder.AppendLine("        }");
    builder.AppendLine();
    if (notifications.Count == 0)
    {
      builder.AppendLine("        return default;");
    }
    else if (notifications.Count == 1)
    {
      EmissionNotification only = notifications[0];
      builder.Append("        if (notification is ").Append(Fq(only.NotificationType)).AppendLine(" n0)");
      builder.AppendLine("        {");
      builder.Append("            return Publish_").Append(names.For(only.NotificationType)).AppendLine("(n0, cancellationToken);");
      builder.AppendLine("        }");
      builder.AppendLine();
      builder.AppendLine("        return default;");
    }
    else
    {
      builder.AppendLine("        return PublishTyped(notification, cancellationToken);");
    }

    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    public global::System.Threading.Tasks.ValueTask Publish(object notification, global::System.Threading.CancellationToken cancellationToken = default)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (notification is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(notification));");
    builder.AppendLine("        }");
    builder.AppendLine();
    if (notifications.Count == 0)
    {
      builder.AppendLine("        return default;");
      builder.AppendLine("    }");
      builder.AppendLine();
      return;
    }

    if (notifications.Count == 1)
    {
      EmissionNotification only = notifications[0];
      builder.Append("        if (notification is ").Append(Fq(only.NotificationType)).AppendLine(" n0)");
      builder.AppendLine("        {");
      builder.Append("            return Publish_").Append(names.For(only.NotificationType)).AppendLine("(n0, cancellationToken);");
      builder.AppendLine("        }");
      builder.AppendLine();
      builder.AppendLine("        return default;");
      builder.AppendLine("    }");
    }
    else
    {
      builder.AppendLine("        return PublishCore(notification, cancellationToken);");
      builder.AppendLine("    }");
      builder.AppendLine();
      EmitPublishCore(builder, names, notifications, typed: false);
    }

    if (notifications.Count > 1)
    {
      EmitPublishCore(builder, names, notifications, typed: true);
    }

    foreach (EmissionNotification notification in notifications)
    {
      EmitPublishOwn(builder, names, notification);
    }
  }

  // Separate ifs, most-derived first. An unbound derived value matches every bound supertype.
  private static void EmitPublishCore(StringBuilder builder, MemberNames names, List<EmissionNotification> notifications, bool typed)
  {
    List<TypeModel> notificationTypes = new();
    foreach (EmissionNotification notification in notifications)
    {
      notificationTypes.Add(notification.NotificationType);
    }

    if (typed)
    {
      builder.AppendLine("    private async global::System.Threading.Tasks.ValueTask PublishTyped<TNotification>(TNotification notification, global::System.Threading.CancellationToken cancellationToken)");
      builder.AppendLine("        where TNotification : global::TimeWarp.Mediator.INotification");
    }
    else
    {
      builder.AppendLine("    private async global::System.Threading.Tasks.ValueTask PublishCore(object notification, global::System.Threading.CancellationToken cancellationToken)");
    }

    builder.AppendLine("    {");
    int caseIndex = 0;
    foreach (TypeModel notificationType in TypeOrder.MostDerivedFirst(notificationTypes))
    {
      string local = "n" + caseIndex;
      builder.Append("        if (notification is ").Append(Fq(notificationType)).Append(' ').Append(local).AppendLine(")");
      builder.AppendLine("        {");
      builder.Append("            await Publish_").Append(names.For(notificationType));
      builder.Append('(').Append(local).AppendLine(", cancellationToken).ConfigureAwait(false);");
      builder.AppendLine("        }");
      caseIndex++;
    }

    builder.AppendLine("    }");
    builder.AppendLine();
  }

  private static void EmitPublishOwn(StringBuilder builder, MemberNames names, EmissionNotification notification)
  {
    string typeName = Fq(notification.NotificationType);
    string method = "Publish_" + names.For(notification.NotificationType);
    bool single = notification.HandlerTypes.Length == 1;
    builder.Append("    private ");
    if (!single)
    {
      builder.Append("async ");
    }

    builder.Append("global::System.Threading.Tasks.ValueTask ").Append(method).Append('(');
    builder.Append(typeName).AppendLine(" notification, global::System.Threading.CancellationToken cancellationToken)");
    builder.AppendLine("    {");
    int index = 0;
    foreach (TypeModel handler in notification.HandlerTypes)
    {
      string handlerType = Fq(handler);
      string variable = "handler" + index;
      builder.Append("        ").Append(handlerType).Append(' ').Append(variable).Append(" = ");
      builder.Append("global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<");
      builder.Append(handlerType).AppendLine(">(ServiceProvider);");

      if (single)
      {
        builder.Append("        return ").Append(variable).AppendLine(".Handle(notification, cancellationToken);");
      }
      else
      {
        builder.Append("        await ").Append(variable).AppendLine(".Handle(notification, cancellationToken).ConfigureAwait(false);");
      }

      index++;
    }

    builder.AppendLine("    }");
    builder.AppendLine();
  }

  private static void EmitDispatchMethods(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    foreach (EmissionRequest request in requests)
    {
      string requestType = Fq(request.RequestType);
      string responseType = Fq(request.ResponseType);
      string dispatch = "Dispatch_" + names.For(request.RequestType);
      bool chain = request.ClosedBehaviors.Length > 0;
      builder.Append("    public static global::System.Threading.Tasks.ValueTask<").Append(responseType).Append("> ").Append(dispatch).Append('(');
      builder.Append("global::System.IServiceProvider scope, ");
      builder.Append(requestType).AppendLine(" request, global::System.Threading.CancellationToken cancellationToken)");
      builder.AppendLine("    {");
      builder.AppendLine("        if (scope is null)");
      builder.AppendLine("        {");
      builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(scope));");
      builder.AppendLine("        }");
      builder.AppendLine();

      EmitRequestNullGuard(builder, request.RequestType);
      if (chain)
      {
        builder.Append("        return global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<");
        builder.Append(ChainClassName(names, request)).AppendLine(">(scope).Invoke(request, cancellationToken);");
        builder.AppendLine("    }");
        builder.AppendLine();
        EmitChain(builder, names, request);
      }
      else
      {
        EmitHandlerResolve(builder, request.HandlerType, "handler", "scope");
        if (request.IsUnitResponse)
        {
          builder.AppendLine("        return AdaptUnit(handler.Handle(request, cancellationToken));");
        }
        else
        {
          builder.AppendLine("        return handler.Handle(request, cancellationToken);");
        }

        builder.AppendLine("    }");
        builder.AppendLine();
      }
    }
  }

  private static string ChainClassName(MemberNames names, EmissionRequest request)
  {
    return "Chain_" + names.For(request.RequestType);
  }

  private static void EmitChain(StringBuilder builder, MemberNames names, EmissionRequest request)
  {
    string requestType = Fq(request.RequestType);
    string responseType = Fq(request.ResponseType);
    string chain = ChainClassName(names, request);
    string delegateType = "global::TimeWarp.Mediator.MessageHandlerDelegate<" + requestType + ", " + responseType + ">";
    int steps = request.ClosedBehaviors.Length + 1;
    builder.Append("    internal sealed class ").AppendLine(chain);
    builder.AppendLine("    {");
    builder.AppendLine("        private readonly global::System.IServiceProvider Scope;");
    for (int step = 0; step < steps; step++)
    {
      builder.Append("        private readonly ").Append(delegateType).Append(" Next").Append(step).AppendLine(";");
    }

    builder.AppendLine();
    builder.Append("        public ").Append(chain).AppendLine("(global::System.IServiceProvider scope)");
    builder.AppendLine("        {");
    builder.AppendLine("            if (scope is null)");
    builder.AppendLine("            {");
    builder.AppendLine("                throw new global::System.ArgumentNullException(nameof(scope));");
    builder.AppendLine("            }");
    builder.AppendLine();
    builder.AppendLine("            Scope = scope;");
    for (int step = 0; step < steps; step++)
    {
      builder.Append("            Next").Append(step).Append(" = Step").Append(step).AppendLine(";");
    }

    builder.AppendLine("        }");
    builder.AppendLine();
    builder.Append("        internal global::System.Threading.Tasks.ValueTask<").Append(responseType).Append("> Invoke(");
    builder.Append(requestType).AppendLine(" request, global::System.Threading.CancellationToken cancellationToken)");
    builder.AppendLine("        {");
    builder.AppendLine("            return Next0(request, cancellationToken);");
    builder.AppendLine("        }");
    builder.AppendLine();
    for (int step = 0; step < steps; step++)
    {
      bool terminal = step == request.ClosedBehaviors.Length;
      TypeModel service = terminal ? request.HandlerType : request.ClosedBehaviors[step];
      string serviceType = Fq(service);
      string variable = terminal ? "handler" : "behavior";
      builder.Append("        private global::System.Threading.Tasks.ValueTask<").Append(responseType).Append("> Step").Append(step).Append('(');
      builder.Append(requestType).AppendLine(" request, global::System.Threading.CancellationToken cancellationToken)");
      builder.AppendLine("        {");
      builder.Append("            ").Append(serviceType).Append(' ').Append(variable).Append(" = ");
      builder.Append("global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<");
      builder.Append(serviceType).AppendLine(">(Scope);");
      if (terminal && request.IsUnitResponse)
      {
        builder.Append("            return AdaptUnit(").Append(variable).AppendLine(".Handle(request, cancellationToken));");
      }
      else if (terminal)
      {
        builder.Append("            return ").Append(variable).AppendLine(".Handle(request, cancellationToken);");
      }
      else
      {
        builder.Append("            return ").Append(variable).Append(".Handle(request, Next").Append(step + 1).AppendLine(", cancellationToken);");
      }

      builder.AppendLine("        }");
      builder.AppendLine();
    }

    builder.AppendLine("    }");
    builder.AppendLine();
  }

  private static void EmitHandlerResolve(
      StringBuilder builder,
      TypeModel handler,
      string variable,
      string scopeExpression)
  {
    string handlerType = Fq(handler);
    builder.Append("        ").Append(handlerType).Append(' ').Append(variable).Append(" = ");
    builder.Append("global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<");
    builder.Append(handlerType).Append(">(").Append(scopeExpression).AppendLine(");");
  }

  private static void EmitUnitAdapters(StringBuilder builder)
  {
    builder.AppendLine();
    builder.AppendLine("    private static global::System.Threading.Tasks.ValueTask<global::TimeWarp.Mediator.Unit> AdaptUnit(global::System.Threading.Tasks.ValueTask pending)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (pending.IsCompletedSuccessfully)");
    builder.AppendLine("        {");
    builder.AppendLine("            pending.GetAwaiter().GetResult();");
    builder.AppendLine("            return new global::System.Threading.Tasks.ValueTask<global::TimeWarp.Mediator.Unit>(global::TimeWarp.Mediator.Unit.Value);");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        return AwaitUnit(pending);");
    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    private static async global::System.Threading.Tasks.ValueTask<global::TimeWarp.Mediator.Unit> AwaitUnit(global::System.Threading.Tasks.ValueTask pending)");
    builder.AppendLine("    {");
    builder.AppendLine("        await pending.ConfigureAwait(false);");
    builder.AppendLine("        return global::TimeWarp.Mediator.Unit.Value;");
    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    private static global::System.Threading.Tasks.ValueTask AsVoid<T>(global::System.Threading.Tasks.ValueTask<T> pending)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (pending.IsCompletedSuccessfully)");
    builder.AppendLine("        {");
    builder.AppendLine("            pending.GetAwaiter().GetResult();");
    builder.AppendLine("            return default;");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        return AwaitVoid(pending);");
    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    private static async global::System.Threading.Tasks.ValueTask AwaitVoid<T>(global::System.Threading.Tasks.ValueTask<T> pending)");
    builder.AppendLine("    {");
    builder.AppendLine("        await pending.ConfigureAwait(false);");
    builder.AppendLine("    }");
  }

  private static bool HasUnitRequest(List<EmissionRequest> requests)
  {
    foreach (EmissionRequest request in requests)
    {
      if (request.IsUnitResponse)
      {
        return true;
      }
    }

    return false;
  }

  private static bool HasResponseRequest(List<EmissionRequest> requests)
  {
    foreach (EmissionRequest request in requests)
    {
      if (!request.IsUnitResponse)
      {
        return true;
      }
    }

    return false;
  }

  private static bool HasUnchainedRequest(List<EmissionRequest> requests)
  {
    foreach (EmissionRequest request in requests)
    {
      if (request.ClosedBehaviors.Length == 0)
      {
        return true;
      }
    }

    return false;
  }

  // Property names come from MemberNames. The backing field appends "Cache" and, if that
  // identifier is already a property name, a numeric suffix. Two members of this class cannot share a name.
  private static List<(TypeModel Handler, string Property, string Field)> HandlerSlots(
      MemberNames names,
      List<TypeModel> handlers)
  {
    HashSet<string> taken = new(StringComparer.Ordinal);
    foreach (TypeModel handler in handlers)
    {
      taken.Add(names.For(handler));
    }

    List<(TypeModel Handler, string Property, string Field)> slots = new();
    foreach (TypeModel handler in handlers)
    {
      string property = names.For(handler);
      string field = property + "Cache";
      int suffix = 2;
      while (!taken.Add(field))
      {
        field = property + "Cache" + suffix;
        suffix++;
      }

      slots.Add((handler, property, field));
    }

    return slots;
  }

  private static void EmitHandlerCache(StringBuilder builder, MemberNames names, List<EmissionRequest> requests)
  {
    List<TypeModel> handlers = new();
    HashSet<TypeModel> seen = new(TypeModelKeyComparer.Instance);
    foreach (EmissionRequest request in requests)
    {
      if (request.ClosedBehaviors.Length == 0 && seen.Add(request.HandlerType))
      {
        handlers.Add(request.HandlerType);
      }
    }

    builder.AppendLine();
    builder.AppendLine("    private HandlerCache LiveCache()");
    builder.AppendLine("    {");
    builder.AppendLine("        HandlerCache? cache = CachedHandlers;");
    builder.AppendLine("        if (cache is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            cache = global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<HandlerCache>(ServiceProvider);");
    builder.AppendLine("            CachedHandlers = cache;");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        cache.EnsureAlive();");
    builder.AppendLine("        return cache;");
    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    internal sealed class HandlerCache : global::System.IDisposable");
    builder.AppendLine("    {");
    builder.AppendLine("        private readonly global::System.IServiceProvider Scope;");
    List<(TypeModel Handler, string Property, string Field)> slots = HandlerSlots(names, handlers);
    foreach ((TypeModel Handler, string Property, string Field) slot in slots)
    {
      builder.Append("        private ").Append(Fq(slot.Handler)).Append("? ").Append(slot.Field).AppendLine(";");
    }

    builder.AppendLine("        private volatile bool Disposed;");
    builder.AppendLine();
    builder.AppendLine("        public HandlerCache(global::System.IServiceProvider scope)");
    builder.AppendLine("        {");
    builder.AppendLine("            if (scope is null)");
    builder.AppendLine("            {");
    builder.AppendLine("                throw new global::System.ArgumentNullException(nameof(scope));");
    builder.AppendLine("            }");
    builder.AppendLine();
    builder.AppendLine("            Scope = scope;");
    builder.AppendLine("        }");
    foreach ((TypeModel handler, string property, string field) in slots)
    {
      builder.AppendLine();
      builder.Append("        internal ").Append(Fq(handler)).Append(' ').Append(property).Append(" => ");
      builder.Append(field).Append(" ??= global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<");
      builder.Append(Fq(handler)).AppendLine(">(Scope);");
    }

    builder.AppendLine();
    builder.AppendLine("        internal void EnsureAlive()");
    builder.AppendLine("        {");
    builder.AppendLine("            if (Disposed)");
    builder.AppendLine("            {");
    builder.AppendLine("                throw new global::System.ObjectDisposedException(\"IServiceProvider\");");
    builder.AppendLine("            }");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        public void Dispose()");
    builder.AppendLine("        {");
    builder.AppendLine("            Disposed = true;");
    builder.AppendLine("        }");
    builder.AppendLine("    }");
  }

  private static void EmitResponseAdapter(StringBuilder builder)
  {
    builder.AppendLine();
    builder.AppendLine("    private static async global::System.Threading.Tasks.ValueTask<TResponse> AdaptResponse<TConcrete, TResponse>(global::System.Threading.Tasks.ValueTask<TConcrete> pending)");
    builder.AppendLine("    {");
    builder.AppendLine("        TConcrete result = await pending.ConfigureAwait(false);");
    builder.AppendLine("        object? boxed = result;");
    builder.AppendLine("        if (boxed is TResponse typed)");
    builder.AppendLine("        {");
    builder.AppendLine("            return typed;");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        if (boxed is null && default(TResponse) is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            return default!;");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.AppendLine("        throw new global::System.InvalidCastException();");
    builder.AppendLine("    }");
  }

  private static string EmitRegistration(EmissionGraph graph, MemberNames names)
  {
    string ns = graph.GeneratedNamespace;
    StringBuilder builder = new();
    string className = RegistrationClassName(ns);
    builder.AppendLine("// <auto-generated/>");
    builder.AppendLine("#nullable enable");
    builder.AppendLine();
    builder.Append("namespace ").Append(ns).AppendLine(";");
    builder.AppendLine();
    builder.Append("internal static class ").Append(className).AppendLine();
    builder.AppendLine("{");
    builder.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddGeneratedMediator(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
    builder.AppendLine("    {");
    builder.AppendLine("        if (services is null)");
    builder.AppendLine("        {");
    builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(services));");
    builder.AppendLine("        }");
    builder.AppendLine();
    builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<").Append(ns).Append(".Mediator>(services);").AppendLine();
    builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<global::TimeWarp.Mediator.IMediator>(services, static sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<").Append(ns).AppendLine(".Mediator>(sp));");
    builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<global::TimeWarp.Mediator.ISender>(services, static sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<").Append(ns).AppendLine(".Mediator>(sp));");
    builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<global::TimeWarp.Mediator.IPublisher>(services, static sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<").Append(ns).AppendLine(".Mediator>(sp));");

    EmitHandlerRegistrations(builder, names, ns + ".Mediator", RequestsFor(graph, scope: null), NotificationsFor(graph, scope: null));

    builder.AppendLine("        return services;");
    builder.AppendLine("    }");

    List<TypeModel> scopes = DistinctScopes(graph);
    if (scopes.Count > 0)
    {
      builder.AppendLine();
      builder.AppendLine("    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddGeneratedMediator<TScope>(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
      builder.AppendLine("    {");
      builder.AppendLine("        if (services is null)");
      builder.AppendLine("        {");
      builder.AppendLine("            throw new global::System.ArgumentNullException(nameof(services));");
      builder.AppendLine("        }");
      builder.AppendLine();
      foreach (TypeModel scope in scopes)
      {
        string suffix = ScopeSuffix(scope, graph, names);
        builder.Append("        if (typeof(TScope) == typeof(").Append(Fq(scope)).AppendLine("))");
        builder.AppendLine("        {");
        builder.Append("            AddGeneratedMediator_").Append(suffix).AppendLine("(services);");
        builder.AppendLine("            return services;");
        builder.AppendLine("        }");
      }

      builder.AppendLine("        throw new global::System.ArgumentException(\"No generated pipeline for scope '\" + typeof(TScope).FullName + \"'.\", nameof(TScope));");
      builder.AppendLine("    }");

      foreach (TypeModel scope in scopes)
      {
        string suffix = ScopeSuffix(scope, graph, names);
        string senderType = ns + ".Sender_" + suffix;
        string publisherType = ns + ".Publisher_" + suffix;
        builder.AppendLine();
        builder.Append("    private static void AddGeneratedMediator_").Append(suffix).AppendLine("(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
        builder.AppendLine("    {");
        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<").Append(senderType).AppendLine(">(services);");
        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<global::TimeWarp.Mediator.ISender<").Append(Fq(scope)).Append(">>(services, static sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<").Append(senderType).AppendLine(">(sp));");
        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<").Append(publisherType).AppendLine(">(services);");
        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddTransient<global::TimeWarp.Mediator.IPublisher<").Append(Fq(scope)).Append(">>(services, static sp => global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<").Append(publisherType).AppendLine(">(sp));");
        EmitHandlerRegistrations(builder, names, ns + ".Sender_" + suffix, RequestsFor(graph, scope), NotificationsFor(graph, scope));
        builder.AppendLine("    }");
      }
    }

    builder.AppendLine("}");
    return builder.ToString();
  }

  private static void EmitHandlerRegistrations(
      StringBuilder builder,
      MemberNames names,
      string dispatcherType,
      List<EmissionRequest> requests,
      List<EmissionNotification> notifications)
  {
    HashSet<string> registered = new();
    bool unchained = false;
    foreach (EmissionRequest request in requests)
    {
      string handlerType = Fq(request.HandlerType);
      if (registered.Add(handlerType))
      {
        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<").Append(handlerType).AppendLine(">(services);");
      }

      foreach (TypeModel behavior in request.ClosedBehaviors)
      {
        string behaviorType = Fq(behavior);
        if (!registered.Add(behaviorType))
        {
          continue;
        }

        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<").Append(behaviorType).AppendLine(">(services);");
      }

      if (request.ClosedBehaviors.Length == 0)
      {
        unchained = true;
        continue;
      }

      string chainType = dispatcherType + "." + ChainClassName(names, request);
      if (registered.Add(chainType))
      {
        builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<global::").Append(chainType).AppendLine(">(services);");
      }
    }

    if (unchained)
    {
      builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<global::").Append(dispatcherType).AppendLine(".HandlerCache>(services);");
    }

    foreach (EmissionNotification notification in notifications)
    {
      foreach (TypeModel handler in notification.HandlerTypes)
      {
        string handlerType = Fq(handler);
        if (registered.Add(handlerType))
        {
          builder.Append("        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<").Append(handlerType).AppendLine(">(services);");
        }
      }
    }
  }

  // TimeWarp.Mediator.Generated -> TimeWarpMediatorGeneratedServiceCollectionExtensions.
  private static string RegistrationClassName(string generatedNamespace)
  {
    StringBuilder builder = new();
    foreach (char character in generatedNamespace)
    {
      if (character != '.')
      {
        builder.Append(character);
      }
    }

    builder.Append("ServiceCollectionExtensions");
    return builder.ToString();
  }

  private static string Fq(TypeModel type)
  {
    return type.FullyQualified;
  }

  private static List<TypeModel> DistinctScopes(EmissionGraph graph)
  {
    List<TypeModel> scopes = new();
    foreach (EmissionRequest request in graph.Requests)
    {
      AddScope(scopes, request.ScopeType);
    }

    foreach (EmissionNotification notification in graph.Notifications)
    {
      AddScope(scopes, notification.ScopeType);
    }

    scopes.Sort((left, right) => string.CompareOrdinal(left.Display, right.Display));
    return scopes;
  }

  private static void AddScope(List<TypeModel> scopes, TypeModel? scope)
  {
    if (scope is null)
    {
      return;
    }

    foreach (TypeModel existing in scopes)
    {
      if (TypeModels.Same(existing, scope))
      {
        return;
      }
    }

    scopes.Add(scope);
  }

  private static string ScopeSuffix(TypeModel scope, EmissionGraph graph, MemberNames names)
  {
    int sameName = 0;
    foreach (TypeModel candidate in DistinctScopes(graph))
    {
      if (candidate.Name == scope.Name)
      {
        sameName++;
      }
    }

    return sameName <= 1 ? scope.Name : names.For(scope);
  }

  private static List<EmissionRequest> RequestsFor(EmissionGraph graph, TypeModel? scope)
  {
    List<EmissionRequest> list = new();
    foreach (EmissionRequest request in graph.Requests)
    {
      if (TypeModels.Same(request.ScopeType, scope))
      {
        list.Add(request);
      }
    }

    return list;
  }

  private static List<EmissionNotification> NotificationsFor(EmissionGraph graph, TypeModel? scope)
  {
    List<EmissionNotification> list = new();
    foreach (EmissionNotification notification in graph.Notifications)
    {
      if (TypeModels.Same(notification.ScopeType, scope))
      {
        list.Add(notification);
      }
    }

    return list;
  }
}