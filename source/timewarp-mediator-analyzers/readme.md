# TimeWarp.Mediator.Analyzers

Roslyn analyzers for TimeWarp Mediator libraries that declare requests and handlers and do not run the source generator. The analyzer DLL ships inside the `TimeWarp.Mediator` package at `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`. This project is not a NuGet package.

| Id | Summary |
|----|---------|
| TWM001 | Request has no handler |
| TWM002 | Duplicate handler for request |
| TWM003 | Handler and message belong to different pipelines |
| TWM004 | Request is not a member of this sender pipeline |

A project that references `TimeWarp.Mediator.Generators` already runs the generator, which reports TWM001–TWM003 in that host. The analyzer still reports TWM004 there, and it reports TWM001–TWM004 in a library.

## Install

Reference the contracts package. The analyzer comes with it.

```bash
dotnet add package TimeWarp.Mediator --version 15.0.0-beta.2
```

Without `--version`, that command resolves 14.0.0, the reflection runtime. Drop the flag when 15.0 is the stable package.

## Example

```csharp
[assembly: MediatorAssembly]

public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
    ValueTask.FromResult("Pong");
}
```

The analyzer targets netstandard2.0 and needs the Roslyn that ships with the .NET 10 SDK or newer (`Microsoft.CodeAnalysis.CSharp` 5.0.0 is the floor). `TimeWarp.Mediator` targets net10.0 and net11.0.

## Migration

These analyzers do not register a mediator. The host that dispatches the messages in this library references `TimeWarp.Mediator.Generators` and calls `AddGeneratedMediator()`. A missing handler fails that host's build (TWM001). Reference `TimeWarp.Mediator` so the library fails its own build first.

TWM005, TWM006, and TWM009 are retired. Streams and the Aot and Link profiles are removed.

Steps: [Migration to 15.0](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/migration.md).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Architecture](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/architecture.md)
- [Diagnostics](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)

## License

Released under The Unlicense.
