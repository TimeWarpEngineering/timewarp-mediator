#region Purpose
// Scoped Ping/Pong dispatched through ISender<ClientPipeline> with no behaviors.
#endregion

#region Design
// Scope is the marker, not a behavior chain. The State pipeline row is a different request
// so this row measures named-pipeline interface dispatch alone.
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

[MediatorScope(typeof(ClientPipeline))]
public sealed class ScopedPing : IRequest<Pong>
{
  public string Message { get; set; } = "hi";
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ScopedPingHandler : IRequestHandler<ScopedPing, Pong>
{
  public ValueTask<Pong> Handle(ScopedPing request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(new Pong { Message = request.Message });
  }
}
