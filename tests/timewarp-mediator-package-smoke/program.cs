#region Purpose
// Outside-consumer smoke for the two packed nupkgs: TimeWarp.Mediator and TimeWarp.Mediator.Generators.
#endregion
#region Design
// Compiles only if TimeWarp.Mediator ships the TWM001–TWM004 analyzer and Generators ships a
// working generator plus its buildTransitive props (the generated type lands in the default
// TimeWarp.Mediator.Generated namespace). Runs only if the TimeWarp.Mediator lib/ folder
// for the host framework resolves. dev pack runs this project on net10.0 and net11.0.
// Exit code 0 and a package-pong line are the pass signal.
#endregion

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;

[assembly: MediatorBehavior(typeof(SmokeBehavior<,>))]

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

GeneratedMediatorAttribute? marker = typeof(Mediator).Assembly.GetCustomAttribute<GeneratedMediatorAttribute>();
if (marker is null || marker.MediatorType != typeof(Mediator))
{
  await Console.Error.WriteLineAsync("GeneratedMediatorAttribute is missing or names a different mediator.");
  return 1;
}

MediatorManifest.RequestEntry entry = MediatorManifest.Request(typeof(SmokePing).FullName!);
if (entry.Kind != MediatorManifest.KindRequest || entry.Response != "string")
{
  await Console.Error.WriteLineAsync($"Unexpected manifest entry: {entry.Kind} -> {entry.Response}");
  return 1;
}

string assemblyName = typeof(SmokePing).Assembly.GetName().Name ?? string.Empty;
int membership = MediatorManifest.Json.IndexOf("\"membership\":[", StringComparison.Ordinal);
if (membership < 0 || MediatorManifest.Json.IndexOf("\"" + assemblyName + "\"", membership, StringComparison.Ordinal) < 0)
{
  await Console.Error.WriteLineAsync($"Host '{assemblyName}' is not a manifest member: {MediatorManifest.Json}");
  return 1;
}

bool behaviorWoven = false;
foreach (string behavior in entry.Behaviors)
{
  if (behavior == "SmokeBehavior<SmokePing, string>")
  {
    behaviorWoven = true;
  }
}

if (!behaviorWoven)
{
  await Console.Error.WriteLineAsync("Host behavior SmokeBehavior<SmokePing, string> was not woven.");
  return 1;
}

Console.WriteLine($"package-smoke: {reply} via {typeof(Mediator).FullName}");
return 0;

public sealed record SmokePing(string Message) : IRequest<string>;

public sealed class SmokePingHandler : IRequestHandler<SmokePing, string>
{
  public ValueTask<string> Handle(SmokePing request, CancellationToken cancellationToken) =>
      ValueTask.FromResult(request.Message + "-pong");
}

public sealed class SmokeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
  public ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken) => next(request, cancellationToken);
}