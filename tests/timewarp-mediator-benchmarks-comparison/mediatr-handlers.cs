#region Purpose
// MediatR Send on the same Ping as TimeWarpGeneratedInterfaceSend.
#endregion

#region Design
// MediatR 14.2.0 handlers return Task. The row keeps that contract, so its allocation
// includes the Task. The handler body otherwise matches PingHandler.
#endregion

#if !BENCHMARK_AOT
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MediatorBenchmarks;

public sealed class MediatRPingHandler : IRequestHandler<Ping, Pong>
{
  public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
  {
    return Task.FromResult(new Pong { Message = request.Message });
  }
}
#endif
