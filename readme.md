TimeWarp Mediator
=================

[![CI/CD](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/workflows/workflow.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-mediator/actions/workflows/workflow.yml)
[![NuGet](https://img.shields.io/nuget/v/TimeWarp.Mediator.svg)](https://www.nuget.org/packages/TimeWarp.Mediator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TimeWarp.Mediator.svg)](https://www.nuget.org/packages/TimeWarp.Mediator)

TimeWarp Mediator 14.0: in-process messaging for .NET with a source-generated dispatcher. The
generated path is trim and NativeAOT safe. This project is a fork of
[MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard.

## Packages

| Package | Use it for |
|---------|------------|
| [TimeWarp.Mediator.Contracts](https://www.nuget.org/packages/TimeWarp.Mediator.Contracts) | Message, handler and pipeline interfaces, `ISender` / `IPublisher` / `IMediator`, membership attributes. Needed by both stacks. |
| [TimeWarp.Mediator.Generators](https://www.nuget.org/packages/TimeWarp.Mediator.Generators) | Source generator for the compile-time mediator (`AddGeneratedMediator()`). Bundles the analyzers. |
| [TimeWarp.Mediator.Analyzers](https://www.nuget.org/packages/TimeWarp.Mediator.Analyzers) | TWM001–TWM004 for libraries that do not run the generator. |

## Target framework

`TimeWarp.Mediator.Contracts` 14.x targets **net10.0 only**. Stay on 13.x if you cannot move to
net10.0 or newer. The Generators and Analyzers packages target netstandard2.0 because they run
inside the compiler; they need Roslyn 4.8 or later.

## Quick start: generated stack

Keep every TimeWarp.Mediator package on the same version.

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0
```

Those lines are the 14.0.0 stable install, which is what the packed package readmes describe.

```csharp
public sealed class Ping : IRequest<string>
{
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult("Pong");
}

services.AddGeneratedMediator();

ISender sender = provider.GetRequiredService<ISender>();
string reply = await sender.Send(new Ping());
```

The generated types and `AddGeneratedMediator()` are `internal` to the host project, so call it
from the host's own code. Membership, named pipelines (`AddGeneratedMediator<TScope>()`) and the
Aot profile: [m1-generated-mediator.md](./documentation/m1-generated-mediator.md) and
[m2-named-pipelines.md](./documentation/m2-named-pipelines.md). Diagnostics:
[diagnostics.md](./documentation/diagnostics.md).

## Contracts

`TimeWarp.Mediator.Contracts` holds the message, handler, and pipeline types:

- `IRequest` / `IRequest<T>`, `INotification`, `IStreamRequest<T>`
- CQRS markers and handlers: `ICommand` / `ICommand<T>`, `IQuery<T>`, `IAction`
- Idempotency markers: `IIdempotent`, `IIdempotentCommand` / `IIdempotentCommand<T>` and their
  handlers
- `ISender` / `ISender<TScope>`, `IPublisher` / `IPublisher<TScope>`, `IMediator`
- Membership attributes: `MediatorAssembly`, `MediatorScope`, `MediatorBehavior`, `MediatorModule`

Reference only Contracts from an assembly that declares messages (API, gRPC, Blazor client).
A generated host still needs Generators on the compilation that emits the mediator.

### Idempotent commands

Mark a command that is safe to retry with `IIdempotentCommand` / `IIdempotentCommand<T>`.
`IQuery<T>` is already `IIdempotent`. The generated mediator dispatches it exactly like an `ICommand`; the
marker is for your own behaviors (for example a retry policy). There is no idempotency-key
enforcement or dedup store.

```csharp
public sealed class UpsertSetting : IIdempotentCommand<string>
{
  public string Key { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
}

public sealed class UpsertSettingHandler : IIdempotentCommandHandler<UpsertSetting, string>
{
  public Task<string> Handle(UpsertSetting request, CancellationToken cancellationToken) =>
    Task.FromResult(request.Key + "=" + request.Value);
}

public sealed class RetryIdempotentBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IIdempotent
{
  public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) =>
    next(cancellationToken); // wrap in your retry policy
}
```

## Migrating

Host packages, registration, behaviors, and TWM009 are in
[migration.md](./migration.md#generated-host).

## Documentation

- [diagnostics.md](./documentation/diagnostics.md): TWM001–TWM011
- [m1-generated-mediator.md](./documentation/m1-generated-mediator.md),
  [m2-named-pipelines.md](./documentation/m2-named-pipelines.md): generated dispatcher details
- [overview.md](./documentation/overview.md): versioning, CI and releasing
- Release notes: [14.0.0](./documentation/release-notes/release-notes-v14.0.0.md),
  [13.0.0](./documentation/release-notes/release-notes-v13.0.0.md)

**For AI agents:** Load the built-in [Mediator skill](skills/tw-mediator/SKILL.md) for:
- A minimal generated host: requests, notifications, behaviors and named pipelines
- Profiles (Host, Aot, Link) and TWM001–TWM011 fixes
- Known gotchas and the generated-host migration

## About this fork

TimeWarp Mediator started as a fork of MediatR to correct the spelling to "Mediator" and release
under The Unlicense. The source in this tree is the generated mediator.

## License

TimeWarp Mediator is released under The Unlicense (see `unlicense`). Original MediatR code by
Jimmy Bogard is under Apache 2.0 (see `notice`).
