# TimeWarp.Mediator.Generators

Source generator for TimeWarp Mediator 14.0. It emits an internal mediator so `Send` is a direct call: less time, fewer bytes, and a trim and NativeAOT safe path.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 14.0.0
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0
```

Generators does not bring `TimeWarp.Mediator` with it. Keep both on 14.0.0. TWM001–TWM004 ship inside `TimeWarp.Mediator`. This package reports TWM007, TWM008, TWM010, and TWM011, and in a generator host it also reports TWM001–TWM003.

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

The host that references this package is a graph member. A library opts in with `[assembly: MediatorAssembly]`. Register a pipeline behavior with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`. The generated mediator takes `IServiceProvider`, and registered behaviors are woven.

## Migration

14.0.0 is not a drop-in for 13.0.0.

- The host calls `AddGeneratedMediator()`.
- `TimeWarp.Mediator` targets net10.0. The generator itself is netstandard2.0 because Roslyn loads it. The Roslyn floor is the compiler that ships with the .NET 10 SDK (`Microsoft.CodeAnalysis.CSharp` 5.0.0).
- A missing or duplicate handler is a build error (TWM001, TWM002), not a runtime exception.

Steps: [Generated host](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#generated-host).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated mediator](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m1-generated-mediator.md)
- [Diagnostics](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Released under The Unlicense. The `notice` and `unlicense` files ship in this package.
