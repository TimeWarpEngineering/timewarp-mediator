# TimeWarp.Mediator.Analyzers

Roslyn analyzers for [TimeWarp Mediator](https://github.com/TimeWarpEngineering/timewarp-mediator)
libraries that do **not** run the source generator.

| Id | Summary |
|----|---------|
| TWM001 | Request has no handler |
| TWM002 | Duplicate handler for request |
| TWM003 | Handler and message belong to different pipelines |
| TWM004 | Request is not a member of this sender pipeline |

TimeWarp Mediator has **two independent dispatchers**: the reflection runtime
(`AddMediator()`, package `TimeWarp.Mediator`) and the source-generated mediator
(`AddGeneratedMediator()`, packages `TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`).
`AddMediator()` does not register the generated mediator, and 14.0.0-beta is not a drop-in for
13.0.0. See [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md).

## When to add this package

A project that references `TimeWarp.Mediator.Generators` already gets these analyzers; do not
add both. Add this package to a library that holds requests and handlers for a generated host,
so it fails its own build on a missing or duplicate handler:

```bash
dotnet add package TimeWarp.Mediator.Contracts --prerelease
dotnet add package TimeWarp.Mediator.Analyzers --prerelease
```

```csharp
[assembly: MediatorAssembly]
```

Requires Roslyn 4.8 or later (.NET SDK 8.0.100 or Visual Studio 2022 17.8). In practice the
library needs .NET SDK 10.0.100, because the install above also references Contracts, which
targets net10.0.

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated vs reflection: two stacks, packages, membership, named pipelines](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md)
- [Migration: MediatR to 13.x, and 13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [Release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/release-notes-v14.0.0.md)

## License

Released under The Unlicense. The `notice` and `unlicense` files ship in this package.
