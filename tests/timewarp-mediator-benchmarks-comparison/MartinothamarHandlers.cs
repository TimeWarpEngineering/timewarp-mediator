#region Purpose
// Parallel Ping/Pong against martinothamar Mediator for an honest gap number.
#endregion

using System.Threading;
using System.Threading.Tasks;
using Mediator;

namespace MartinothamarBenchmarks;

public sealed class MartinPing : IRequest<MartinPong>
{
    public string Message { get; set; } = "hi";
}

public sealed class MartinPong
{
    public string Message { get; set; } = string.Empty;
}

public sealed class MartinPingHandler : IRequestHandler<MartinPing, MartinPong>
{
    public ValueTask<MartinPong> Handle(MartinPing request, CancellationToken cancellationToken)
    {
        return new ValueTask<MartinPong>(new MartinPong { Message = request.Message });
    }
}
