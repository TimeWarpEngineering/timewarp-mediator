TimeWarp Mediator
=================

[![CI/CD](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/workflows/workflow.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/workflows/workflow.yml)
[![NuGet](https://img.shields.io/nuget/v/TimeWarp.Mediator.svg)](https://www.nuget.org/packages/TimeWarp.Mediator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TimeWarp.Mediator.svg)](https://www.nuget.org/packages/TimeWarp.Mediator)

TimeWarp Mediator 15.0: in-process messaging for .NET with one source-generated dispatcher.
Handlers return `ValueTask`. The generated path is trim and NativeAOT safe, and pipeline
behaviors are woven. This project is a fork of
[MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard.

14.0.0 on nuget.org is the previous surface. 15.0 is not a drop-in. See
[migration.md](documentation/migration.md).

## Packages

| Package | Use it for |
|---------|------------|
| [TimeWarp.Mediator](https://www.nuget.org/packages/TimeWarp.Mediator) | Message, handler, and pipeline interfaces, `ISender` / `IPublisher` / `IMediator`, membership attributes, and the TWM001–TWM004 analyzer. Needed by every host and message library. |
| [TimeWarp.Mediator.Generators](https://www.nuget.org/packages/TimeWarp.Mediator.Generators) | Source generator for the compile-time mediator (`AddGeneratedMediator()`), `MediatorManifest`, and `[GeneratedMediator]`. |

## Target framework

`TimeWarp.Mediator` targets **net10.0 and net11.0**. The generator targets netstandard2.0 because it runs
inside the compiler. It needs the Roslyn that ships with the .NET 10 SDK or newer
(`Microsoft.CodeAnalysis.CSharp` 5.0.0 is the floor).

## Quick start

Keep both packages on the same version. Generators does not bring `TimeWarp.Mediator` with it.

```bash
dotnet add package TimeWarp.Mediator --version 15.0.0-beta.2
dotnet add package TimeWarp.Mediator.Generators --version 15.0.0-beta.2
```

Without `--version`, `dotnet add package TimeWarp.Mediator` resolves 14.0.0, the reflection
runtime. Drop the flag when 15.0 is the stable package, and keep it equal to `<Version>` until
then.

```csharp
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;

ServiceCollection services = new();
services.AddGeneratedMediator();
using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
string reply = await sender.Send(new Ping());

public sealed class Ping : IRequest<string>
{
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
    ValueTask.FromResult("Pong");
}
```

The generated types and `AddGeneratedMediator()` are `internal` to the host project. Call
registration from the host. Resolve `ISender` from a scope: handlers are scoped. Named
pipelines (`AddGeneratedMediator<TScope>()`):
[m2-named-pipelines.md](documentation/m2-named-pipelines.md). The full contract:
[architecture.md](documentation/architecture.md). Diagnostics:
[diagnostics.md](documentation/diagnostics.md).

## Contracts

`TimeWarp.Mediator` holds the message, handler, and pipeline types:

- `IRequest` / `IRequest<T>`, `INotification`
- Markers: `ICommand` / `ICommand<T>`, `IQuery<T>`, `IAction`, `IIdempotent`,
  `IIdempotentCommand` / `IIdempotentCommand<T>`
- Handlers: `IRequestHandler<TRequest, TResponse>`, `IRequestHandler<TRequest>`,
  `ActionHandler<TAction>`, `INotificationHandler<T>`
- `ISender` / `ISender<TScope>`, `IPublisher` / `IPublisher<TScope>`, `IMediator`
- `MessageHandlerDelegate<TRequest, TResponse>`, `IPipelineBehavior<TRequest, TResponse>`
- Membership: `MediatorAssembly`, `MediatorScope`, `MediatorBehavior`
- `GeneratedMediatorAttribute`, `Unit`, `NoHandlerException`

Reference only `TimeWarp.Mediator` from an assembly that declares messages and does not emit
a mediator. Opt in with `[assembly: MediatorAssembly]`.

### Idempotent commands

Mark a command that is safe to retry with `IIdempotentCommand` / `IIdempotentCommand<T>`.
`IQuery<T>` is already `IIdempotent`. Dispatch matches any other `IRequest`. The marker is
for behaviors and for `MediatorManifest` kind data. There is no idempotency-key enforcement
or dedup store. Handlers implement `IRequestHandler`.

```csharp
public sealed class UpsertSetting : IIdempotentCommand<string>
{
  public string Key { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
}

public sealed class UpsertSettingHandler : IRequestHandler<UpsertSetting, string>
{
  public ValueTask<string> Handle(UpsertSetting request, CancellationToken cancellationToken) =>
    ValueTask.FromResult(request.Key + "=" + request.Value);
}

public sealed class RetryIdempotentBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IIdempotent
{
  public ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken) =>
    next(request, cancellationToken);
}
```

## Migrating

Package ids, `ValueTask` signatures, `Unit.ValueTask`, and the three consumer sections
(TimeWarp.State, TimeWarp.Architecture, TimeWarp.Nuru) are in
[migration.md](documentation/migration.md).

## Documentation

- [architecture.md](documentation/architecture.md): contracts, dispatcher, registration, manifest, membership
- [diagnostics.md](documentation/diagnostics.md): TWM001–TWM004 and TWM007, TWM008, TWM010, TWM011
- [m2-named-pipelines.md](documentation/m2-named-pipelines.md): `ISender<TScope>`
- [benchmarks/comparison-baseline.md](documentation/benchmarks/comparison-baseline.md): 021-003 baseline (commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`)
- [overview.md](documentation/overview.md): versioning, CI and releasing
- Release notes: [15.0.0](documentation/release-notes/15.0.0.md),
  [14.0.0](documentation/release-notes/release-notes-v14.0.0.md),
  [13.0.0](documentation/release-notes/release-notes-v13.0.0.md)

**For AI agents:** Load the built-in [Mediator skill](skills/tw-mediator/SKILL.md) for a
minimal host, named pipelines, `MediatorManifest`, TWM diagnostics, and the 15.0 migration.

## About this fork

TimeWarp Mediator started as a fork of MediatR to correct the spelling to "Mediator" and release
under The Unlicense. The source in this tree is the generated mediator.

## License

TimeWarp Mediator is released under The Unlicense (see `unlicense`). Original MediatR code by
Jimmy Bogard is under Apache 2.0 (see `notice`).
