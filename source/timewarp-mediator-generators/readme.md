# TimeWarp.Mediator.Generators

Source generator for TimeWarp Mediator 15.0. It emits one internal mediator. `Send` returns the handler `ValueTask`. The host supplies `IServiceProvider`, and registered pipeline behaviors are woven. The path is trim and NativeAOT safe.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 15.0.0-beta.7
dotnet add package TimeWarp.Mediator.Generators --version 15.0.0-beta.7
```

Without `--version`, `dotnet add package TimeWarp.Mediator` resolves 14.0.0, the reflection runtime. Drop the flag when 15.0 is the stable package.

Generators does not bring `TimeWarp.Mediator` with it. Keep both on the same version. TWM001–TWM004 and TWM012 ship inside `TimeWarp.Mediator`. This package reports TWM007, TWM008, TWM010, and TWM011, and in a generator host it also reports TWM001–TWM003. A `GeneratedHandlerAttribute` whose handler name is not a constant, or is not a type name, is TWM012 from this package. An attribute in another assembly is read from a constructor parameter named `handlerTypeName`. The missing-type half of TWM012, and a named type that is not an `IRequestHandler`, are reported by the analyzer after every generator has run.

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
- `TimeWarp.Mediator` and this generator target net11.0 only (since 15.0.0-beta.7). `net10.0` consumers stay on 15.0.0-beta.6 or earlier. Build with the .NET 11 SDK (11.0.100 or later) and `dotnet build`. Visual Studio and .NET Framework MSBuild are not supported. The SDK compiler must be at least the Roslyn version the packages reference; an older SDK reports CS8032/CS9057 and the generator emits nothing.
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
