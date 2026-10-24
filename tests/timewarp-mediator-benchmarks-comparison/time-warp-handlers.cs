#region Purpose
// Closed Ping/Pong used by the generated Mediator and CallSiteInlining.
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

public sealed class Ping : IRequest<Pong>
{
  public string Message { get; set; } = "hi";
}

public sealed class Pong
{
  public string Message { get; set; } = string.Empty;
}

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
  public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(new Pong { Message = request.Message });
  }
}

public sealed class Nop : IRequest
{
}

public sealed class NopHandler : IRequestHandler<Nop>
{
  public ValueTask Handle(Nop request, CancellationToken cancellationToken)
  {
    return default;
  }
}
