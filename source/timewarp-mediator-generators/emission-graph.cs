#region Purpose
// Symbol-free message graph the emitters render. Same shape as the analyzer's MessageGraph.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed class EmissionGraph
{
  public EmissionGraph(
      ImmutableArray<EmissionRequest> requests,
      ImmutableArray<EmissionNotification> notifications,
      string generatedNamespace,
      ImmutableArray<string> memberAssemblyNames)
  {
    Requests = requests;
    Notifications = notifications;
    GeneratedNamespace = generatedNamespace;
    MemberAssemblyNames = memberAssemblyNames;
  }

  public ImmutableArray<EmissionRequest> Requests { get; }

  public ImmutableArray<EmissionNotification> Notifications { get; }

  public string GeneratedNamespace { get; }

  public ImmutableArray<string> MemberAssemblyNames { get; }
}

internal sealed class EmissionRequest
{
  public EmissionRequest(
      TypeModel requestType,
      TypeModel responseType,
      TypeModel handlerType,
      bool isUnitResponse,
      ImmutableArray<TypeModel> closedBehaviors,
      TypeModel? scopeType)
  {
    RequestType = requestType;
    ResponseType = responseType;
    HandlerType = handlerType;
    IsUnitResponse = isUnitResponse;
    ClosedBehaviors = closedBehaviors;
    ScopeType = scopeType;
  }

  public TypeModel RequestType { get; }

  public TypeModel ResponseType { get; }

  public TypeModel HandlerType { get; }

  public bool IsUnitResponse { get; }

  public ImmutableArray<TypeModel> ClosedBehaviors { get; }

  public TypeModel? ScopeType { get; }
}

internal sealed class EmissionNotification
{
  public EmissionNotification(
      TypeModel notificationType,
      ImmutableArray<TypeModel> handlerTypes,
      TypeModel? scopeType)
  {
    NotificationType = notificationType;
    HandlerTypes = handlerTypes;
    ScopeType = scopeType;
  }

  public TypeModel NotificationType { get; }

  public ImmutableArray<TypeModel> HandlerTypes { get; }

  public TypeModel? ScopeType { get; }
}
