// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Pipeline;

/// <summary>
/// Behavior for executing all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> instances after handling the request
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class RequestPostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> PostProcessors;

  /// <summary>
  /// Initializes a new instance of the <see cref="RequestPostProcessorBehavior{TRequest,TResponse}"/> class.
  /// </summary>
  /// <param name="postProcessors">Post-processors to run, in order, after the handler returns</param>
  public RequestPostProcessorBehavior(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> postProcessors)
      => PostProcessors = postProcessors;

  /// <inheritdoc/>
  public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
  {
    TResponse? response = await next(request, cancellationToken).ConfigureAwait(false);

    foreach (IRequestPostProcessor<TRequest, TResponse> processor in PostProcessors)
    {
      await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
    }

    return response;
  }
}