# TimeWarp.Mediator.Generators

Source generator for TimeWarp Mediator 15.0. It emits one internal mediator. `Send` returns the handler `ValueTask`. The host supplies `IServiceProvider`, and registered pipeline behaviors are woven. The path is trim and NativeAOT safe.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 15.0.0-beta.2
dotnet add package TimeWarp.Mediator.Generators --version 15.0.0-beta.2
```

Without `--version`, `dotnet add package TimeWarp.Mediator` resolves 14.0.0, the reflection runtime. Drop the flag when 15.0 is the stable package.

Generators does not bring `TimeWarp.Mediator` with it. Keep both on the same version. TWM001–TWM004 ship inside `TimeWarp.Mediator`. This package reports TWM007, TWM008, TWM010, and TWM011, and in a generator host it also reports TWM001–TWM003.

## Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;

ServiceCollection services = new();
services.AddGeneratedMediator();
using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
string reply = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new Ping());

public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
    ValueTask.FromResult("Pong");
}
```

The host that references this package is a graph member. A library opts in with `[assembly: MediatorAssembly]` and does not reference this package. Register a pipeline behavior with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`. `AddGeneratedMediator()` lives in the generated namespace (`TimeWarp.Mediator.Generated` unless `TimeWarpMediatorNamespace` is set).

## Migration

15.0 is a breaking release.

- Handlers and `ISender` / `IPublisher` return `ValueTask`. Behaviors take `MessageHandlerDelegate<TRequest, TResponse>` and call `next(request, cancellationToken)`.
- `TimeWarp.Mediator` targets net10.0 and net11.0. The generator itself is netstandard2.0 because Roslyn loads it. The Roslyn floor is `Microsoft.CodeAnalysis.CSharp` 5.0.0.
- A missing or duplicate handler is a build error (TWM001, TWM002).
- `TimeWarpMediatorProfile` is removed. Tooling reads `[assembly: GeneratedMediator(typeof(Mediator))]` and `MediatorManifest` version 2.

Steps: [Migration to 15.0](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/migration.md).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Architecture](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/architecture.md)
- [Diagnostics](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [15.0.0 release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/15.0.0.md)

## License

Released under The Unlicense. The `notice` and `unlicense` files ship in this package.
