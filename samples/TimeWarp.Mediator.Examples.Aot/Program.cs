#region Purpose
// AOT/trim-analyzer-clean host that uses the generated Mediator with ServiceGen static fields.
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;

[assembly: MediatorAssembly]

internal static class Program
{
    private static async Task<int> Main()
    {
        Mediator mediator = new();
        Pong pong = await mediator.Send(new Ping("aot"));
        object? boxed = await mediator.Send((object)new Ping("switch"));
        if (pong.Message != "aot-pong" || boxed is not Pong boxedPong || boxedPong.Message != "switch-pong")
        {
            return 1;
        }

        Console.WriteLine(pong.Message);
        return 0;
    }
}

public sealed class Ping : IRequest<Pong>
{
    public Ping(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

public sealed class Pong
{
    public Pong(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Pong(request.Message + "-pong"));
    }
}
