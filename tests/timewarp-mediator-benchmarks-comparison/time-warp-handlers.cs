#region Purpose
// Closed Ping/Pong used by legacy MakeGenericType, generated Mediator, and CallSiteInlining.
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
    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Pong { Message = request.Message });
    }
}
