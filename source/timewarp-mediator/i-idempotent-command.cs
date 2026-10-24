#region Purpose
// Idempotent command markers. Dispatch is identical to ICommand; idempotency is a contract only.
#endregion

#region Design
// Markers only. Handlers implement IRequestHandler. IIdempotent is data for tooling, not a dispatch rule.
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
