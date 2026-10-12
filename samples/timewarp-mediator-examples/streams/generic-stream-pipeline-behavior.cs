// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class GenericStreamPipelineBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
{
  private readonly TextWriter TextWriter;

  public GenericStreamPipelineBehavior(TextWriter writer)
  {
    TextWriter = writer;
  }

  public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    await TextWriter.WriteLineAsync("-- Handling StreamRequest");
    await foreach (TResponse response in next().WithCancellation(cancellationToken).ConfigureAwait(false))
    {
      yield return response;
    }
    await TextWriter.WriteLineAsync("-- Finished StreamRequest");
  }
}