// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Benchmarks;

public class GenericRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : notnull
{
  private readonly TextWriter TextWriter;

  public GenericRequestPostProcessor(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("- All Done");
  }
}