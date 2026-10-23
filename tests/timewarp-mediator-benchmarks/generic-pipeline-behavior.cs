// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Benchmarks;

public class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  private readonly TextWriter TextWriter;

  public GenericPipelineBehavior(TextWriter writer)
  {
    TextWriter = writer;
  }

  public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
  {
    await TextWriter.WriteLineAsync("-- Handling Request");
    TResponse response = await next(request, cancellationToken);
    await TextWriter.WriteLineAsync("-- Finished Request");
    return response;
  }
}