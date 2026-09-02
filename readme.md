TimeWarp Mediator
=================

[![CI](https://github.com/TimeWarpEngineering/timewarp-mediator/workflows/CI/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-mediator/actions)
[![NuGet](https://img.shields.io/nuget/v/TimeWarp.Mediator.svg)](https://www.nuget.org/packages/TimeWarp.Mediator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TimeWarp.Mediator.svg)](https://www.nuget.org/packages/TimeWarp.Mediator)

Simple, unambitious mediator implementation in .NET

In-process messaging with no dependencies.

Supports request/response, commands, queries, notifications and events, synchronous and async with intelligent dispatching via C# generic variance.

## About This Fork

TimeWarp.Mediator is a fork of the excellent [MediatR](https://github.com/jbogard/MediatR) library by Jimmy Bogard. We created this fork to:

- ✅ Correct the spelling from "MediatR" to "Mediator"
- ✅ Release under The Unlicense for maximum freedom
- ✅ Maintain full API compatibility with MediatR
- ✅ Add helpful diagnostic tools like `GetPipelineInfo()`

### Migration from MediatR

Migrating from MediatR is the 13.x reflection rename — see our [migration guide](./migration.md).
That path is `AddMediator()`, not source generation.

## Two stacks (read this before `AddMediator`)

This repository ships **two independent dispatchers**. Calling `AddMediator()` does **not**
register the source-generated mediator.

| Registration | Dispatcher |
|--------------|------------|
| `services.AddMediator(...)` | Reflection MediatR fork (13.x runtime, still in this repo) |
| `services.AddGeneratedMediator()` | Source-generated unscoped `IMediator` / `ISender` / `IPublisher` |
| `services.AddGeneratedMediator<TScope>()` | Source-generated named pipeline (`ISender<TScope>` / `IPublisher<TScope>`) |

**14.0.0-beta is not a drop-in for 13.0.0.** As of `14.0.0-beta.1`, the generated stack is
proven only against the M1/M2 golden files in this repo. Do not bump a 13.0.0 host to
14.0.0-beta and keep `AddMediator(...)` expecting source-gen, AOT-clean dispatch, or named
pipelines. TimeWarp.State and TimeWarp.Nuru must call `AddGeneratedMediator()` /
`AddGeneratedMediator<TScope>()`. This tree's `<Version>` is `14.0.0-beta.1`. nuget.org
serves that as a **prerelease**; **13.0.0** remains the last stable reflection line.

GitHub issue [#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52)
stays **open** until a **stable 14.0.0**.

Comparison: [documentation/generated-vs-legacy.md](./documentation/generated-vs-legacy.md).
Milestones: [m1-generated-mediator.md](./documentation/m1-generated-mediator.md),
[m2-named-pipelines.md](./documentation/m2-named-pipelines.md).

---

## Original MediatR

![CI](https://github.com/jbogard/MediatR/workflows/CI/badge.svg)
[![NuGet](https://img.shields.io/nuget/dt/mediatr.svg)](https://www.nuget.org/packages/mediatr) 
[![NuGet](https://img.shields.io/nuget/vpre/mediatr.svg)](https://www.nuget.org/packages/mediatr)
[![MyGet (dev)](https://img.shields.io/myget/mediatr-ci/v/MediatR.svg)](https://myget.org/gallery/mediatr-ci)

Simple mediator implementation in .NET

In-process messaging with no dependencies.

Supports request/response, commands, queries, notifications and events, synchronous and async with intelligent dispatching via C# generic variance.

Examples in the [wiki](https://github.com/jbogard/MediatR/wiki).

## Installing the reflection stack (`AddMediator`)

NuGet **13.0.0** is the last published reflection line. Install
[TimeWarp.Mediator](https://www.nuget.org/packages/TimeWarp.Mediator):

```bash
dotnet add package TimeWarp.Mediator --version 13.0.0
```

This is the MediatR-fork runtime: assembly scan, wrappers, `MakeGenericType`.
`14.0.0-beta` still contains this runtime, but that prerelease is **not** a drop-in for 13.0.0
(see [Two stacks](#two-stacks-read-this-before-addmediator)).

## Installing the generated stack (`AddGeneratedMediator`)

A generated host needs **Contracts + Generators**. Analyzers ride inside the Generators nupkg;
add `TimeWarp.Mediator.Analyzers` only when the generator is not referenced.

```xml
<ItemGroup>
  <PackageReference Include="TimeWarp.Mediator.Contracts" Version="14.0.0-beta.1" />
  <PackageReference Include="TimeWarp.Mediator.Generators" Version="14.0.0-beta.1" />
</ItemGroup>
```

```bash
dotnet add package TimeWarp.Mediator.Contracts --version 14.0.0-beta.1
dotnet add package TimeWarp.Mediator.Generators --version 14.0.0-beta.1
# library-only (no generator):
dotnet add package TimeWarp.Mediator.Analyzers --version 14.0.0-beta.1
```

Do **not** add only `TimeWarp.Mediator` and call `AddMediator()` expecting this stack.

## Using Contracts-Only Package

To reference only the contracts for TimeWarp.Mediator, which includes:

- `IRequest` (including generic variants)
- `INotification`
- `IStreamRequest`
- `ISender` / `ISender<TScope>`, `IPublisher` / `IPublisher<TScope>`
- Membership attributes (`MediatorAssembly`, `MediatorScope`, `MediatorBehavior`)

Add a package reference to [TimeWarp.Mediator.Contracts](https://www.nuget.org/packages/TimeWarp.Mediator.Contracts).

This package is useful when contracts live in a separate assembly from handlers (API / gRPC /
Blazor). A generated host still needs Generators on the compilation that emits the mediator.

## Generated membership and named pipelines

Hosts that reference Generators get `TimeWarpMediatorAssembly=true` from the package
`buildTransitive` props. Domain libraries that only reference Analyzers must opt in:

```csharp
[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(LoggingBehavior<,>))]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
```

Named pipelines use **marker types**, not strings (`ClientPipeline` / `ServerPipeline`):

```csharp
public sealed class ClientPipeline
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPing : IRequest<string>
{
}
```

`[MediatorModule("Orders")]` joins the graph. It does not name a pipeline.

```csharp
services.AddGeneratedMediator();
services.AddGeneratedMediator<ClientPipeline>();
services.AddGeneratedMediator<ServerPipeline>();
```

Details: [documentation/generated-vs-legacy.md](./documentation/generated-vs-legacy.md),
[documentation/m1-generated-mediator.md](./documentation/m1-generated-mediator.md),
[documentation/m2-named-pipelines.md](./documentation/m2-named-pipelines.md).

## Registering reflection `AddMediator` with `IServiceCollection`

The reflection package supports `Microsoft.Extensions.DependencyInjection.Abstractions` directly.
This is **not** the generated dispatcher. To scan and register the fork runtime:

```csharp
services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());
```

or with an assembly:

```csharp
services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));
```

This registers:

- `IMediator` as transient
- `ISender` as transient
- `IPublisher` as transient
- `IRequestHandler<,>` concrete implementations as transient
- `IRequestHandler<>` concrete implementations as transient
- `INotificationHandler<>` concrete implementations as transient
- `IStreamRequestHandler<>` concrete implementations as transient
- `IRequestExceptionHandler<,,>` concrete implementations as transient
- `IRequestExceptionAction<,>)` concrete implementations as transient

This also registers open generic implementations for:

- `INotificationHandler<>`
- `IRequestExceptionHandler<,,>`
- `IRequestExceptionAction<,>`

To register behaviors, stream behaviors, pre/post processors:

```csharp
services.AddMediator(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly);
    cfg.AddBehavior<PingPongBehavior>();
    cfg.AddStreamBehavior<PingPongStreamBehavior>();
    cfg.AddRequestPreProcessor<PingPreProcessor>();
    cfg.AddRequestPostProcessor<PingPongPostProcessor>();
    cfg.AddOpenBehavior(typeof(GenericBehavior<,>));
    });
```

With additional methods for open generics and overloads for explicit service types.

## License

TimeWarp Mediator is released under The Unlicense (see `UNLICENSE`). Original MediatR code by Jimmy Bogard is under Apache 2.0 (see `NOTICE`).