#region Purpose
// Idempotency marker shared by IQuery and IIdempotentCommand. Types only; no key enforcement or dedup store.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Marker for a request that is safe to retry: repeating it produces no additional side effects
/// beyond the first successful execution.
/// </summary>
public interface IIdempotent
{
}
