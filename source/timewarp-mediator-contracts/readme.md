# TimeWarp.Mediator.Contracts

Message, handler and pipeline contracts for [TimeWarp Mediator](https://github.com/TimeWarpEngineering/timewarp-mediator).
Both the source-generated and the reflection dispatcher use these types.

TimeWarp Mediator has **two independent dispatchers**: the reflection runtime
(`AddMediator()`, package `TimeWarp.Mediator`) and the source-generated mediator
(`AddGeneratedMediator()`, packages `TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`).
`AddMediator()` does not register the generated mediator, and 14.0.0-beta is not a drop-in for
13.0.0. See [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md).

## What is in this package

- `IRequest` / `IRequest<T>`, `INotification`, `IStreamRequest<T>`
- CQRS markers and handlers: `ICommand` / `ICommand<T>`, `IQuery<T>`, `IAction`
- Idempotency markers: `IIdempotent`, `IIdempotentCommand` / `IIdempotentCommand<T>` and their handlers
- `IPipelineBehavior<,>` and the other handler interfaces
- `ISender` / `ISender<TScope>`, `IPublisher` / `IPublisher<TScope>`, `IMediator`
- Membership attributes: `MediatorAssembly`, `MediatorScope`, `MediatorBehavior`, `MediatorModule`
- `NoHandlerException`, `Unit`

Reference only this package from an assembly that declares messages (API, gRPC, Blazor client).
A host that dispatches still needs `TimeWarp.Mediator.Generators` (generated) or
`TimeWarp.Mediator` (reflection).

## Generated host

```bash
dotnet add package TimeWarp.Mediator.Contracts --prerelease
dotnet add package TimeWarp.Mediator.Generators --prerelease
```

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
```

Targets net10.0.

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated vs reflection: two stacks, packages, membership, named pipelines](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md)
- [Migration: MediatR to 13.x, and 13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [Release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/release-notes-v14.0.0.md)

## License

Released under The Unlicense. Contains code derived from
[MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard under Apache 2.0; the `notice`
and `unlicense` files ship in this package.
