# TimeWarp.Mediator.Generators

Source generator for TimeWarp Mediator 14.0. It emits an internal mediator so `Send` is a direct call: less time, fewer bytes, and a trim and NativeAOT safe path.

## Install

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0
```

Generators does not bring Contracts with it. Keep both on 14.0.0. This package bundles the analyzers (TWM001–TWM004) and adds TWM005–TWM011.

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

The host that references this package is a graph member. A library opts in with `[assembly: MediatorAssembly]`. Register a pipeline behavior with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`. Set `<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>` for dispatch with no `IServiceProvider` (handlers are `new Handler()` singletons, and behaviors are not woven).

## Migration

14.0.0 is not a drop-in for 13.0.0.

- The host calls `AddGeneratedMediator()`.
- Contracts targets net10.0. The generator itself is netstandard2.0 because Roslyn loads it. You need Roslyn 4.8 or later, and in practice the .NET 10 SDK, because Contracts targets net10.0.
- Streams, pre/post processors, and the exception cascade are not dispatched (TWM009). Move that logic into a request handler or an `IPipelineBehavior`.
- A missing or duplicate handler is a build error (TWM001, TWM002), not a runtime exception.

Steps: [Generated host](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#generated-host).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated mediator](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m1-generated-mediator.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Released under The Unlicense. The `notice` and `unlicense` files ship in this package.
