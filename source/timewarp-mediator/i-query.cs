#region Purpose
// CQRS query marker used by TimeWarp.Architecture nested query types.
#endregion

#region Design
// Marker only. Handlers implement IRequestHandler. IIdempotent is data for tooling, not a dispatch rule.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// A read-only query request with a response. Queries are idempotent by definition.
/// </summary>
/// <remarks>
/// Inherits <see cref="IIdempotent"/>. Neither read-only behavior nor idempotency is enforced: there is no
/// idempotency key, deduplication store, or retry logic, and dispatch is identical to <see cref="IRequest{TResponse}"/>.
/// </remarks>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>, IIdempotent
{
}
