# TimeWarp Mediator 14.0: The Rewrite That Pays You Back

13.0.0 corrected the spelling and the license. 14.0.0 is a different dispatcher. The reflection
`AddMediator()` you already call is still here, and it still does not register the
source-generated mediator. The generated one is the path that pays the rewrite back: a
monomorphic `Send`, fewer allocated bytes, and a host that can publish trimmed or with
NativeAOT.

## Ten lines

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0
```

```csharp
public sealed class Ping : IRequest<string> { }

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult("Pong");
}

services.AddGeneratedMediator();
string reply = await provider.GetRequiredService<ISender>().Send(new Ping());
```

`AddGeneratedMediator()` is generated as `internal`. Call it from the host project. A library
that only declares messages references `TimeWarp.Mediator.Contracts`. A library that declares
handlers and does not run the generator also references `TimeWarp.Mediator.Analyzers` and adds
`[assembly: MediatorAssembly]`.

## What the baseline actually says

[comparison-baseline.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/benchmarks/comparison-baseline.md)
was recorded 2026-10-12 at commit `d478415` with BenchmarkDotNet 0.15.8 on .NET 10.0.11, an
AMD Ryzen 9 7950X3D, Ubuntu. The workload is Ping/Pong with no pipeline behaviors.

| Send | Mean | Allocated |
|---|---:|---:|
| Reflection `AddMediator` | 69.0 ns | 224 B |
| Generated monomorphic `Send` | 29.9 ns | 96 B |
| martinothamar generator (same run) | 8.8 ns | 24 B |

The ratio column in that file is **0.43** for time and **0.43** for bytes. Generated `Send`
takes less than half the time and less than half the bytes of this repository's reflection
`Send`. martinothamar's generator is still faster on this microbenchmark (8.8 ns, 24 B). That
gap is real. 14.0.0 does not close it. The number to remember is the one against our own
reflection path, on this machine, for this Ping.

The reflection entry points carry `RequiresUnreferencedCode` and `RequiresDynamicCode`. A
trimmed or NativeAOT publish that still calls `AddMediator()` gets IL2026 / IL3050. The
generated host does not. The Aot profile goes further: no `IServiceProvider`, handlers
constructed once with `new Handler()`. Behaviors are not woven on that profile. Use the Host
profile when handlers take constructor dependencies or you need `IPipelineBehavior`.

## What broke

A lot. Read this before you bump a 13.0.0 host.

- **net10.0 only** for `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts`. netstandard2.0
  and net6.0 assets are gone. If you cannot move, stay on 13.0.0.
- **`AddMediator()` is the other stack.** It will not start generating code because the
  package version changed. Call `AddGeneratedMediator()`.
- **Streams, pre/post processors, and the exception cascade** are reflection-only. The
  generator reports TWM009 and the generated `CreateStream` throws. Move that work into a
  request handler or an `IPipelineBehavior`, or keep `AddMediator()`.
- **Behaviors move to an attribute.** `cfg.AddOpenBehavior(typeof(LoggingBehavior<,>))`
  becomes `[assembly: MediatorBehavior(typeof(LoggingBehavior<,>))]` on the Host profile.
- **Some runtime types are gone or internal.** `ServiceFactory` is removed. The handler
  wrappers and `ServiceRegistrar` are internal. You did not need them to send a request.
- **License expression.** The reflection package and Contracts are `Unlicense AND Apache-2.0`
  because they still contain MediatR-derived code. Generators and Analyzers are Unlicense.
  `notice` and `unlicense` ship in every package.

The steps, and the feature matrix, are in
[migration.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/v14.0.0/migration.md#13x-reflection-to-14x-generated).
From MediatR itself, the change is still the 13.x rename (`using MediatR` to
`using TimeWarp.Mediator`, `AddMediatR` to `AddMediator`). That rename does not select the
generated mediator.

Named pipelines, if you want them, are a marker type rather than a string:
`[MediatorScope(typeof(ClientPipeline))]` and `AddGeneratedMediator<ClientPipeline>()`.
Diagnostics TWM001–TWM011 are listed in
[diagnostics.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md).

## Install

NuGet: https://www.nuget.org/packages/TimeWarp.Mediator/14.0.0

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0
```

Repository: https://github.com/TimeWarpEngineering/timewarp-mediator
