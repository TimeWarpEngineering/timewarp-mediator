#region Purpose
// Woven behavior that records each request it runs on, to show declared requests use the pipeline.
#endregion

namespace TimeWarp.Mediator.Examples.GeneratedRequests;

public sealed class TraceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
  public static readonly List<string> Seen = new();

  public ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
  {
    Seen.Add(typeof(TRequest).DeclaringType?.Name ?? typeof(TRequest).Name);
    return next(request, cancellationToken);
  }
}
