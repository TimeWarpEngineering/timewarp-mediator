# TimeWarp.Mediator

Message contracts and analyzers for TimeWarp Mediator 14.0: requests, commands, queries, notifications, pipeline types, and TWM001–TWM004.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 14.0.0
```

A host also needs `TimeWarp.Mediator.Generators` at the same version. Keep every TimeWarp.Mediator package on 14.0.0. This package ships the TWM001–TWM004 analyzer. There is no separate analyzers package.

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

`AddGeneratedMediator()` is emitted into the host project by `TimeWarp.Mediator.Generators`. Call it from the host. Reference only this package from a project that declares messages and does not dispatch them; that project still gets TWM001–TWM004.

## Migration

14.0.0 is not a drop-in for 13.0.0.

- This package targets net10.0 only. Stay on 13.0.0 if you cannot.
- The package id is `TimeWarp.Mediator`. The old `TimeWarp.Mediator.Contracts` id is gone.
- A host calls `AddGeneratedMediator()` and supplies `IServiceProvider`. Registered pipeline behaviors are woven.
- From MediatR: change `using MediatR;` to `using TimeWarp.Mediator;`.

Steps: [Generated host](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#generated-host).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated mediator](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m1-generated-mediator.md)
- [Diagnostics](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Licensed as `Unlicense AND Apache-2.0`. The Apache-2.0 terms cover code derived from [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. The `notice` and `unlicense` files ship in this package.
