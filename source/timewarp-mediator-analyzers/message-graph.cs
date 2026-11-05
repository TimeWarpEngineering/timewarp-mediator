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
      Membership membership,
      ImmutableArray<DeclaredHandler> declaredHandlers,
      ImmutableArray<Diagnostic> declaredHandlerDiagnostics)
  {
    Requests = requests;
    Notifications = notifications;
    Diagnostics = diagnostics;
    GeneratorDiagnostics = generatorDiagnostics;
    Membership = membership;
    DeclaredHandlers = declaredHandlers;
    DeclaredHandlerDiagnostics = declaredHandlerDiagnostics;
  }

  public ImmutableArray<RequestBinding> Requests { get; }

  public ImmutableArray<NotificationBinding> Notifications { get; }

  /// <summary>
  /// TWM001–TWM003: reported by the generator in a generator host, by the analyzer everywhere else.
  /// </summary>
  public ImmutableArray<Diagnostic> Diagnostics { get; }

  /// <summary>
  /// TWM007, TWM010, and TWM011: shapes the generated mediator drops. Reported by the generator only.
  /// </summary>
  public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

  public Membership Membership { get; }

  /// <summary>
  /// Handlers named by <c>GeneratedHandlerAttribute</c> on a request. HandlerType is null
  /// when that type is not in the compilation.
  /// </summary>
  public ImmutableArray<DeclaredHandler> DeclaredHandlers { get; }

  /// <summary>
  /// TWM012. Reported by the analyzer after every generator has run, including in a generator host.
  /// </summary>
  public ImmutableArray<Diagnostic> DeclaredHandlerDiagnostics { get; }
}

/// <summary>
/// A request's declared generated handler.
/// </summary>
public sealed class DeclaredHandler
{
  /// <summary>
  /// Initializes a declared handler fact.
  /// </summary>
  public DeclaredHandler(
      INamedTypeSymbol requestType,
      string relativeName,
      string resolvedDisplay,
      INamedTypeSymbol? handlerType,
      Location location)
  {
    RequestType = requestType;
    RelativeName = relativeName;
    ResolvedDisplay = resolvedDisplay;
    HandlerType = handlerType;
    Location = location;
  }

  /// <summary>
  /// Request the attribute is applied to.
  /// </summary>
  public INamedTypeSymbol RequestType { get; }

  /// <summary>
  /// Handler type name as written on the attribute, relative to the request's containing type.
  /// </summary>
  public string RelativeName { get; }

  /// <summary>
  /// Display name of the handler the relative name resolves to.
  /// </summary>
  public string ResolvedDisplay { get; }

  /// <summary>
  /// Resolved handler, or null when the type is not in this compilation.
  /// </summary>
  public INamedTypeSymbol? HandlerType { get; }

  /// <summary>
  /// Attribute application, when it is syntax in this compilation.
  /// </summary>
  public Location Location { get; }
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