// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Pipeline;

/// <summary>
/// Behavior for executing all <see cref="IRequestPreProcessor{TRequest}"/> instances before handling a request
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class RequestPreProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  private readonly IEnumerable<IRequestPreProcessor<TRequest>> PreProcessors;

  /// <summary>
  /// Initializes a new instance of the <see cref="RequestPreProcessorBehavior{TRequest,TResponse}"/> class.
  /// </summary>
  /// <param name="preProcessors">Pre-processors to run, in order, before the handler</param>
  public RequestPreProcessorBehavior(IEnumerable<IRequestPreProcessor<TRequest>> preProcessors)
      => PreProcessors = preProcessors;

  /// <inheritdoc/>
  public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
  {
    foreach (IRequestPreProcessor<TRequest> processor in PreProcessors)
    {
      await processor.Process(request, cancellationToken).ConfigureAwait(false);
    }

    return await next(request, cancellationToken).ConfigureAwait(false);
  }
}