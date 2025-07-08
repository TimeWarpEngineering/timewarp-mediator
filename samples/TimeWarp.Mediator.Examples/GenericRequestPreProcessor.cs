// Modified by Steven T. Cramer
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator.Pipeline;

namespace TimeWarp.Mediator.Examples;

public class GenericRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
{
    private readonly TextWriter _writer;

    public GenericRequestPreProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        return _writer.WriteLineAsync("- Starting Up");
    }
}