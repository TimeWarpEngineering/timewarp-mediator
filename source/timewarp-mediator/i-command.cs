#region Purpose
// CQRS command markers used by TimeWarp.Architecture nested command types.
#endregion

#region Design
// Markers only. Handlers implement IRequestHandler. Dispatch does not branch on ICommand.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// A void command request.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// A command request with a response.
/// </summary>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
