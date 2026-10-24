#region Purpose
// Builds the emission graph and diagnostics from equatable host and referenced-assembly facts.
#endregion

#region Design
// Same decisions as MessageGraphBuilder: member assemblies by name, handlers by display name,
// one binding per request, inherited handler interfaces dropped, behaviors closed by constraint.
// The analyzer keeps the symbol walk. This path exists so the generator model step does not take
// a Compilation. Display strings were copied from symbols at the syntax and metadata boundary,
// so the emitters still print the same text.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static class CandidateGraph
{
  internal static MediatorModel Create(HostInput host, ReferencedInput referenced, MembershipOptions options, CancellationToken cancellationToken)
  {
    if (referenced.MissingRuntime.Length > 0)
    {
      string metadataName = First(referenced.MissingRuntime);
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

    List<TypeFact> facts = new();
    facts.AddRange(host.Types);
    facts.AddRange(referenced.Types);
    Dictionary<string, TypeFact> byKey = new(StringComparer.Ordinal);
    foreach (TypeFact fact in facts)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!byKey.ContainsKey(fact.Type.Key))
      {
        byKey.Add(fact.Type.Key, fact);
      }
    }

    HashSet<string> members = new(StringComparer.Ordinal);
    if (options.HostAssemblyName.Length > 0)
    {
      members.Add(options.HostAssemblyName);
    }

    foreach (string name in referenced.MemberAssemblyNames)
    {
      members.Add(name);
    }

    List<DiagnosticInfo> diagnostics = new();
    List<DiagnosticInfo> generatorDiagnostics = new();
    HashSet<string> reported = new(StringComparer.Ordinal);
    HashSet<string> dropped = new(StringComparer.Ordinal);
    Dictionary<string, List<TypeModel>> handlersByRequest = new(StringComparer.Ordinal);
    Dictionary<string, TypeModel> responseByRequest = new(StringComparer.Ordinal);
    HashSet<string> voidRequests = new(StringComparer.Ordinal);
    Dictionary<string, List<TypeModel>> handlersByNotification = new(StringComparer.Ordinal);
    Dictionary<string, TypeModel> requests = new(StringComparer.Ordinal);

    foreach (TypeFact fact in byKey.Values)
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

      foreach (HandlerMatch match in fact.Matches)
      {
        if (BoundByBase(fact.Type, match, byKey, members))
        {
          continue;
        }

        if (!Allows(fact.Type, reported, generatorDiagnostics, options)
            || !Allows(match.Message, reported, generatorDiagnostics, options)
            || (match.Response is not null && !Allows(match.Response, reported, generatorDiagnostics, options)))
        {
          dropped.Add(match.Message.Key);
          continue;
        }

        TypeModel message = Prefer(match.Message, byKey);
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

      if (!Allows(behavior.Definition, reported, generatorDiagnostics, options))
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
      TypeModel? handlerScope = ScopeOf(handlerType, byKey);
      TypeModel? requestScope = ScopeOf(requestType, byKey);
      ReportScopeMismatch(handlerType, handlerScope, requestType, requestScope, diagnostics);
      TypeModel? scopeType = handlerScope ?? requestScope;
      if (!PipelineScopeAllowed(scopeType, reported, generatorDiagnostics, options))
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
      TypeModel? notificationScope = ScopeOf(notification, byKey);
      foreach (TypeModel handler in pair.Value)
      {
        TypeModel? handlerScope = ScopeOf(handler, byKey);
        ReportScopeMismatch(handler, handlerScope, notification, notificationScope, diagnostics);
        TypeModel? scopeType = handlerScope ?? notificationScope;
        if (!PipelineScopeAllowed(scopeType, reported, generatorDiagnostics, options))
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

  private static TypeModel RequestModel(
      string key,
      Dictionary<string, TypeModel> requests,
      Dictionary<string, TypeModel> responseByRequest)
  {
    if (requests.TryGetValue(key, out TypeModel? request))
    {
      return request;
    }

    return responseByRequest[key];
  }

  private static string RequestDisplay(string key, Dictionary<string, TypeModel> requests)
  {
    return requests.TryGetValue(key, out TypeModel? request) ? request.Display : key;
  }

  private static TypeModel NotificationModel(string key, Dictionary<string, TypeFact> facts, List<TypeModel> handlers)
  {
    if (facts.TryGetValue(key, out TypeFact? fact))
    {
      return fact.Type;
    }

    foreach (TypeFact candidate in facts.Values)
    {
      foreach (HandlerMatch match in candidate.Matches)
      {
        if (match.IsNotification && match.Message.Key == key)
        {
          return match.Message;
        }
      }
    }

    return handlers[0];
  }

  private static TypeModel Prefer(TypeModel model, Dictionary<string, TypeFact> facts)
  {
    return facts.TryGetValue(model.Key, out TypeFact? fact) ? fact.Type : model;
  }

  private static void AddHandler(Dictionary<string, List<TypeModel>> handlers, TypeModel message, TypeModel handler)
  {
    if (!handlers.TryGetValue(message.Key, out List<TypeModel>? list))
    {
      list = new List<TypeModel>();
      handlers.Add(message.Key, list);
    }

    list.Add(handler);
  }

  private static bool BoundByBase(
      TypeModel handler,
      HandlerMatch match,
      Dictionary<string, TypeFact> facts,
      HashSet<string> members)
  {
    foreach (string baseKey in handler.BaseKeys)
    {
      if (!facts.TryGetValue(baseKey, out TypeFact? baseFact))
      {
        continue;
      }

      TypeModel baseType = baseFact.Type;
      if (!baseType.IsClassOrStruct
          || baseType.IsAbstract
          || baseType.Arity != 0
          || !baseType.IsDiscoverable
          || !members.Contains(baseType.AssemblyName))
      {
        continue;
      }

      foreach (HandlerMatch baseMatch in baseFact.Matches)
      {
        string baseResponse = baseMatch.Response is null ? string.Empty : baseMatch.Response.Key;
        string matchResponse = match.Response is null ? string.Empty : match.Response.Key;
        if (baseMatch.IsNotification == match.IsNotification
            && baseMatch.IsVoid == match.IsVoid
            && baseMatch.Message.Key == match.Message.Key
            && baseResponse == matchResponse)
        {
          return true;
        }
      }
    }

    return false;
  }

  private static TypeModel? ScopeOf(TypeModel type, Dictionary<string, TypeFact> facts)
  {
    if (facts.TryGetValue(type.Key, out TypeFact? fact))
    {
      return fact.Scope;
    }

    return null;
  }

  private static void ReportScopeMismatch(
      TypeModel handlerType,
      TypeModel? handlerScope,
      TypeModel messageType,
      TypeModel? messageScope,
      List<DiagnosticInfo> diagnostics)
  {
    if (handlerScope is null || messageScope is null || handlerScope.Key == messageScope.Key)
    {
      return;
    }

    diagnostics.Add(Info(
        DiagnosticDescriptors.ScopeMismatch,
        handlerType.Location,
        handlerType.ErrorDisplay,
        handlerScope.ErrorDisplay,
        messageType.ErrorDisplay,
        messageScope.ErrorDisplay));
  }

  private static bool PipelineScopeAllowed(
      TypeModel? scopeType,
      HashSet<string> reported,
      List<DiagnosticInfo> generatorDiagnostics,
      MembershipOptions options)
  {
    return scopeType is null || Allows(scopeType, reported, generatorDiagnostics, options);
  }

  private static bool Allows(
      TypeModel type,
      HashSet<string> reported,
      List<DiagnosticInfo> generatorDiagnostics,
      MembershipOptions options)
  {
    TypeModel? inaccessible = type.InaccessiblePart ?? (type.Accessible ? null : type);
    if (inaccessible is null)
    {
      return true;
    }

    if (reported.Add(inaccessible.Key))
    {
      generatorDiagnostics.Add(Info(
          DiagnosticDescriptors.InaccessibleMemberType,
          inaccessible.Location ?? type.Location,
          inaccessible.ErrorDisplay,
          inaccessible.AssemblyName,
          options.HostAssemblyName));
    }

    return false;
  }

  private static ImmutableArray<TypeModel> CloseBehaviors(
      List<OrderedBehavior> behaviors,
      TypeModel requestType,
      TypeModel responseType,
      TypeModel? requestScope,
      HashSet<int> woven)
  {
    List<TypeModel> closed = new();
    foreach (OrderedBehavior behavior in behaviors)
    {
      if (!TypeModels.Same(behavior.Behavior.Scope, requestScope))
      {
        continue;
      }

      TypeModel? constructed = TryClose(behavior.Behavior, requestType, responseType);
      if (constructed is null)
      {
        continue;
      }

      closed.Add(constructed);
      woven.Add(behavior.SourceIndex);
    }

    return closed.ToImmutableArray();
  }

  private static TypeModel? TryClose(BehaviorFact behavior, TypeModel requestType, TypeModel responseType)
  {
    if (behavior.Parameters.Length == 0)
    {
      return ImplementsPipeline(behavior, Array.Empty<TypeModel>(), requestType, responseType) ? behavior.Definition : null;
    }

    TypeModel[] arguments;
    if (behavior.Parameters.Length == 2)
    {
      arguments = new[] { requestType, responseType };
    }
    else if (behavior.Parameters.Length == 1)
    {
      arguments = new[] { requestType };
    }
    else
    {
      return null;
    }

    if (!CanConstruct(behavior.Parameters, arguments))
    {
      return null;
    }

    return ImplementsPipeline(behavior, arguments, requestType, responseType)
        ? CloseGeneric(behavior, arguments)
        : null;
  }

  private static bool ImplementsPipeline(
      BehaviorFact behavior,
      TypeModel[] arguments,
      TypeModel requestType,
      TypeModel responseType)
  {
    foreach (PipelineSlot pipeline in behavior.Pipelines)
    {
      string? requestKey = Resolve(pipeline.Request, arguments);
      string? responseKey = Resolve(pipeline.Response, arguments);
      if (requestKey == requestType.Key && responseKey == responseType.Key)
      {
        return true;
      }
    }

    return false;
  }

  private static string? Resolve(TypeArgSlot slot, TypeModel[] arguments)
  {
    if (slot.ParameterIndex >= 0)
    {
      return slot.ParameterIndex < arguments.Length ? arguments[slot.ParameterIndex].Key : null;
    }

    return slot.ConcreteKey;
  }

  private static bool CanConstruct(EquatableArray<ParameterSlot> parameters, TypeModel[] arguments)
  {
    if (parameters.Length != arguments.Length)
    {
      return false;
    }

    for (int i = 0; i < parameters.Length; i++)
    {
      ParameterSlot parameter = parameters.ElementAt(i);
      TypeModel argument = arguments[i];
      if (parameter.ReferenceType && !argument.IsReferenceType)
      {
        return false;
      }

      if (parameter.ValueType && !argument.IsValueType)
      {
        return false;
      }

      if (parameter.Unmanaged && !argument.IsUnmanagedType)
      {
        return false;
      }

      foreach (ConstraintSlot constraint in parameter.Constraints)
      {
        if (!ImplementsOrInherits(argument, constraint))
        {
          return false;
        }
      }

      if (parameter.Constructor)
      {
        if (!argument.IsNamedType || argument.IsAbstract || !argument.HasPublicParameterlessConstructor)
        {
          return false;
        }
      }
    }

    return true;
  }

  private static bool ImplementsOrInherits(TypeModel type, ConstraintSlot constraint)
  {
    if (type.Key == constraint.ConcreteKey || type.DefinitionKey == constraint.DefinitionKey)
    {
      return true;
    }

    for (int i = 0; i < type.BaseKeys.Length; i++)
    {
      if (KeyAt(type.BaseKeys, i) == constraint.ConcreteKey || KeyAt(type.BaseDefinitionKeys, i) == constraint.DefinitionKey)
      {
        return true;
      }
    }

    for (int i = 0; i < type.InterfaceKeys.Length; i++)
    {
      if (KeyAt(type.InterfaceKeys, i) == constraint.ConcreteKey
          || KeyAt(type.InterfaceDefinitionKeys, i) == constraint.DefinitionKey)
      {
        return true;
      }
    }

    return false;
  }

  private static string KeyAt(EquatableArray<string> keys, int index)
  {
    int current = 0;
    foreach (string key in keys)
    {
      if (current == index)
      {
        return key;
      }

      current++;
    }

    return string.Empty;
  }

  private static TypeModel CloseGeneric(BehaviorFact behavior, TypeModel[] arguments)
  {
    string argumentsFullyQualified = string.Join(", ", arguments.Select(argument => argument.FullyQualified));
    string argumentsDisplay = string.Join(", ", arguments.Select(argument => argument.Display));
    string argumentsError = string.Join(", ", arguments.Select(argument => argument.ErrorDisplay));
    string key = behavior.Definition.AssemblyName + "|closed:" + behavior.Definition.Key + "<" + string.Join(",", arguments.Select(argument => argument.Key)) + ">";
    return behavior.Definition with
    {
      Key = key,
      FullyQualified = behavior.FullyQualifiedPrefix + "<" + argumentsFullyQualified + ">",
      Display = behavior.DisplayPrefix + "<" + argumentsDisplay + ">",
      ErrorDisplay = behavior.ErrorDisplayPrefix + "<" + argumentsError + ">",
      Arity = arguments.Length,
      Location = null,
      InaccessiblePart = null,
      Accessible = true,
    };
  }

  private static int CompareBehaviors(BehaviorFact left, BehaviorFact right)
  {
    int assembly = string.CompareOrdinal(left.AssemblyName, right.AssemblyName);
    if (assembly != 0)
    {
      return assembly;
    }

    int path = string.CompareOrdinal(left.Path, right.Path);
    if (path != 0)
    {
      return path;
    }

    return left.SpanStart.CompareTo(right.SpanStart);
  }

  private static int CompareOrdered(OrderedBehavior left, OrderedBehavior right)
  {
    int order = left.Behavior.Order.CompareTo(right.Behavior.Order);
    if (order != 0)
    {
      return order;
    }

    return left.SourceIndex.CompareTo(right.SourceIndex);
  }

  private static string First(EquatableArray<string> values)
  {
    foreach (string value in values)
    {
      return value;
    }

    return string.Empty;
  }

  private static void SortByDisplay(List<TypeModel> types)
  {
    types.Sort((left, right) => string.CompareOrdinal(left.Display, right.Display));
  }

  private static DiagnosticInfo Info(DiagnosticDescriptor descriptor, LocationInfo? location, params object[] args)
  {
    Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, args);
    return DiagnosticInfo.From(diagnostic) with { Location = location };
  }

  private readonly struct OrderedBehavior
  {
    public OrderedBehavior(BehaviorFact behavior, int sourceIndex)
    {
      Behavior = behavior;
      SourceIndex = sourceIndex;
    }

    public BehaviorFact Behavior { get; }

    public int SourceIndex { get; }
  }

  private sealed class NotificationGroup
  {
    public NotificationGroup(TypeModel notification, TypeModel? scope)
    {
      Notification = notification;
      Scope = scope;
      Handlers = new List<TypeModel>();
    }

    public TypeModel Notification { get; }

    public TypeModel? Scope { get; }

    public List<TypeModel> Handlers { get; }
  }
}
