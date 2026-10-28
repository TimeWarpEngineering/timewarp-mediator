extern alias scopedmartin;

#region Purpose
// Scoped Ping/Pong dispatched through ISender<ClientPipeline>. The handler takes
// BenchmarkPeers.ScopeToken, the same scoped dependency as MartinothamarScopedSend.
#endregion

#region Design
// Scope is the marker, not a behavior chain. The handler takes ScopeToken so the row
// includes one scoped dependency, the same type MartinothamarScopedSend injects.
// The State pipeline row is a different request.
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
  private readonly scopedmartin::BenchmarkPeers.ScopeToken ScopeToken;

  public ScopedPingHandler(scopedmartin::BenchmarkPeers.ScopeToken scopeToken)
  {
    ScopeToken = scopeToken;
  }

  public ValueTask<Pong> Handle(ScopedPing request, CancellationToken cancellationToken)
  {
    _ = ScopeToken;
    return ValueTask.FromResult(new Pong { Message = request.Message });
  }
}
