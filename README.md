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

Migrating from MediatR is straightforward - see our [migration guide](./migration.md) for step-by-step instructions.

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

### Installing TimeWarp.Mediator

You should install [TimeWarp.Mediator with NuGet](https://www.nuget.org/packages/TimeWarp.Mediator):

    Install-Package TimeWarp.Mediator
    
Or via the .NET Core command line interface:

    dotnet add package TimeWarp.Mediator

Either commands, from Package Manager Console or .NET Core CLI, will download and install TimeWarp.Mediator and all required dependencies.

### Using Contracts-Only Package

To reference only the contracts for TimeWarp.Mediator, which includes:

- `IRequest` (including generic variants)
- `INotification`
- `IStreamRequest`

Add a package reference to [TimeWarp.Mediator.Contracts](https://www.nuget.org/packages/TimeWarp.Mediator.Contracts)

This package is useful in scenarios where your TimeWarp.Mediator contracts are in a separate assembly/project from handlers. Example scenarios include:
- API contracts
- GRPC contracts
- Blazor

### Registering with `IServiceCollection`

TimeWarp.Mediator supports `Microsoft.Extensions.DependencyInjection.Abstractions` directly. To register various Mediator services and handlers:

```
services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());
```

or with an assembly:

```
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