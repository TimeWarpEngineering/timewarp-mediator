# TimeWarp.Mediator 14.0.0 release notes

14.0.0 is in beta on nuget.org as a prerelease; **13.0.0** is still the last stable release. This
file collects the changes across the 14.0.0 betas, newest first. GitHub Releases carry the
generated per-tag notes.

The unpublished GitHub Release body for the stable tag is
[github-release-v14.0.0-draft.md](./github-release-v14.0.0-draft.md). Do not publish it until
14.0.0 is on nuget.org and Steven has approved the post. `dev release` still generates its own
notes; replace that body with the draft.

> **14.0.0-beta is not a drop-in for 13.0.0.** The 14.0.0 line adds a second, source-generated
> dispatcher (`AddGeneratedMediator()`). It is proven only against the golden files, tests and
> samples in this repository. Do not bump a 13.0.0 host to 14.0.0-beta and keep
> `AddMediator(...)` expecting source generation, AOT-clean dispatch or named pipelines; those
> need `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()`. See
> [generated-vs-legacy.md](../generated-vs-legacy.md). Issue
> [#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52) stays open until a
> stable 14.0.0.

## 14.0.0-beta.5

Published 2026-10-13. Everything merged on `master` after 14.0.0-beta.4, including the 22 children of the repository review (kanban 013, PR #95).

### Breaking changes

#### net10.0 minimum

`TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` target `net10.0` only. The netstandard2.0
and net6.0 assets are gone, and so is the `Microsoft.Bcl.AsyncInterfaces` dependency. Projects
that cannot target net10.0 or newer stay on 13.x. `TimeWarp.Mediator.Generators` and
`TimeWarp.Mediator.Analyzers` stay netstandard2.0 because Roslyn loads them inside the compiler;
their analyzer payload does not depend on your app's target framework.

#### License expression

`TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` now declare
`Unlicense AND Apache-2.0`. `TimeWarp.Mediator.Generators` and
`TimeWarp.Mediator.Analyzers` stay `Unlicense`. Every package packs `notice` and `unlicense`.

#### Trim warnings on the reflection runtime

`AddMediator(...)` (both overloads), the `Mediator` constructors, and the constructors of
`RequestExceptionProcessorBehavior<,>` and `RequestExceptionActionProcessorBehavior<,>` are
annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. A trimmed or NativeAOT host
that still calls them gets IL2026 / IL3050. The generated path does not. See
[generated-vs-legacy.md](../generated-vs-legacy.md).

#### Smaller public API in the `TimeWarp.Mediator` runtime package

Types that only the runtime uses are no longer public. Code that only uses `IMediator`, `ISender`,
`IPublisher`, `AddMediator(...)`, `MediatorServiceConfiguration` and the pipeline contracts needs
no change. `TimeWarp.Mediator.Contracts`, `.Generators` and `.Analyzers` are unchanged.

| Type | Change | Replacement |
|---|---|---|
| `ServiceFactory` delegate | Removed. Nothing in the library used it. | Resolve through `IServiceProvider`. If a container sample needs a resolver delegate, declare it in your own code. |
| `TimeWarp.Mediator.Wrappers.RequestHandlerBase`, `RequestHandlerWrapper`, `RequestHandlerWrapper<TResponse>`, `RequestHandlerWrapperImpl<TRequest>`, `RequestHandlerWrapperImpl<TRequest, TResponse>` | Now `internal` | Call `ISender.Send(...)`. |
| `TimeWarp.Mediator.Wrappers.NotificationHandlerWrapper`, `NotificationHandlerWrapperImpl<TNotification>` | Now `internal` | Call `IPublisher.Publish(...)`. To change how handlers run, implement `INotificationPublisher` or override `Mediator.PublishCore`. |
| `TimeWarp.Mediator.Registration.ServiceRegistrar` (including `AddMediatorClassesWithTimeout`, `AddMediatorClasses`, `GenerateCombinations`, `AddRequiredServices`) | Now `internal` | Call `services.AddMediator(cfg => ...)`. |

These types stay public because consumers use them directly: `Mediator`, `INotificationPublisher`,
`NotificationHandlerExecutor` (part of the `INotificationPublisher` and `PublishCore` signatures),
`ForeachAwaitPublisher` and `TaskWhenAllPublisher` (values for
`MediatorServiceConfiguration.NotificationPublisher`), `MediatorServiceConfiguration`,
`OpenBehavior`, `RequestExceptionActionProcessorStrategy`, `RequestExceptionHandlerState<TResponse>`,
the pre/post/exception processor interfaces, and the four built-in behaviors. Containers other
than Microsoft.Extensions.DependencyInjection register those behaviors by hand.

### Fixes

#### Generator: struct requests compile

A request declared as a `struct` (for example `readonly struct Measure : IRequest<int>`) made the
generated `Mediator.g.cs` fail with CS0037, because the typed `Send(...)` overload and the dispatch
method null-checked the request. The generator now omits that check for value-type requests.

### Tooling

- The generator and analyzers require Roslyn 4.8 (**.NET SDK 8.0.100** / **Visual Studio 2022
  17.8**) or later. A test leg now compiles against Microsoft.CodeAnalysis.CSharp 4.8.0.
- `AnalyzerReleases.Shipped.md` records TWM001–TWM004 under `## Release 14.0.0`, noted as shipped in
  14.0.0-beta.1 (release headers cannot carry a prerelease label).

## 14.0.0-beta.4

### Generated types are internal

The generator now emits every generated type as `internal` in every profile (Host, Aot, Link):
`Mediator`, `Sender_*` / `Publisher_*`, and `MediatorManifest`. The Host profile also emits
internal `GeneratedMediatorServiceCollectionExtensions`. Aot and Link do not emit that type, so
they have no `AddGeneratedMediator()`. Every host emits the same type names, so public types
caused CS0436 warnings, and ambiguous `AddGeneratedMediator()` calls (CS0121), in any compilation
that referenced two hosts, for example a test project that references two apps.

- On the Host profile, `AddGeneratedMediator()` stays callable from the host's own code.
  Assemblies that reference the host no longer see it; resolve `IMediator` / `ISender` /
  `IPublisher` instead.
- `InternalsVisibleTo` re-exposes the types, so a friend of two hosts still sees duplicates.
- A build step that removed the generator from a library compile only to avoid an ambiguous public
  `AddGeneratedMediator()` is no longer needed.

## 14.0.0-beta.3

Generator fixes for [#68](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/68):

- An explicit `ICommand<Unit>` handled by `IRequestHandler<TCommand, Unit>` is registered and
  dispatched as the two-arity handler. It previously emitted the void `IRequestHandler<TCommand>`
  and failed with CS0311. Void dispatch now comes only from `IRequestHandler<TRequest>`.
- Handlers whose response is not a named type (for example `IQuery<SearchResult[]>`) are
  discovered and dispatched, and pipeline behaviors close over those responses. They previously
  reported TWM001.

## 14.0.0-beta.2

### Idempotency contracts

`TimeWarp.Mediator.Contracts` adds `IIdempotent`, `IIdempotentCommand` / `IIdempotentCommand<T>`
and `IIdempotentCommandHandler<>` / `IIdempotentCommandHandler<,>`. `IQuery<T>` now implements
`IIdempotent`. Both dispatchers treat idempotent commands exactly like `ICommand`. These are types
only: there is no idempotency-key enforcement or dedup store.

## 14.0.0-beta.1

The first 14.0.0 prerelease. It ships four packages instead of two:

| Package | Role |
|---|---|
| `TimeWarp.Mediator.Contracts` | Contracts for both dispatchers. Required by a generated host at compile time and run time. |
| `TimeWarp.Mediator.Generators` | Source generator (new). Emits the generated mediator and `AddGeneratedMediator()`. Bundles the analyzers. |
| `TimeWarp.Mediator.Analyzers` | Roslyn analyzers (new) for libraries that do not run the generator. |
| `TimeWarp.Mediator` | The 13.x reflection runtime and `AddMediator()`. Not needed by a generated-only host. |

### Source-generated mediator (M1)

- The generator builds the message graph at compile time and emits a sealed, reflection-free
  `TimeWarp.Mediator.Generated.Mediator : IMediator` with monomorphic `Send`, a `Send(object)`
  switch, an embedded `MediatorManifest` and `AddGeneratedMediator()`.
- Assemblies opt in with `[assembly: MediatorAssembly]`; compile-time pipeline behaviors are
  registered with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`.
- The `Aot` profile (`<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>`) dispatches through
  static fields with no `IServiceProvider` and is trim and NativeAOT safe
  (`samples/timewarp-mediator-examples-aot`).
- Contracts add `IAction` / `IActionHandler` / `ActionHandler`, `ICommand` / `IQuery` and their
  handlers, the membership attributes and `NoHandlerException`. `ISender`, `IPublisher`,
  `IMediator` and the handler and pipeline interfaces moved to Contracts and are type-forwarded
  from `TimeWarp.Mediator`.

### Named pipelines (M2)

`[MediatorScope(typeof(TScope))]` assigns messages to a named pipeline, served by
`ISender<TScope>` / `IPublisher<TScope>` and registered with `AddGeneratedMediator<TScope>()`.
See [m2-named-pipelines.md](../m2-named-pipelines.md).

### Analyzers

TWM001 (request with no handler), TWM002 (duplicate handler), TWM003 (scope mismatch) and TWM004
(wrong-scope send). See [diagnostics.md](../diagnostics.md).

### Reflection stack

`AddMediator()` and the reflection `Mediator` are unchanged from 13.0.0 apart from the interfaces
moving to Contracts. It is a separate dispatcher; calling it does not register the generated
mediator.
