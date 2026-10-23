// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class PingHandler : IRequestHandler<Ping, Pong>
{
  private readonly TextWriter TextWriter;

  public PingHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public async ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
  {
    await TextWriter.WriteLineAsync($"--- Handled Ping: {request.Message}");
    return new Pong { Message = request.Message + " Pong" };
  }
}