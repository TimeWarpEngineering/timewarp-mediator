#region Purpose
// Immutable IR of requests, handlers, notifications, and ordered behaviors for verify + emit.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public sealed class MessageGraph
{
  public MessageGraph(
      ImmutableArray<RequestBinding> requests,
      ImmutableArray<NotificationBinding> notifications,
      ImmutableArray<Diagnostic> diagnostics,
      ImmutableArray<Diagnostic> generatorDiagnostics,
      Membership membership)
  {
    Requests = requests;
    Notifications = notifications;
    Diagnostics = diagnostics;
    GeneratorDiagnostics = generatorDiagnostics;
    Membership = membership;
  }

  public ImmutableArray<RequestBinding> Requests { get; }

  public ImmutableArray<NotificationBinding> Notifications { get; }

  /// <summary>
  /// TWM001–TWM003: reported by the generator in a generator host, by the analyzer everywhere else.
  /// </summary>
  public ImmutableArray<Diagnostic> Diagnostics { get; }

  /// <summary>
  /// TWM007, TWM009–TWM011: shapes the generated mediator drops. Reported by the generator only.
  /// </summary>
  public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

  public Membership Membership { get; }

  public bool IsAotProfile =>
      string.Equals(Membership.Profile, "Aot", System.StringComparison.OrdinalIgnoreCase)
      || string.Equals(Membership.Profile, "Link", System.StringComparison.OrdinalIgnoreCase);
}

public sealed class RequestBinding
{
  public RequestBinding(
      INamedTypeSymbol requestType,
      ITypeSymbol responseType,
      INamedTypeSymbol handlerType,
      bool isUnitResponse,
      ImmutableArray<INamedTypeSymbol> closedBehaviors,
      INamedTypeSymbol? scopeType)
  {
    RequestType = requestType;
    ResponseType = responseType;
    HandlerType = handlerType;
    IsUnitResponse = isUnitResponse;
    ClosedBehaviors = closedBehaviors;
    ScopeType = scopeType;
  }

  public INamedTypeSymbol RequestType { get; }

  public ITypeSymbol ResponseType { get; }

  public INamedTypeSymbol HandlerType { get; }

  public bool IsUnitResponse { get; }

  public ImmutableArray<INamedTypeSymbol> ClosedBehaviors { get; }

  /// <summary>
  /// Pipeline marker type, or null for the unscoped default pipeline.
  /// </summary>
  public INamedTypeSymbol? ScopeType { get; }
}

public sealed class NotificationBinding
{
  public NotificationBinding(
      INamedTypeSymbol notificationType,
      ImmutableArray<INamedTypeSymbol> handlerTypes,
      INamedTypeSymbol? scopeType)
  {
    NotificationType = notificationType;
    HandlerTypes = handlerTypes;
    ScopeType = scopeType;
  }

  public INamedTypeSymbol NotificationType { get; }

  public ImmutableArray<INamedTypeSymbol> HandlerTypes { get; }

  /// <summary>
  /// Pipeline marker type, or null for the unscoped default pipeline.
  /// </summary>
  public INamedTypeSymbol? ScopeType { get; }
}

public sealed class BehaviorRegistration
{
  public BehaviorRegistration(
      INamedTypeSymbol behaviorType,
      int order,
      int sourceIndex,
      INamedTypeSymbol? scopeType,
      Location location)
  {
    BehaviorType = behaviorType;
    Order = order;
    SourceIndex = sourceIndex;
    ScopeType = scopeType;
    Location = location;
  }

  public INamedTypeSymbol BehaviorType { get; }

  public int Order { get; }

  public int SourceIndex { get; }

  /// <summary>
  /// Pipeline marker type, or null for the unscoped default pipeline.
  /// </summary>
  public INamedTypeSymbol? ScopeType { get; }

  /// <summary>
  /// The [assembly: MediatorBehavior] attribute when it is in this compilation, else Location.None.
  /// </summary>
  public Location Location { get; }
}