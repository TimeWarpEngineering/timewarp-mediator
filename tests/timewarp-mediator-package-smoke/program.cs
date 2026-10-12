#region Purpose
// Outside-consumer smoke for the packed nupkgs: Contracts + Generators only, no TimeWarp.Mediator.
#endregion
#region Design
// Compiles only if the Generators nupkg ships a working analyzer and its buildTransitive props
// (the generated type lands in the default TimeWarp.Mediator.Generated namespace). Runs only if
// the Contracts lib/ folder resolves for net10.0. Exit code 0 is the pass signal for dev pack.
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;

ServiceCollection services = new();
services.AddGeneratedMediator();
using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();

ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
string reply = await sender.Send(new SmokePing("package"));
if (reply != "package-pong")
{
    await Console.Error.WriteLineAsync($"Unexpected reply: {reply}");
    return 1;
}

Console.WriteLine($"package-smoke: {reply} via {typeof(Mediator).FullName}");
return 0;

public sealed record SmokePing(string Message) : IRequest<string>;

public sealed class SmokePingHandler : IRequestHandler<SmokePing, string>
{
    public Task<string> Handle(SmokePing request, CancellationToken cancellationToken) =>
        Task.FromResult(request.Message + "-pong");
}
