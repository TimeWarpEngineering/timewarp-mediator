#region Purpose
// CQRS command markers and handlers used by TimeWarp.Architecture nested ICommand types.
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

/// <summary>
/// Handles a void command.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

/// <summary>
/// Handles a command with a response.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
