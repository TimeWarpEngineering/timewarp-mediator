#region Purpose
// Idempotency marker shared by IQuery and IIdempotentCommand. Types only; no key enforcement or dedup store.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Marker for a request that is safe to retry: repeating it produces no additional side effects
/// beyond the first successful execution.
/// </summary>
/// <remarks>
/// Idempotency is a declared contract only. TimeWarp.Mediator does not enforce it: there is no
/// idempotency key, deduplication store, or retry logic, and dispatch is identical to a non-idempotent request.
/// </remarks>
public interface IIdempotent
{
}
