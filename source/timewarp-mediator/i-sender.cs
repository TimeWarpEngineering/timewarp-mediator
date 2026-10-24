// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// Send a request through the mediator pipeline to be handled by a single handler.
/// </summary>
public interface ISender
{
  /// <summary>
  /// Sends a request to a single handler.
  /// </summary>
  /// <typeparam name="TResponse">Response type.</typeparam>
  /// <param name="request">Request object.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask{TResult}"/> whose result is the handler response.</returns>
  ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

  /// <summary>
  /// Sends a request that has no response value.
  /// </summary>
  /// <typeparam name="TRequest">Request type.</typeparam>
  /// <param name="request">Request object.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask"/> that completes when the handler finishes.</returns>
  ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
      where TRequest : IRequest;

  /// <summary>
  /// Sends an object request to a single handler. The response is type-erased.
  /// </summary>
  /// <param name="request">Request object.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask{TResult}"/> whose result is the type-erased handler response.</returns>
  ValueTask<object?> Send(object request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Send through the named pipeline identified by <typeparamref name="TScope"/>.
/// Unscoped <see cref="ISender"/> is the default pipeline (handlers with no
/// <see cref="MediatorScopeAttribute"/>).
/// </summary>
/// <typeparam name="TScope">Marker type that names the pipeline (for example <c>ClientPipeline</c>).</typeparam>
public interface ISender<TScope> : ISender
{
}
