# TimeWarp.Mediator

The reflection dispatcher for [TimeWarp Mediator](https://github.com/TimeWarpEngineering/timewarp-mediator):
a fork of [MediatR](https://github.com/jbogard/MediatR) with `AddMediator()`, assembly scanning,
pipeline behaviors, pre/post processors, streams, and the exception cascade.

TimeWarp Mediator has **two independent dispatchers**: the reflection runtime
(`AddMediator()`, package `TimeWarp.Mediator`) and the source-generated mediator
(`AddGeneratedMediator()`, packages `TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`).
`AddMediator()` does not register the generated mediator, and 14.0.0-beta is not a drop-in for
13.0.0. See [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md).

## Install

**13.0.0** is the last stable reflection line and also targets netstandard2.0 and net6.0. 14.x
targets net10.0 only.

```bash
dotnet add package TimeWarp.Mediator --version 13.0.0
```

## Use

```csharp
services.AddMediator(cfg =>
{
  cfg.RegisterServicesFromAssemblyContaining<Startup>();
  cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
```

`AddMediator()` registers `IMediator`, `ISender`, `IPublisher` and every handler it finds with
`Microsoft.Extensions.DependencyInjection`. Coming from MediatR: replace `using MediatR;` with
`using TimeWarp.Mediator;` and `AddMediatR` with `AddMediator`.

The reflection runtime is not trim or NativeAOT safe (IL2026 / IL3050). Use the generated stack
for those hosts.

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated vs reflection: two stacks, packages, membership, named pipelines](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md)
- [Migration: MediatR to 13.x, and 13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [Release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/release-notes-v14.0.0.md)

## License

Released under The Unlicense. Contains code derived from
[MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard under Apache 2.0; the `notice`
and `unlicense` files ship in this package.
