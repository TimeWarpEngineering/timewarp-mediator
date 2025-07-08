// Modified by Steven T. Cramer
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator.Pipeline;

namespace TimeWarp.Mediator.Benchmarks
{
    public class GenericRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly TextWriter _writer;

        public GenericRequestPostProcessor(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("- All Done");
        }
    }
}