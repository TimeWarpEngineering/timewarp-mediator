# TimeWarp.Mediator

Message contracts and the TWM001–TWM004 analyzer for TimeWarp Mediator 15.0: requests, commands, queries, notifications, and pipeline types.

## Install

```bash
dotnet add package TimeWarp.Mediator
```

A host also references `TimeWarp.Mediator.Generators` at the same version. This package ships the TWM001–TWM004 analyzer. There is no separate analyzers package. 14.0.0 on nuget.org is the previous surface (`TimeWarp.Mediator.Contracts` plus a separate analyzers package).

## Example

```csharp
public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
    ValueTask.FromResult("Pong");
}

services.AddGeneratedMediator();
string reply = await provider.GetRequiredService<ISender>().Send(new Ping());
```

`AddGeneratedMediator()` is emitted into the host project by `TimeWarp.Mediator.Generators`, in the generated namespace (`TimeWarp.Mediator.Generated` unless the host sets `TimeWarpMediatorNamespace`). Call it from the host. Reference only this package from a project that declares messages and does not dispatch them. That project still gets TWM001–TWM004, and it opts in with `[assembly: MediatorAssembly]`.

## Migration

15.0 is a breaking release. Handlers return `ValueTask`. The host calls `AddGeneratedMediator()` and supplies `IServiceProvider`. Registered pipeline behaviors are woven.

- This package targets net10.0.
- The package id is `TimeWarp.Mediator`. The 14.0.0 id `TimeWarp.Mediator.Contracts` is gone, and so is the reflection runtime that used this id in 13.x.
- From MediatR: change `using MediatR;` to `using TimeWarp.Mediator;`.

Steps: [Migration to 15.0](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/migration.md).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Architecture](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/architecture.md)
- [Diagnostics](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [15.0.0 release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/15.0.0.md)

## License

Licensed as `Unlicense AND Apache-2.0`. The Apache-2.0 terms cover code derived from [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. The `notice` and `unlicense` files ship in this package.
