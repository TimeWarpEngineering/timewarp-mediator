#region Purpose
// Emits Publish, per-request dispatch, and the handler chain.
#endregion

#region Design
// Publish delivers to every bound notification the value is assignable to. A chain exists only for a request that closed at least one behavior.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class MediatorEmitter
{
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
}
