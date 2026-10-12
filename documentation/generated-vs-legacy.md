# Generated vs reflection `AddMediator`

This page is the single source for the "two stacks" story: which registration gives which
dispatcher, which packages each needs, membership, and named pipelines. Other pages link here
instead of repeating it.

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

Moving a 13.x host to the generated stack, and what it does not support yet:
[migration.md](../migration.md#13x-reflection-to-14x-generated).

## Target framework

14.x requires **net10.0 or newer** for both stacks: `TimeWarp.Mediator` (reflection) and
`TimeWarp.Mediator.Contracts` (generated) target `net10.0` only. 13.0.0 is the line for
netstandard2.0 and net6.0 consumers. The Generators and Analyzers packages stay netstandard2.0
because Roslyn loads them inside the compiler process; that does not change the consumer floor.

## Trimming and NativeAOT

The reflection runtime (`TimeWarp.Mediator`) is **not** trim or NativeAOT safe. It scans
assemblies, closes generic wrapper and handler types with `MakeGenericType`, creates them with
`Activator.CreateInstance`, and invokes exception handlers and actions through `MethodInfo`.

The reflection entry points carry `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`:

- `services.AddMediator(...)` (both overloads)
- the `Mediator` constructors
- the `RequestExceptionProcessorBehavior<,>` and `RequestExceptionActionProcessorBehavior<,>` constructors

A host that publishes with `PublishTrimmed` or `PublishAot` (or sets `IsAotCompatible`) gets
**IL2026** / **IL3050** at each call site. Do not suppress them: switch the host to the
generated path.

The generated path (`AddGeneratedMediator()` from `TimeWarp.Mediator.Contracts` +
`TimeWarp.Mediator.Generators`) is trim and NativeAOT safe. It does not reference the
`TimeWarp.Mediator` assembly. `samples/timewarp-mediator-examples-aot` publishes with
`PublishAot`, `IsAotCompatible`, and the trim/AOT analyzers on, with no IL2026/IL3050 `NoWarn`.

### Aot / Link profile limits

`<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>` (or `Link`) emits no
`AddGeneratedMediator()` and no `IServiceProvider`: `new Mediator()` dispatches through static
`ServiceGen` fields.

- **Handlers are process-wide singletons.** Each handler is created once with `new Handler()`
  into a `static readonly` field and shared by every `Mediator` instance and every thread. Keep
  Aot handlers stateless (or thread-safe).
- **Handlers must be constructible with `new Handler()`.** An accessible parameterless
  constructor is enough (optional parameters are fine). A handler with required constructor
  parameters, only a private constructor, or a `required` member that constructor does not set
  (`[SetsRequiredMembers]`) is **TWM006** (error) at the handler. Use the Host profile when
  handlers take DI dependencies.
- **`[assembly: MediatorBehavior]` is not woven.** Each registered behavior that would close
  over a request is **TWM005** (warning) at the attribute, and `MediatorManifest.Json` lists
  `"behaviors":[]` for every request so the manifest matches the emitted pipeline. Validation,
  transaction, and logging behaviors need the Host profile.

## 14.0.0-beta is not a drop-in for 13.0.0

During the 14.0.0 prereleases, the generated stack is proven only against the M1/M2 golden
files in this repo:

- Generator tests (`tests/timewarp-mediator-generators-tests`), including State-shaped
  `IncrementActionSet` + `StateTransactionBehavior`
- Named-pipeline tests and `samples/timewarp-mediator-examples-named-pipelines`
- AOT sample `samples/timewarp-mediator-examples-aot` (ServiceGen static fields; no
  `AddMediator`)

It is **not** an API-compatible upgrade of NuGet **13.0.0** (the last published reflection
line). Do not bump a 13.0.0 host to 14.0.0-beta and keep `AddMediator(...)` expecting
source-gen, AOT-clean dispatch, or named pipelines. nuget.org serves every 14.0.0 beta as a
**prerelease**; **13.0.0** remains the last stable reflection line.

GitHub issue [#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52)
stays **open** until a **stable 14.0.0**. This beta does not close that issue.

Per-beta changes and unreleased work are in the
[14.0.0 release notes](./release-notes/release-notes-v14.0.0.md).

## Packages for a generated host

Use the same version for every TimeWarp.Mediator package. While 14.0.0 is in prerelease, add
`--prerelease` to pick the latest beta:

```bash
dotnet add package TimeWarp.Mediator.Contracts --prerelease
dotnet add package TimeWarp.Mediator.Generators --prerelease
# library-only (no generator):
dotnet add package TimeWarp.Mediator.Analyzers --prerelease
```

| Package | Role |
|---------|------|
| `TimeWarp.Mediator.Contracts` | `IRequest` / `ISender` / `ISender<TScope>` / membership attributes. Required at compile and runtime. |
| `TimeWarp.Mediator.Generators` | Source generator + `buildTransitive` props. Development dependency; emits `AddGeneratedMediator` into `Microsoft.Extensions.DependencyInjection`. Packs `TimeWarp.Mediator.Analyzers.dll` next to the generator. |
| `TimeWarp.Mediator.Analyzers` | TWM001–TWM004 (the generator adds TWM005–TWM011; see [diagnostics.md](./diagnostics.md)). Add this package **when the generator is not referenced** (a contracts/library project that should fail the build on a missing handler without emitting a mediator). |
| `TimeWarp.Mediator` | Reflection runtime and `AddMediator()`. Not required for a generated-only host. |

The Generators nupkg is `DevelopmentDependency` and suppresses package dependencies, so
Contracts is **not** transitive. Analyzers are not a NuGet dependency of Generators either;
they ride along as analyzer DLLs inside that nupkg. A library that does not reference
Generators must add `TimeWarp.Mediator.Analyzers` itself.

### Minimum SDK / IDE

`TimeWarp.Mediator.Generators` and `TimeWarp.Mediator.Analyzers` are built against
**Microsoft.CodeAnalysis.CSharp 4.8.0**, so the host compiler must be Roslyn 4.8 or later:
**.NET SDK 8.0.100** or later, or **Visual Studio 2022 17.8** or later (or a Rider/MSBuild that
ships Roslyn 4.8+). On an older compiler the generator does not load (CS9057) and no mediator is
emitted. CI runs the generator compilation tests against 4.8.0 as well as the current compiler.

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
- [migration.md](../migration.md) — MediatR → TimeWarp.Mediator 13.x, and 13.x reflection → 14.x generated
- [diagnostics.md](./diagnostics.md) — TWM001–TWM011
- [m1-benchmark-gap.md](./m1-benchmark-gap.md) — legacy vs generated vs martinothamar dispatch cost;
  the recorded baseline (commit SHA, date, host) is [benchmarks/comparison-baseline.md](./benchmarks/comparison-baseline.md)
- [samples/readme.md](../samples/readme.md) — which samples use which stack
