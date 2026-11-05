#region Purpose
// Emits HandlerCache, service registration, and scope partitions.
#endregion

#region Design
// Registration is an internal extension in the host generated namespace. Each scope gets its own sender, publisher, and type switch.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class MediatorEmitter
{
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
