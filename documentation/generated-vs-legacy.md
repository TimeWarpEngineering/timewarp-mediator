# Generated vs reflection `AddMediator`

This repository ships **two independent dispatchers**. Calling `AddMediator()` does **not**
register the source-generated mediator. TimeWarp.State and TimeWarp.Nuru must call
`AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` to get the compile-time graph.

## Two stacks

| Registration | What you get | Line |
|--------------|--------------|------|
| `services.AddMediator(...)` | Reflection MediatR fork (the 13.x runtime). Assembly scan, wrappers, `MakeGenericType`. Still in this repo. | `TimeWarp.Mediator` |
| `services.AddGeneratedMediator()` | Source-generated unscoped `IMediator` / `ISender` / `IPublisher` (`TimeWarp.Mediator.Generated.Mediator`) | Contracts + Generators |
| `services.AddGeneratedMediator<TScope>()` | Source-generated named pipeline `ISender<TScope>` / `IPublisher<TScope>` | same |

`AddMediator()` does not prefer, wrap, or fall through to the generated type. That shim is
deferred (see [m1-generated-mediator.md](./m1-generated-mediator.md) deferred table).

## 14.0.0-beta is not a drop-in for 13.0.0

As of `14.0.0-beta.1`, the generated stack is proven only against the M1/M2 golden files in
this repo:

- Generator tests (`tests/timewarp-mediator-generators-tests`), including State-shaped
  `IncrementActionSet` + `StateTransactionBehavior`
- Named-pipeline tests and `samples/timewarp-mediator-examples-named-pipelines`
- AOT sample `samples/timewarp-mediator-examples-aot` (ServiceGen static fields; no
  `AddMediator`)

It is **not** an API-compatible upgrade of NuGet **13.0.0** (the last published reflection
line). Do not bump a 13.0.0 host to 14.0.0-beta and keep `AddMediator(...)` expecting
source-gen, AOT-clean dispatch, or named pipelines. `<Version>` in this tree is
`14.0.0-beta.1`. nuget.org serves that as a **prerelease**; **13.0.0** remains the last
stable reflection line.

GitHub issue [#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52)
stays **open** until a **stable 14.0.0**. This beta does not close that issue.

## Packages for a generated host

```xml
<ItemGroup>
  <PackageReference Include="TimeWarp.Mediator.Contracts" Version="14.0.0-beta.1" />
  <PackageReference Include="TimeWarp.Mediator.Generators" Version="14.0.0-beta.1" />
</ItemGroup>
```

| Package | Role |
|---------|------|
| `TimeWarp.Mediator.Contracts` | `IRequest` / `ISender` / `ISender<TScope>` / membership attributes. Required at compile and runtime. |
| `TimeWarp.Mediator.Generators` | Source generator + `buildTransitive` props. Development dependency; emits `AddGeneratedMediator` into `Microsoft.Extensions.DependencyInjection`. Packs `TimeWarp.Mediator.Analyzers.dll` next to the generator. |
| `TimeWarp.Mediator.Analyzers` | TWM001–TWM004. Add this package **when the generator is not referenced** (a contracts/library project that should fail the build on a missing handler without emitting a mediator). |
| `TimeWarp.Mediator` | Reflection runtime and `AddMediator()`. Not required for a generated-only host. |

The Generators nupkg is `DevelopmentDependency` and suppresses package dependencies, so
Contracts is **not** transitive. Analyzers are not a NuGet dependency of Generators either;
they ride along as analyzer DLLs inside that nupkg. A library that does not reference
Generators must add `TimeWarp.Mediator.Analyzers` itself.

A reflection host (13.x fork behavior) still uses:

```xml
<PackageReference Include="TimeWarp.Mediator" Version="13.0.0" />
```

`TimeWarp.Mediator` type-forwards contracts types and pulls Contracts as a package
dependency.

## Membership

The compile-time graph includes an assembly only when it opts in.

```csharp
[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(LoggingBehavior<,>))]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
```

| Attribute | Target | Meaning |
|-----------|--------|---------|
| `[assembly: MediatorAssembly]` | assembly | This assembly is a graph member (handlers, requests, behaviors). |
| `[MediatorScope(typeof(TScope))]` | assembly, type | Assigns the type (or the assembly default) to a named pipeline. |
| `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]` | assembly | Compile-time pipeline behavior. Optional `Scope = typeof(TScope)` binds it to one named pipeline; omitted `Scope` is the unscoped default only. |

Hosts that reference `TimeWarp.Mediator.Generators` get `TimeWarpMediatorAssembly=true`
from `buildTransitive/TimeWarp.Mediator.Generators.props`, so they are members without
writing `[assembly: MediatorAssembly]`. Domain libraries that only reference Analyzers
**must** add `[assembly: MediatorAssembly]` (or `[assembly: MediatorAssemblies(typeof(...))]`
on a host that lists them).

`[MediatorModule("Orders")]` is graph membership (the declaring assembly joins the linker).
It does **not** name a pipeline.

## Named pipelines (marker types, not strings)

Pipelines are empty marker types. There is no string pipeline name.

```csharp
public sealed class ClientPipeline
{
}

public sealed class ServerPipeline
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPing : IRequest<string>
{
}

[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
```

```csharp
services.AddGeneratedMediator();
services.AddGeneratedMediator<ClientPipeline>();
services.AddGeneratedMediator<ServerPipeline>();

ServiceProvider provider = services.BuildServiceProvider();
ISender unscoped = provider.GetRequiredService<ISender>();
ISender<ClientPipeline> client = provider.GetRequiredService<ISender<ClientPipeline>>();
ISender<ServerPipeline> server = provider.GetRequiredService<ISender<ServerPipeline>>();
```

`AddGeneratedMediator()` registers only the unscoped sender/publisher/mediator.
`AddGeneratedMediator<ClientPipeline>()` does not dispatch `ServerPipeline` handlers.
Wrong-scope typed `Send` is **TWM004** (compile error); `Send(object)` of a foreign-scope
request throws `NoHandlerException`.

Full M2 notes: [m2-named-pipelines.md](./m2-named-pipelines.md). Sample:
`samples/timewarp-mediator-examples-named-pipelines`.

## See also

- [m1-generated-mediator.md](./m1-generated-mediator.md) — generated dispatcher, analyzers, AOT sample
- [m2-named-pipelines.md](./m2-named-pipelines.md) — `ISender<TScope>` / `IPublisher<TScope>`
- [migration.md](../migration.md) — MediatR → TimeWarp.Mediator **13.x reflection** rename
