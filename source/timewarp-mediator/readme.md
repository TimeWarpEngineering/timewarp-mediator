# TimeWarp.Mediator

The reflection dispatcher for TimeWarp Mediator 14.0: assembly scanning, pipeline behaviors, streams, pre/post processors, and the exception cascade. It is the MediatR-shaped API. It is not the source-generated mediator.

## Install

```bash
dotnet add package TimeWarp.Mediator --version 14.0.0
```

14.0.0 targets net10.0 only. Stay on 13.0.0 when you cannot:

```bash
dotnet add package TimeWarp.Mediator --version 13.0.0
```

## Example

```csharp
public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult("Pong");
}

services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining<PingHandler>());
string reply = await provider.GetRequiredService<ISender>().Send(new Ping());
```

`AddMediator()` registers `IMediator`, `ISender`, `IPublisher`, and the handlers it finds. It does not register the generated mediator. The reflection runtime is not trim or NativeAOT safe (IL2026 / IL3050).

## Migration

14.0.0 is not a drop-in for 13.0.0, and neither stack is a drop-in for the other.

- From MediatR: `using MediatR;` becomes `using TimeWarp.Mediator;`, and `AddMediatR` becomes `AddMediator`.
- From 13.x reflection to the generated stack: reference `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators`, mark handler assemblies with `[assembly: MediatorAssembly]`, and call `AddGeneratedMediator()`. Drop this package once nothing calls `AddMediator()`.
- Generated dispatch does not run streams, pre/post processors, or the exception cascade. Stay on this package if you need those.
- `ServiceFactory` is removed; resolve through `IServiceProvider`. The handler wrappers and `ServiceRegistrar` are internal. Call `ISender.Send`, `IPublisher.Publish`, and `AddMediator`.

Matrix and steps: [13.x reflection to 14.x generated](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).

## Documentation

- [Repository readme](https://github.com/TimeWarpEngineering/timewarp-mediator#readme)
- [Generated vs reflection](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md)
- [Diagnostics TWM001–TWM011](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md)
- [Release notes](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/release-notes-v14.0.0.md)

## License

Licensed as `Unlicense AND Apache-2.0`. The Apache-2.0 terms cover code derived from [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. The `notice` and `unlicense` files ship in this package.
