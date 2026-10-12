// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Benchmarks;

public class GenericRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
  private readonly TextWriter TextWriter;

  public GenericRequestPreProcessor(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Process(TRequest request, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("- Starting Up");
  }
}