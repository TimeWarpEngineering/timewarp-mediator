#region Purpose
// Idempotent command markers and handlers. Dispatch is identical to ICommand; idempotency is a contract only.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// A void command that mutates state and is safe to retry.
/// </summary>
/// <remarks>
/// Idempotency is a declared contract only. TimeWarp.Mediator does not enforce it: there is no
/// idempotency key, deduplication store, or retry logic, and dispatch is identical to a non-idempotent request.
/// </remarks>
public interface IIdempotentCommand : ICommand, IIdempotent
{
}

/// <summary>
/// A command with a response that mutates state and is safe to retry.
/// </summary>
/// <remarks>
/// Idempotency is a declared contract only. TimeWarp.Mediator does not enforce it: there is no
/// idempotency key, deduplication store, or retry logic, and dispatch is identical to a non-idempotent request.
/// </remarks>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>, IIdempotent
{
}

/// <summary>
/// Handles a void idempotent command.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
public interface IIdempotentCommandHandler<in TCommand> : ICommandHandler<TCommand>
    where TCommand : IIdempotentCommand
{
}

/// <summary>
/// Handles an idempotent command with a response.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IIdempotentCommandHandler<in TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : IIdempotentCommand<TResponse>
{
}
