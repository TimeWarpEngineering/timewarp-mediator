# Nuru's use of TimeWarp.Mediator

Nuru uses Mediator mainly as a contract package. Its own source generator and pipeline do the dispatch, and the generated mediator is only an optional DI-injected service. All paths below are under `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-nuru/master`, and nothing was changed.

## 1. Packages and versions

| Package | Reference | Version |
|---|---|---|
| `TimeWarp.Mediator.Contracts` | `source/timewarp-nuru/timewarp-nuru.csproj:59` | 14.0.0-beta.5 (`Directory.Packages.props:33`) |
| `TimeWarp.Mediator.Generators` | `source/timewarp-nuru/timewarp-nuru.csproj:60`, with `PrivateAssets="none"` so the generator flows into consuming apps | 14.0.0-beta.5 (`Directory.Packages.props:34`) |

- Nuru does not reference `TimeWarp.Mediator` (the reflection runtime) or `TimeWarp.Mediator.Analyzers` directly.
- Nuru is not yet on 14.0.0 stable. Kanban 489 (`kanban/to-do/489-*.md`) is the open gate to decide that. It still says beta.4, which is stale because task 498 moved the pins to beta.5.
- `source/timewarp-nuru/timewarp-nuru.csproj:69` sets `TimeWarpMediatorAssembly=false`. Nuru owns no handlers, so it does not emit `AddGeneratedMediator()` into its own library.

## 2. Inventory of Mediator types used

| Type | Where and how |
|---|---|
| `ICommand<T>`, `IQuery<T>`, `IIdempotentCommand<T>` | Message markers on `[NuruRoute]` endpoint classes. About 154 `ICommand<Unit>` and 33 `IQuery<Unit>` in samples and tests. Shipped endpoints are in `source/timewarp-nuru-search/endpoints/*.cs` and `source/timewarp-nuru-devcli/content/any/endpoints/*.cs`. |
| `ICommandHandler<,>`, `IQueryHandler<,>`, `IIdempotentCommandHandler<,>` | Nested `Handler` classes, for example `search-query.cs:32` and `clean-command.cs:22`. The generator detects them by name in `add-configuration-locator.cs:313`. |
| `IIdempotent` | Marker on endpoints. Read by the generator in `endpoint-extractor.cs:730`. Test: `tests/timewarp-nuru-tests/capabilities/capabilities-07-mediator-message-kind.cs:133-155`. |
| `Unit` | Return type of most handlers, used as `Unit.Value` and `Unit.Task`. The generator special-cases `global::TimeWarp.Mediator.Unit` as "no output" at `handler-invoker-emitter.cs:280, 696, 765`. The JSON-context task does the same at `source/timewarp-nuru-build/GenerateNuruJsonContextTask.cs:328`. |
| `ISender`, `IPublisher`, `IMediator` | Injected into handlers and lambda delegates. About 28, 13 and 14 uses, mostly in tests. Resolution logic is in `generators/models/framework-services.cs:10-29` and the emitters listed in section 3. Tests: `generator-46-generated-mediator.cs`, `generator-47-*`, `generator-51-repl-service-scopes.cs:393`. |
| `INotification`, `INotificationHandler<>` | Only in `generator-46-generated-mediator.cs:34-43`, plus `IPublisher.Publish` there. Nothing in `source/` uses them. |
| `AddGeneratedMediator()` | Emitted into generated code only (section 3). |
| `GetRequiredService<IMediator>` | Emitted at `interceptor-emitter.cs:484`. |

**Types that do not appear anywhere in `source/`, `samples/` or `tests/`:**
- `IRequest`, `IRequestHandler`, `IAction`, `IStreamRequest`
- `IPipelineBehavior`, `RequestHandlerDelegate`
- `MediatorScope`, `MediatorBehavior`, `MediatorModule`, `MediatorAssembly` (as attributes)
- `ISender<TScope>`, `NoHandlerException`, `AddMediator`

## 3. Nuru's generator emits code that references Mediator

**Detection** (`source/timewarp-nuru-analyzers/generators/extractors/generated-mediator-detector.cs:12-20`):
- It reads the MSBuild properties `build_property.TimeWarpMediatorAssembly` and `build_property.TimeWarpMediatorProfile`.
- It requires the `TimeWarp.Mediator.ISender` metadata name.
- It returns true unless the Aot or Link profile is active, because those profiles do not emit `AddGeneratedMediator()`.
- The result feeds `nuru-generator.cs:102-125` and the model flag `HasGeneratedMediator` (`generator-model.cs:13-23`).

**Runtime-DI apps** (`.UseMicrosoftDependencyInjection()`): `interceptor-emitter.cs:830-836` emits `GeneratedMediatorServiceCollectionExtensions.AddGeneratedMediator(services)` into the generated service-collection setup.

**Source-gen DI apps** (`interceptor-emitter.cs:431-486`, `EmitSourceGenMediator`):
- It emits a static `__GetMediator()` that lazily builds a private `ServiceCollection`.
- That collection holds Nuru's `ITerminal`, `NuruApp`, `IConfiguration`, `ILoggerFactory` and `ILogger<>` as transients.
- It also holds every statically registered service as a transient factory, followed by `AddGeneratedMediator()`.
- It then calls `GetRequiredService<global::TimeWarp.Mediator.IMediator>`.
- If the app never injects a mediator type, no container code is emitted. The gate is `SourceGenDIRequiresMediator` (`generator-model.cs:56-82`).

**Injection sites:** `service-resolver-emitter.cs:92-96, 356, 391`, `handler-invoker-emitter.cs:440`, and `behavior-emitter.cs:394` all substitute `__GetMediator()` for `ISender`, `IPublisher` and `IMediator` parameters.

**Validation:** `service-validator.cs:183-190` treats those three types as always registered when `hasGeneratedMediator` is true.

**Classification** (`endpoint-extractor.cs:690-746, 1185-1240`):
- Message kind is inferred from interface names in the `TimeWarp.Mediator` namespace. The precedence is Query, then IdempotentCommand, then Command.
- The return type is taken from `TypeArguments[0]` of `IQuery`, `ICommand` and `IIdempotentCommand`.
- Every `TimeWarp.Mediator` interface is excluded from the behavior-filter interface list.

Nuru does not emit calls to `Send` or `Publish`. Its generated code calls the nested `Handler` directly through its own invoker.

## 4. Workarounds, shims and complaints

- **Single `AddGeneratedMediator()`:** `timewarp-nuru.csproj:65-69` and `changelog.md:38` explain that the generator package's host types are `internal` in beta.4 and later, so apps do not see two `AddGeneratedMediator()` methods. That is why the library sets the assembly flag false.
- **CS0436 suppression:** `tests/Directory.Build.props:15-17` and `tests/ci-tests/Directory.Build.props:15-17` suppress it. Test assemblies generate their own `TimeWarp.Mediator.Generated.Mediator`, which collides with the internal copy visible through InternalsVisibleTo in TimeWarp.Nuru and TimeWarp.Nuru.Mcp.
- **Endpoint accessibility:** `changelog.md:43` says `[NuruRoute]` endpoints must be `public`. The Mediator generator emits public `Send(TRequest)` overloads, so internal requests fail with CS0051 (timewarp-mediator#64).
- **`Unit` collision:** `changelog.md:42` says `using static TimeWarp.Mediator.Unit` hides `Task`, so use `Unit.Value` and `Unit.Task`. Nuru also needed a fully-qualified `Unit` fix in the emitter (done task 442).
- **Handler return type:** `changelog.md:41` notes the move from `ValueTask<T>` to `Task<T>` as a breaking change.
- **Search-query comment:** `search-query.cs:5` explains why the handler returns `Unit`.
- **Unit-return special-casing:** it is repeated in three places (the emitter, the JSON-context task, the extractor) as string comparisons on `"global::TimeWarp.Mediator.Unit" or "Unit"`.
- There are no TODO or HACK comments about Mediator in `source/`.

## 5. Features used and not used

**Used:** message and handler contracts, `Unit`, `IIdempotent`, and injection of `ISender`, `IPublisher` and `IMediator` as optional services.

**Not used:**
- Pipeline behaviors. Nuru has its own `INuruBehavior` and `BehaviorContext` in `source/timewarp-nuru/abstractions/behavior-interfaces.cs` and `behavior-context.cs`, built on `ValueTask HandleAsync(BehaviorContext, Func<ValueTask>)`. Examples are in `samples/*/05-pipeline/`. It is generated inline and has no relation to `IPipelineBehavior`.
- Named pipelines, `ISender<TScope>` and the membership attributes. Task 443 (`kanban/done/443-extract-mediator-abstractions-*/task.md:5, 35, 43`) lists `ISender<TScope>` as out of scope and "Nuru named pipelines" as an optional later item.
- Streams and `NoHandlerException`.
- Notifications beyond one test.
- The reflection runtime.
- The Aot, Host and Link profile choice. Nuru only reads the profile to tell whether `AddGeneratedMediator()` exists.

## 6. Documentation and kanban

- `kanban/done/443-extract-mediator-abstractions-to-timewarp-mediator-with-source-generated-dispatch/task.md` and `kanban/done/443-001-*`: the integration design. Nuru consumes Contracts and Generators, and Nuru-owned interfaces were deleted in favor of Mediator's.
- `kanban/done/475-*` and `kanban/done/498-*/task.md`: the beta.4 and beta.5 bump tasks.
- `kanban/to-do/489-*.md`: the open prerelease-dependency decision.
- `changelog.md:12, 36-50`: the user-facing migration notes quoted in section 4.
- `skills/tw-nuru/SKILL.md:65` and `documentation/developer/design/source-generators/endpoint-generator.md:56`: the contracts live in `TimeWarp.Mediator`, and handlers return `Task<T>`.
- `kanban/done/304-*` and `kanban/done/330-remove-mediator-dependency.md`: history. Nuru once dropped Mediator and then returned to it in task 443.
- Most `documentation/` hits (such as `architecture-choices.md`) use "mediator" as a generic pattern name and are not design inputs.

## 7. Dependency injection

- Nuru references `Microsoft.Extensions.DependencyInjection.Abstractions` at `timewarp-nuru.csproj:45`. It also ships `AddSessionScoped` in `dependency-injection/nuru-service-collection-extensions.cs`.
- Nuru has no full container of its own. By default it uses source-generated, container-free DI: static fields and constructor calls emitted by the generator. `.UseMicrosoftDependencyInjection()` is the opt-in runtime container, so most Nuru apps run without MS DI.
- The generated `AddGeneratedMediator()` needs an `IServiceProvider` to resolve handlers. Nuru therefore builds a hidden MS DI `ServiceCollection` just for mediator injection in source-gen apps (`interceptor-emitter.cs:431-486`). Only that path forces a MS DI container onto otherwise container-free apps.

## Observations for 15.0

- **Contracts are the real dependency.** The hot path is `ICommand<T>`, `IQuery<T>`, `IIdempotentCommand<T>`, `IIdempotent`, the three handler interfaces and `Unit`. Renames or shape changes there break every endpoint and the generator's string-based detection.
- **Detection is by namespace and interface name.** Nuru matches on `ContainingNamespace == "TimeWarp.Mediator"` and names such as `IIdempotentCommand` (`endpoint-extractor.cs:690-746`). Moving types to another namespace or renaming them silently breaks classification and filter-interface exclusion.
- **Coupling to Mediator build properties.** Nuru depends on `TimeWarpMediatorAssembly` and `TimeWarpMediatorProfile`. A stable, documented signal would be better than reading these `build_property.*` names.
- **The MS DI bridge is the main friction.** It would help to have a generated mediator that resolves handlers without `IServiceProvider`, or one that accepts a service-resolver delegate. Either would remove the hidden `ServiceCollection` in `EmitSourceGenMediator`.
- **A public message-kind enum or interface split would help.** Nuru's Query, Command and IdempotentCommand categories are re-derived by hand. The `IIdempotentCommand` derives-from-`ICommand` precedence trap already caused a bug.
- **Endpoint accessibility.** Nuru wants `internal` endpoints, but the generator emits public `Send` overloads. The issue is timewarp-mediator#64.
- **Pipelines may never be used by Nuru.** It has a separate `INuruBehavior` system built around `BehaviorContext`. Treat `ISender<TScope>` and named pipelines as speculative for Nuru, not as a driver.
- **`Unit` ergonomics.** `Unit.Task` and `Unit.Value` are used heavily. Nuru would like `Unit` to avoid hiding `Task` under `using static`.
