// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class JingHandler : IRequestHandler<Jing>
{
  private readonly TextWriter TextWriter;

  public JingHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public ValueTask Handle(Jing request, CancellationToken cancellationToken)
  {
    return new ValueTask(TextWriter.WriteLineAsync($"--- Handled Jing: {request.Message}, no Jong"));
  }
}