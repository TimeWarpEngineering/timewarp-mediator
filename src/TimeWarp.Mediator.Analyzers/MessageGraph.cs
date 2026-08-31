#region Purpose
// Immutable IR of requests, handlers, notifications, and ordered behaviors for verify + emit.
#endregion

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TimeWarp.Mediator.Analyzers;

public sealed class MessageGraph
{
    public MessageGraph(
        ImmutableArray<RequestBinding> requests,
        ImmutableArray<NotificationBinding> notifications,
        ImmutableArray<Diagnostic> diagnostics,
        Membership membership)
    {
        Requests = requests;
        Notifications = notifications;
        Diagnostics = diagnostics;
        Membership = membership;
    }

    public ImmutableArray<RequestBinding> Requests { get; }

    public ImmutableArray<NotificationBinding> Notifications { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public Membership Membership { get; }

    public bool IsAotProfile =>
        string.Equals(Membership.Profile, "Aot", System.StringComparison.OrdinalIgnoreCase)
        || string.Equals(Membership.Profile, "Link", System.StringComparison.OrdinalIgnoreCase);
}

public sealed class RequestBinding
{
    public RequestBinding(
        INamedTypeSymbol requestType,
        INamedTypeSymbol responseType,
        INamedTypeSymbol handlerType,
        bool isUnitResponse,
        ImmutableArray<INamedTypeSymbol> closedBehaviors)
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerType = handlerType;
        IsUnitResponse = isUnitResponse;
        ClosedBehaviors = closedBehaviors;
    }

    public INamedTypeSymbol RequestType { get; }

    public INamedTypeSymbol ResponseType { get; }

    public INamedTypeSymbol HandlerType { get; }

    public bool IsUnitResponse { get; }

    public ImmutableArray<INamedTypeSymbol> ClosedBehaviors { get; }
}

public sealed class NotificationBinding
{
    public NotificationBinding(
        INamedTypeSymbol notificationType,
        ImmutableArray<INamedTypeSymbol> handlerTypes)
    {
        NotificationType = notificationType;
        HandlerTypes = handlerTypes;
    }

    public INamedTypeSymbol NotificationType { get; }

    public ImmutableArray<INamedTypeSymbol> HandlerTypes { get; }
}

public sealed class BehaviorRegistration
{
    public BehaviorRegistration(INamedTypeSymbol behaviorType, int order, int sourceIndex)
    {
        BehaviorType = behaviorType;
        Order = order;
        SourceIndex = sourceIndex;
    }

    public INamedTypeSymbol BehaviorType { get; }

    public int Order { get; }

    public int SourceIndex { get; }
}
