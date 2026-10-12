// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class ConstrainedRequestPostProcessor<TRequest, TResponse>
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : Ping
{
  private readonly TextWriter TextWriter;

  public ConstrainedRequestPostProcessor(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("- All Done with Ping");
  }
}