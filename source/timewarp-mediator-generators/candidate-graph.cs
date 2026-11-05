#region Purpose
// Builds the emission graph and diagnostics from equatable host and referenced-assembly facts.
#endregion

#region Design
// Same decisions as MessageGraphBuilder: member assemblies by name, handlers by display name,
// one binding per request, inherited handler interfaces dropped before that dedupe, behaviors
// closed by constraint. Facts are ordered by assembly name, then discovery index, before byKey.
// An inaccessible notification match does not suppress TWM001 for a request of the same type.
// The host assembly name is ReferencedInput.HostAssemblyName, taken from the compilation.
// The analyzer keeps the symbol walk. This path exists so the generator model step does not take
// a Compilation. Display strings were copied from symbols at the syntax and metadata boundary,
// so the emitters still print the same text.
// A declared handler is added beside any real match. The same type (key or fully qualified name)
// is one handler. AddHandler does that dedupe: the request fact can be visited before the handler
// fact, and the match path would otherwise add the same type again. A different type is a second
// handler and TWM002. An unreadable name is TWM012 here and suppresses TWM001. A readable name
// that no symbol has yet is still emitted; the analyzer reports TWM012 after every generator has
// run, so this step does not.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class CandidateGraph
{
  internal static MediatorModel Create(HostInput host, ReferencedInput referenced, MembershipOptions options, CancellationToken cancellationToken)
  {
    string? missingRuntime = FirstMissingRuntime(referenced.MissingRuntime, host.DeclaredRuntime);
    if (missingRuntime is not null)
    {
      string metadataName = missingRuntime;
      string hint = metadataName.StartsWith("TimeWarp.Mediator.", StringComparison.Ordinal)
          ? "Reference the TimeWarp.Mediator package"
          : "Reference Microsoft.Extensions.DependencyInjection.Abstractions";
      Diagnostic diagnostic = Diagnostic.Create(
          DiagnosticDescriptors.MediatorRuntimeMissing,
          Location.None,
          metadataName,
          hint);
      return new MediatorModel(
          EquatableArray<GeneratedSource>.Empty,
          new EquatableArray<DiagnosticInfo>(ImmutableArray.Create(DiagnosticInfo.From(diagnostic))));
    }

    List<TypeFact> facts = OrderFacts(host, referenced);
    Dictionary<string, TypeFact> byKey = new(StringComparer.Ordinal);
    List<TypeFact> unique = new();
    foreach (TypeFact fact in facts)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!byKey.ContainsKey(fact.Type.Key))
      {
        byKey.Add(fact.Type.Key, fact);
        unique.Add(fact);
      }
    }

    HashSet<string> members = new(StringComparer.Ordinal);
    if (referenced.HostAssemblyName.Length > 0)
    {
      members.Add(referenced.HostAssemblyName);
    }

    foreach (string name in referenced.MemberAssemblyNames)
    {
      members.Add(name);
    }

    List<DiagnosticInfo> diagnostics = new();
    List<DiagnosticInfo> generatorDiagnostics = new();
    HashSet<string> reported = new(StringComparer.Ordinal);
    HashSet<string> dropped = new(StringComparer.Ordinal);
    Dictionary<string, TypeModel> scopeByMessage = new(StringComparer.Ordinal);
    Dictionary<string, List<TypeModel>> handlersByRequest = new(StringComparer.Ordinal);
    Dictionary<string, TypeModel> responseByRequest = new(StringComparer.Ordinal);
    HashSet<string> voidRequests = new(StringComparer.Ordinal);
    Dictionary<string, List<TypeModel>> handlersByNotification = new(StringComparer.Ordinal);
    Dictionary<string, TypeModel> requests = new(StringComparer.Ordinal);
    HashSet<string> declarationCovers = new(StringComparer.Ordinal);

    foreach (TypeFact fact in unique)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!fact.Type.IsDiscoverable)
      {
        continue;
      }

      if (fact.IsConcreteRequest && members.Contains(fact.Type.AssemblyName))
      {
        requests[fact.Type.Key] = fact.Type;
      }

      HashSet<string> boundRequests = new(StringComparer.Ordinal);
      foreach (HandlerMatch match in fact.Matches)
      {
        if (BoundByBase(fact.Type, match, byKey, members))
        {
          continue;
        }

        TypeModel message = Prefer(match.Message, byKey);
        if (match.MessageScope is not null && !scopeByMessage.ContainsKey(message.Key))
        {
          scopeByMessage.Add(message.Key, match.MessageScope);
        }

        // Two-arity matches are listed first. BoundByBase runs before this dedupe, so an
        // inherited interface does not consume the request and hide the derived class's own match.
        if (!match.IsNotification && !boundRequests.Add(message.Key))
        {
          continue;
        }

        if (!Allows(fact.Type, reported, generatorDiagnostics, referenced.HostAssemblyName)
            || !Allows(match.Message, reported, generatorDiagnostics, referenced.HostAssemblyName)
            || (match.Response is not null && !Allows(match.Response, reported, generatorDiagnostics, referenced.HostAssemblyName)))
        {
          if (!match.IsNotification)
          {
            dropped.Add(match.Message.Key);
          }

          continue;
        }

        if (match.IsNotification)
        {
          AddHandler(handlersByNotification, message, fact.Type);
        }
        else if (match.Response is not null)
        {
          AddHandler(handlersByRequest, message, fact.Type);
          responseByRequest[message.Key] = Prefer(match.Response, byKey);
          if (match.IsVoid)
          {
            voidRequests.Add(message.Key);
          }

          requests[message.Key] = message;
        }
      }

      if (fact.IsConcreteRequest
          && members.Contains(fact.Type.AssemblyName)
          && (fact.DeclaredHandlers.Length > 0 || fact.DeclaredHandlerFailures.Length > 0))
      {
        declarationCovers.Add(fact.Type.Key);
        requests[fact.Type.Key] = fact.Type;
        foreach (DeclaredHandlerFailure failure in fact.DeclaredHandlerFailures)
        {
          DiagnosticDescriptor descriptor = failure.Problem == DeclaredHandlerNameProblem.NotATypeName
              ? DiagnosticDescriptors.DeclaredHandlerNotATypeName
              : DiagnosticDescriptors.DeclaredHandlerNameUnreadable;
          diagnostics.Add(Info(
              descriptor,
              failure.Location,
              fact.Type.ErrorDisplay,
              failure.Detail));
        }

        foreach (DeclaredHandlerFact declared in fact.DeclaredHandlers)
        {
          if (!Allows(declared.Handler, reported, generatorDiagnostics, referenced.HostAssemblyName)
              || !Allows(declared.Response, reported, generatorDiagnostics, referenced.HostAssemblyName))
          {
            dropped.Add(fact.Type.Key);
            continue;
          }

          if (AlreadyListed(handlersByRequest, fact.Type.Key, declared.Handler))
          {
            continue;
          }

          AddHandler(handlersByRequest, fact.Type, declared.Handler);
          if (!responseByRequest.ContainsKey(fact.Type.Key))
          {
            responseByRequest[fact.Type.Key] = declared.Response;
            if (declared.IsVoid)
            {
              voidRequests.Add(fact.Type.Key);
            }
          }
        }
      }
    }

    foreach (List<TypeModel> list in handlersByRequest.Values)
    {
      SortByDisplay(list);
    }

    foreach (List<TypeModel> list in handlersByNotification.Values)
    {
      SortByDisplay(list);
    }

    List<BehaviorFact> behaviors = new();
    behaviors.AddRange(host.Behaviors);
    behaviors.AddRange(referenced.Behaviors);
    behaviors.Sort(CompareBehaviors);
    List<OrderedBehavior> accepted = new();
    int sourceIndex = 0;
    foreach (BehaviorFact behavior in behaviors)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!members.Contains(behavior.AssemblyName))
      {
        continue;
      }

      if (behavior.Pipelines.Length == 0)
      {
        generatorDiagnostics.Add(Info(
            DiagnosticDescriptors.InvalidBehaviorRegistration,
            behavior.Location,
            behavior.Definition.ErrorDisplay));
        continue;
      }

      if (!Allows(behavior.Definition, reported, generatorDiagnostics, referenced.HostAssemblyName))
      {
        continue;
      }

      accepted.Add(new OrderedBehavior(behavior, sourceIndex));
      sourceIndex++;
    }

    // Explicit Order is the weave order. Source index is only the tie-break, assigned in
    // assembly-name then GetAttributes order above.
    accepted.Sort(CompareOrdered);

    HashSet<int> woven = new();
    foreach (TypeModel requestType in requests.Values)
    {
      if (!requestType.IsDiscoverable || requestType.Arity > 0)
      {
        continue;
      }

      if (!members.Contains(requestType.AssemblyName) || dropped.Contains(requestType.Key))
      {
        continue;
      }

      handlersByRequest.TryGetValue(requestType.Key, out List<TypeModel>? handlerList);
      int handlerCount = handlerList?.Count ?? 0;
      if (handlerCount == 0)
      {
        if (declarationCovers.Contains(requestType.Key))
        {
          continue;
        }

        diagnostics.Add(Info(
            DiagnosticDescriptors.RequestHasNoHandler,
            requestType.Location,
            requestType.ErrorDisplay));
      }
      else if (handlerCount > 1 && handlerList is not null)
      {
        string names = string.Join(", ", handlerList.Select(handler => handler.ErrorDisplay));
        diagnostics.Add(Info(
            DiagnosticDescriptors.DuplicateHandler,
            handlerList[1].Location,
            requestType.ErrorDisplay,
            names));
      }
    }

    List<EmissionRequest> requestBindings = new();
    foreach (KeyValuePair<string, List<TypeModel>> pair in handlersByRequest.OrderBy(pair => RequestDisplay(pair.Key, requests), StringComparer.Ordinal))
    {
      if (pair.Value.Count != 1 || !responseByRequest.TryGetValue(pair.Key, out TypeModel? responseType))
      {
        continue;
      }

      TypeModel requestType = RequestModel(pair.Key, requests, responseByRequest);
      TypeModel handlerType = pair.Value[0];
      TypeModel? handlerScope = ScopeOf(handlerType, byKey, scopeByMessage);
      TypeModel? requestScope = ScopeOf(requestType, byKey, scopeByMessage);
      ReportScopeMismatch(handlerType, handlerScope, requestType, requestScope, diagnostics);
      TypeModel? scopeType = handlerScope ?? requestScope;
      if (!PipelineScopeAllowed(scopeType, reported, generatorDiagnostics, referenced.HostAssemblyName))
      {
        continue;
      }

      ImmutableArray<TypeModel> closed = CloseBehaviors(
          accepted,
          requestType,
          responseType,
          scopeType,
          woven);
      requestBindings.Add(new EmissionRequest(
          requestType,
          responseType,
          handlerType,
          voidRequests.Contains(requestType.Key),
          closed,
          scopeType));
    }

    foreach (OrderedBehavior behavior in accepted)
    {
      if (woven.Contains(behavior.SourceIndex))
      {
        continue;
      }

      string scopeName = behavior.Behavior.Scope?.ErrorDisplay ?? "unscoped";
      generatorDiagnostics.Add(Info(
          DiagnosticDescriptors.BehaviorAppliesToNoRequest,
          behavior.Behavior.Location,
          behavior.Behavior.Definition.ErrorDisplay,
          scopeName));
    }

    List<NotificationGroup> groups = new();
    foreach (KeyValuePair<string, List<TypeModel>> pair in handlersByNotification)
    {
      TypeModel notification = NotificationModel(pair.Key, byKey, pair.Value);
      TypeModel? notificationScope = ScopeOf(notification, byKey, scopeByMessage);
      foreach (TypeModel handler in pair.Value)
      {
        TypeModel? handlerScope = ScopeOf(handler, byKey, scopeByMessage);
        ReportScopeMismatch(handler, handlerScope, notification, notificationScope, diagnostics);
        TypeModel? scopeType = handlerScope ?? notificationScope;
        if (!PipelineScopeAllowed(scopeType, reported, generatorDiagnostics, referenced.HostAssemblyName))
        {
          continue;
        }

        NotificationGroup? bucket = null;
        foreach (NotificationGroup candidate in groups)
        {
          if (candidate.Notification.Key == notification.Key && TypeModels.Same(candidate.Scope, scopeType))
          {
            bucket = candidate;
            break;
          }
        }

        if (bucket is null)
        {
          bucket = new NotificationGroup(notification, scopeType);
          groups.Add(bucket);
        }

        bucket.Handlers.Add(handler);
      }
    }

    groups.Sort((left, right) =>
    {
      int name = string.CompareOrdinal(left.Notification.Display, right.Notification.Display);
      if (name != 0)
      {
        return name;
      }

      return string.CompareOrdinal(left.Scope?.Display ?? string.Empty, right.Scope?.Display ?? string.Empty);
    });

    List<EmissionNotification> notificationBindings = new();
    foreach (NotificationGroup group in groups)
    {
      notificationBindings.Add(new EmissionNotification(
          group.Notification,
          group.Handlers.ToImmutableArray(),
          group.Scope));
    }

    List<string> memberNames = new();
    memberNames.AddRange(members);
    memberNames.Sort(StringComparer.Ordinal);
    EmissionGraph graph = new(
        requestBindings.ToImmutableArray(),
        notificationBindings.ToImmutableArray(),
        options.GeneratedNamespace,
        memberNames.ToImmutableArray());

    cancellationToken.ThrowIfCancellationRequested();
    List<GeneratedSource> sources = new();
    MediatorEmitter.Emit(sources, graph);
    ManifestEmitter.Emit(sources, graph);
    diagnostics.AddRange(generatorDiagnostics);
    return new MediatorModel(
        new EquatableArray<GeneratedSource>(sources.ToImmutableArray()),
        new EquatableArray<DiagnosticInfo>(diagnostics.ToImmutableArray()));
  }

}
