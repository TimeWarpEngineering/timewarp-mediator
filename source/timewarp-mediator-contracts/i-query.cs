#region Purpose
// CQRS query marker and handler used by TimeWarp.Architecture nested IQuery types.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// A query request with a response.
/// </summary>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Handles a query.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
