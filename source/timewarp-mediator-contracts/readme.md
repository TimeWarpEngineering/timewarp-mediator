# TimeWarp.Mediator.Contracts

Message contracts for TimeWarp Mediator 14.0: requests, commands, queries, notifications, and pipeline types for the generated mediator.

## Install

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
```

A host also needs `TimeWarp.Mediator.Generators` at the same version. Keep every TimeWarp.Mediator package on 14.0.0.

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

`AddGeneratedMediator()` is emitted into the host project. Call it from the host, not from this contracts assembly. Reference only this package from a project that declares messages and does not dispatch them.

## Migration

14.0.0 is not a drop-in for 13.0.0.

- This package targets net10.0 only. Stay on 13.0.0 if you cannot.
- A host calls `AddGeneratedMediator()`.
- Pre/post processors and the exception cascade are not dispatched (TWM009). Move that logic into a request handler or an `IPipelineBehavior`.
- From MediatR: change `using MediatR;` to `using TimeWarp.Mediator;`.

Steps: [Generated host](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#generated-host).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated mediator](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m1-generated-mediator.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Licensed as `Unlicense AND Apache-2.0`. The Apache-2.0 terms cover code derived from [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. The `notice` and `unlicense` files ship in this package.
