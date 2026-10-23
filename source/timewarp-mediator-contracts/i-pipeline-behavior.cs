// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// Continuation for the next handler in the pipeline. The delegate carries the request so a
/// behavior calls <c>next(request, cancellationToken)</c> without closing over the message.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
/// <param name="request">Request passed to the rest of the pipeline.</param>
/// <param name="cancellationToken">Cancellation token for the rest of the pipeline.</param>
/// <returns>A <see cref="ValueTask{TResult}"/> whose result is a <typeparamref name="TResponse"/>.</returns>
public delegate ValueTask<TResponse> MessageHandlerDelegate<TRequest, TResponse>(
    TRequest request,
    CancellationToken cancellationToken);

/// <summary>
/// Pipeline behavior to surround the inner handler.
/// Implementations add additional behavior and await the next delegate.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  /// <summary>
  /// Pipeline handler. Perform any additional behavior and await the <paramref name="next"/> delegate as necessary.
  /// </summary>
  /// <param name="request">Incoming request.</param>
  /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask{TResult}"/> whose result is the <typeparamref name="TResponse"/>.</returns>
  ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken);
}
