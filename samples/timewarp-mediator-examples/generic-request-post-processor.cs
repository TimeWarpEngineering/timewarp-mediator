// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class GenericRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
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