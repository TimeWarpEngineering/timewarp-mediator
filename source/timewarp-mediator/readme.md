# TimeWarp.Mediator

Message contracts and the TWM001–TWM004, TWM012, and TWM013 analyzer for TimeWarp Mediator 15.0: requests, commands, queries, notifications, and pipeline types.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 15.0.0-beta.10
```

Without `--version`, that command resolves 14.0.0. Through 14.0.0 this package id was the reflection runtime (`AddMediator`). The 14.0.0 contracts were `TimeWarp.Mediator.Contracts`, and the analyzers were `TimeWarp.Mediator.Analyzers`. 15.0 reuses this id for the contracts and the analyzer. A host also references `TimeWarp.Mediator.Generators` at the same version. This package ships the TWM001–TWM004, TWM012, and TWM013 analyzer, `GeneratedHandlerAttribute` so a request can name a handler another generator emits, and `GeneratedRequestPatternAttribute` so a source type can declare requests and handlers another generator emits. There is no separate analyzers package.

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

## Requests another generator emits

Source generators do not see each other's output. When another generator emits a request and its handler into the host, declare them on a source type so `Send` dispatches them:

```csharp
[GeneratedRequestPattern(
  "{Host}.{Contract}ActionSet.Action",
  "{Host}.{Contract}ActionSet.Handler",
  "TimeWarp.Mediator.IRequest<{ContractType}.Response>",
  typeof(CreateRole), typeof(UpdateRole))]
public partial class RoleState { }
```

- `requestTypeName` and `handlerTypeName` are dotted type names. `{Host}` is the attributed type's full name, `{Contract}` the contract's simple name, `{ContractType}` the contract's full name. No other placeholder exists.
- `requestInterfaceName` is the Mediator interface the emitted request implements. `TimeWarp.Mediator.IAction` or `TimeWarp.Mediator.IRequest` is a void request. `TimeWarp.Mediator.IRequest<{ContractType}.Response>` (or `IQuery<...>`, `ICommand<...>`, or a fixed type such as `IRequest<string>`) gives the response. Behaviors constrained on that interface close over the request. The interface and its response must be in source or in a reference.
- The attribute allows multiple uses and can be derived. A derived attribute declares its templates as constructor parameters with the same names (a default value is enough) or as `const string` fields named `RequestTypeName`, `HandlerTypeName`, and `RequestInterfaceName`. Its contracts are a constructor parameter named `contracts`:

```csharp
public sealed class ContractStateAttribute(params Type[] contracts) : GeneratedRequestPatternAttribute(contracts)
{
  public const string RequestTypeName = "{Host}.{Contract}ActionSet.Action";
  public const string HandlerTypeName = "{Host}.{Contract}ActionSet.Handler";
  public const string RequestInterfaceName = "TimeWarp.Mediator.IAction";
}
```

The send uses the same handler call and the same woven behaviors as a request written in source. After every generator has run, TWM013 reports a declared request or handler that does not exist, a request that does not implement the declared interface, or a handler that is not its `IRequestHandler`.

## Migration

15.0 is a breaking release. Handlers return `ValueTask`. The host calls `AddGeneratedMediator()` and supplies `IServiceProvider`. Registered pipeline behaviors are woven.

- This package targets net11.0 only (since 15.0.0-beta.9). `net10.0` consumers stay on 15.0.0-beta.8 or earlier. Build with the .NET 11 SDK (11.0.100 or later) and `dotnet build`. Visual Studio and .NET Framework MSBuild are not supported. The SDK compiler must be at least the Roslyn version the packages reference; an older SDK reports CS8032/CS9057 and the generator emits nothing.
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
