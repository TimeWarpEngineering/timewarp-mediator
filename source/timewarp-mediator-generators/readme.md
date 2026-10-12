# TimeWarp.Mediator.Generators

Source generator for [TimeWarp Mediator](https://github.com/TimeWarpEngineering/timewarp-mediator).
It builds the message graph at compile time and emits an `internal sealed Mediator`, per-pipeline
senders and publishers, and `AddGeneratedMediator()`. The analyzers (TWM001–TWM004) ship inside
this package; the generator adds TWM005–TWM011.

TimeWarp Mediator has **two independent dispatchers**: the reflection runtime
(`AddMediator()`, package `TimeWarp.Mediator`) and the source-generated mediator
(`AddGeneratedMediator()`, packages `TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`).
`AddMediator()` does not register the generated mediator, and 14.0.0-beta is not a drop-in for
13.0.0. See [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md).

## Install

Add Contracts and Generators to the host project, at the same version. Generators is a
development dependency and does not bring Contracts with it.

```bash
dotnet add package TimeWarp.Mediator.Contracts --prerelease
dotnet add package TimeWarp.Mediator.Generators --prerelease
```

## Use

```csharp
services.AddGeneratedMediator();                    // unscoped IMediator / ISender / IPublisher
services.AddGeneratedMediator<ClientPipeline>();    // named pipeline ISender<ClientPipeline>
```

The host referencing this package is a graph member automatically. Libraries whose handlers
the host should dispatch add `[assembly: MediatorAssembly]`. Pipeline behaviors are registered
with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`. Streams, pre/post processors and
the exception cascade are not dispatched (TWM009); see the
[13.x to 14.x feature matrix](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).

Set `<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>` for NativeAOT dispatch without
`IServiceProvider`.

## Requirements

Built against Microsoft.CodeAnalysis.CSharp 4.8.0: .NET SDK 8.0.100 or Visual Studio 2022 17.8
or later. In practice the host needs .NET SDK 10.0.100, because Contracts targets net10.0.

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
