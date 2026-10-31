# TimeWarp.Mediator

Message contracts and the TWM001–TWM004 analyzer for TimeWarp Mediator 15.0: requests, commands, queries, notifications, and pipeline types.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 15.0.0-beta.4
```

Without `--version`, that command resolves 14.0.0. Through 14.0.0 this package id was the reflection runtime (`AddMediator`). The 14.0.0 contracts were `TimeWarp.Mediator.Contracts`, and the analyzers were `TimeWarp.Mediator.Analyzers`. 15.0 reuses this id for the contracts and the analyzer. A host also references `TimeWarp.Mediator.Generators` at the same version. This package ships the TWM001–TWM004 analyzer. There is no separate analyzers package.

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

`AddGeneratedMediator()` is emitted into the host project by `TimeWarp.Mediator.Generators`, in the generated namespace (`TimeWarp.Mediator.Generated` unless the host sets `TimeWarpMediatorNamespace`). Call it from the host. Reference only this package from a project that declares messages and does not dispatch them. That project still gets TWM001–TWM004, and it opts in with `[assembly: MediatorAssembly]`.

## Migration

15.0 is a breaking release. Handlers return `ValueTask`. The host calls `AddGeneratedMediator()` and supplies `IServiceProvider`. Registered pipeline behaviors are woven.

- This package targets net10.0 and net11.0. The packages need a .NET SDK whose compiler is at least the Roslyn version they reference (11.0.100 or later; a 10.0.x band only if its compiler is at least that version); an older SDK reports CS8032/CS9057 and the generator emits nothing.
- Through 14.0.0 the id `TimeWarp.Mediator` was the reflection runtime (`AddMediator`). 15.0 reuses that id for the contracts and the analyzer. The 14.0.0 contracts id `TimeWarp.Mediator.Contracts` is gone, and so is `AddMediator`.
- From MediatR: change `using MediatR;` to `using TimeWarp.Mediator;`.

Steps: [Migration to 15.0](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/migration.md).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Architecture](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/architecture.md)
- [Diagnostics](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [15.0.0 release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/15.0.0.md)

## License

Licensed as `Unlicense AND Apache-2.0`. The Apache-2.0 terms cover code derived from [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. The `notice` and `unlicense` files ship in this package.
