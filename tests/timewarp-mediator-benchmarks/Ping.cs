// Modified by Steven T. Cramer
using System.Threading;
using System.Threading.Tasks;

namespace TimeWarp.Mediator.Benchmarks
{
    public class Ping : IRequest
    {
        public string Message { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping>
    {
        public Task Handle(Ping request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}