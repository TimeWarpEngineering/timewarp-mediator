# TimeWarp.Mediator.Contracts

Message contracts for TimeWarp Mediator 14.0: one set of requests, commands, queries, notifications, and pipeline types for both dispatchers.

## Install

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
```

A generated host also needs `TimeWarp.Mediator.Generators` at the same version. A reflection host needs `TimeWarp.Mediator` instead. Keep every TimeWarp.Mediator package on 14.0.0.

## Example

```csharp
public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult("Pong");
}

services.AddGeneratedMediator();
string reply = await provider.GetRequiredService<ISender>().Send(new Ping());
```

`AddGeneratedMediator()` is emitted into the host project. Call it from the host, not from this contracts assembly. Reference only this package from a project that declares messages and does not dispatch them.

## Migration

14.0.0 is not a drop-in for 13.0.0.

- This package and `TimeWarp.Mediator` target net10.0 only. Stay on 13.0.0 if you cannot.
- `AddMediator()` does not register the generated mediator. A generated host calls `AddGeneratedMediator()`.
- The generated dispatcher does not run streams, pre/post processors, or the exception cascade.
- From MediatR: change `using MediatR;` to `using TimeWarp.Mediator;` and `AddMediatR` to `AddMediator`.

Matrix and steps: [13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Licensed as `Unlicense AND Apache-2.0`. The Apache-2.0 terms cover code derived from [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. The `notice` and `unlicense` files ship in this package.
