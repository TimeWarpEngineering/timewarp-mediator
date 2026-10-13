# TimeWarp.Mediator.Analyzers

Roslyn analyzers for TimeWarp Mediator 14.0 libraries that declare requests and handlers and do not run the source generator.

| Id | Summary |
|----|---------|
| TWM001 | Request has no handler |
| TWM002 | Duplicate handler for request |
| TWM003 | Handler and message belong to different pipelines |
| TWM004 | Request is not a member of this sender pipeline |

A project that references `TimeWarp.Mediator.Generators` already gets these analyzers. Do not add both.

## Install

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
dotnet add package TimeWarp.Mediator.Analyzers --version 14.0.0
```

## Example

```csharp
[assembly: MediatorAssembly]

public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult("Pong");
}
```

Requires Roslyn 4.8 or later (.NET SDK 8.0.100 or Visual Studio 2022 17.8). Contracts targets net10.0, so the library project needs the .NET 10 SDK.

## Migration

14.0.0 is not a drop-in for 13.0.0. These analyzers do not register a mediator. The host that dispatches the messages in this library references `TimeWarp.Mediator.Generators` and calls `AddGeneratedMediator()`. A missing handler fails that host's build (TWM001) even when this package is absent. Add this package so the library fails its own build first.

Matrix and steps: [13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Released under The Unlicense. The `notice` and `unlicense` files ship in this package.
