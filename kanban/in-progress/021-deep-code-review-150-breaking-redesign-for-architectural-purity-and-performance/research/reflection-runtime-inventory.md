# Inventory of `source/timewarp-mediator`: compatibility debt vs essential code

Inventory of the reflection runtime in `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite`. Nothing was modified. The 27 `.cs` files plus the csproj total 2228 lines, including doc comments.

## 1. Per-file table

| File | Lines | What it does | Class |
|---|---|---|---|
| `mediator.cs` | 208 | `Send` / `Publish` / `CreateStream`, five static `ConcurrentDictionary` wrapper caches | ESSENTIAL (the `object` and stream overloads are COMPAT) |
| `wrappers/request-handler-wrapper.cs` | 67 | Closed generic wrapper: resolves the handler, aggregates the pipeline | ESSENTIAL |
| `wrappers/notification-handler-wrapper.cs` | 24 | Resolves `INotificationHandler<T>` and builds executors | ESSENTIAL |
| `wrappers/stream-request-handler-wrapper.cs` | 71 | Stream handler plus stream pipeline | COMPAT (stream) |
| `notification-handler-executor.cs` | 10 | Record of handler instance and callback | COMPAT, exists for publisher strategies |
| `i-notification-publisher.cs` | 20 | Publisher strategy interface | COMPAT |
| `notification-publishers/*` | 22 and 38 | `ForeachAwaitPublisher`, `TaskWhenAllPublisher` | COMPAT |
| `pipeline/request-pre-processor-behavior.cs` | 31 | Runs pre-processors before `next` | COMPAT |
| `pipeline/request-post-processor-behavior.cs` | 33 | Runs post-processors after `next` | COMPAT |
| `pipeline/request-exception-processor-behavior.cs` | 104 | Exception handler cascade via `MethodInfo.Invoke` | COMPAT |
| `pipeline/request-exception-action-processor-behavior.cs` | 94 | Exception action cascade via `MethodInfo.Invoke` | COMPAT |
| `pipeline/request-exception-handler-state.cs` | 29 | Handled / response state for exception handlers | COMPAT |
| `pipeline/i-request-{pre,post}-processor.cs`, `i-request-exception-{handler,action}.cs` | 17, 19, 23, 21 | Interfaces for the four processor kinds | COMPAT (they live in the runtime, not Contracts) |
| `internal/handlers-orderer.cs` | 47 | Orders and de-duplicates exception handlers | COMPAT |
| `internal/object-details.cs` | 148 | Assembly and namespace proximity comparer | COMPAT |
| `internal/trimming-messages.cs` | 15 | Text for the IL2026 / IL3050 annotations | DIAGNOSTIC |
| `microsoft-extensions-di/mediator-service-configuration.cs` | 448 | Options object, with about 40 public members | DI-PLUMBING |
| `microsoft-extensions-di/service-collection-extensions.cs` | 125 | `AddMediator` x2, `GetPipelineInfo` | DI-PLUMBING |
| `microsoft-extensions-di/request-exception-action-processor-strategy.cs` | 18 | Two-value enum | DI-PLUMBING |
| `registration/service-registrar.cs` | 459 | Assembly scan, open-generic closing, behavior registration | DI-PLUMBING |
| `entities/open-behavior.cs` | 50 | Value type for `AddOpenBehaviors(IEnumerable<OpenBehavior>)` | DI-PLUMBING |
| `type-forwardings.cs` | 41 | 38 type forwards to Contracts (the 13.x to 14.x binary compatibility shim) | COMPAT |
| `global-usings.cs`, `timewarp-mediator.csproj` | 21, 25 | Housekeeping | OTHER |

**Rough weight:**
- Essential core is about 300 lines: `mediator.cs` without the stream and object overloads, plus the two request and notification wrappers.
- DI-PLUMBING is about 1080 lines: configuration 448, registrar 459, extensions 125, plus the enum and `OpenBehavior`.
- The pipeline processors, exception cascade, orderer and `ObjectDetails` total about 560 lines.
- Streams plus the `object` overloads add about 140 lines.

## 2. Configuration and registrar knobs

**Configuration properties** (`mediator-service-configuration.cs`):

| Option | Line | Cost | Real use? |
|---|---|---|---|
| `TypeEvaluator` | 13 | One `.Where` per scan, 3 call sites in the registrar | Tests only: `type-evaluator-tests.cs`, 46 lines |
| `MediatorImplementationType` | 18 | One line in `AddRequiredServices` | Tests only: `custom-mediator-tests.cs` |
| `NotificationPublisher` (instance) | 23 | Descriptor branch in the registrar | Default only |
| `NotificationPublisherType` | 28 | Descriptor branch in the registrar | Tests only: 3 files |
| `Lifetime` | 33 | Used for IMediator / ISender / IPublisher descriptors | Not set by any sample. Handlers are always `AddTransient`. |
| `RequestExceptionActionProcessorStrategy` | 38 | 18-line enum plus a registrar branch at lines 404-413 | No tests, 1 doc mention |
| `BehaviorsToRegister`, `StreamBehaviorsToRegister`, `RequestPreProcessorsToRegister`, `RequestPostProcessorsToRegister` | 46-61 | Public mutable lists | Tests only |
| `AutoRegisterRequestProcessors` | 66 | Registrar lines 38-42 and 51-55 | One test |
| `MaxGenericTypeParameters` (default 10) | 71 | `GenerateCombinations`, lines 247-253 | Tests only. Docs mention it. |
| `MaxTypesClosing` (default 100) | 76 | Registrar lines 255-259 | Tests only |
| `MaxGenericTypeRegistrations` (default 125000) | 81 | Registrar lines 262-274 | Tests only |
| `RegistrationTimeout` (default 15000 ms) | 86 | `AddMediatorClassesWithTimeout`, lines 7-24 | Tests only, `generic-request-handler-tests.cs:153-176` |
| `RegisterGenericHandlers` (default false) | 91 | Gates the whole open-generic path | Tests only (about 10 uses). No sample sets it. |

**Methods in the configuration class (all with line numbers):**
- Assembly registration: `RegisterServicesFromAssembly*` (98-130).
- Behaviors, 8 overloads: `AddBehavior` x4 (141-185), `AddOpenBehavior` (198), `AddOpenBehaviors` x2 (207, 222).
- Stream behaviors, 5 overloads: `AddStreamBehavior` x4 (239-271), `AddOpenStreamBehavior` (294).
- Pre-processors, 5 overloads: `AddRequestPreProcessor` x4 (304-337), `AddOpenRequestPreProcessor` (360).
- Post-processors, 5 overloads: `AddRequestPostProcessor` x4 (370-402), `AddOpenRequestPostProcessor` (424).
- `AddOpenGeneric` helper plus `ThrowIfNull`.

Roughly 25 public methods are near-duplicate overloads, several differing only in the generic-arity convenience form.

**Sample usage.** `samples/timewarp-mediator-examples-asp-net-core/program.cs:20-22` is the only `AddMediator` call in `samples/`. It sets only `RegisterServicesFromAssemblies`. The other container samples (Lamar, Autofac and the rest) register by hand and never touch `MediatorServiceConfiguration`.

**ServiceRegistrar paths** (`service-registrar.cs`):
- `AddMediatorClasses` (26-74) runs `ConnectImplementationsToTypesClosing` for 6 interfaces, plus 2 more when `AutoRegisterRequestProcessors` is on. It then runs a second pass for open-generic multi-handlers.
- `ConnectImplementationsToTypesClosing` (76-158) has four lists (concretions, interfaces, genericConcretions, genericInterfaces) and a `addIfAlreadyExists` flag with two behaviors.
- The open-generic closing machinery, about 190 lines (199-327), is:
  - `GetConcreteRequestTypes`
  - `GenerateCombinations`, a recursive Cartesian product
  - `AddAllConcretionsThatClose`
  - `GetConcreteRegistrationTypes`
- `RegisterBehaviorIfImplementationsExist` (444-459) scans the whole `IServiceCollection` at registration time. Its own doc comment, lines 438-443, admits order-dependence.
- All four limit knobs (`MaxGenericTypeParameters`, `MaxTypesClosing`, `MaxGenericTypeRegistrations`, `RegistrationTimeout`) exist only to bound the combinatorial open-generic path. With `RegisterGenericHandlers` defaulting to false, that path is opt-in.

## 3. Runtime hot path

`Mediator.Send<TResponse>` (`mediator.cs:46-61`):

```csharp
RequestHandlerWrapper<TResponse> handler = (RequestHandlerWrapper<TResponse>) RequestHandlers.GetOrAdd((request.GetType(), typeof(TResponse)), static key => { ... MakeGenericType ... Activator.CreateInstance ... });
return handler.Handle(request, ServiceProvider, cancellationToken);
```

- The cache is a static `ConcurrentDictionary<(Type, Type), RequestHandlerBase>` (line 17). `MakeGenericType` and `Activator.CreateInstance` run once per request type.
- The per-call cost is a tuple-keyed dictionary lookup, which is a hash of two `Type`s.

`RequestHandlerWrapperImpl<TRequest,TResponse>.Handle` (`request-handler-wrapper.cs:292-303`) allocates on every call:

```csharp
Task<TResponse> Handler(CancellationToken t = default) => serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()...
return serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => (t) => pipeline.Handle(...))();
```

Per-call allocations:
- A closure capturing `request`, `serviceProvider` and `cancellationToken`.
- A delegate for the local function.
- `GetServices<>` (an array or enumerable).
- `.Reverse()` (LINQ buffer).
- `Aggregate`, which allocates one closure and one delegate per behavior.

The pipeline is rebuilt on every call. The handler and the behaviors are resolved from DI on every call, which is the actual per-call dependency lookup.

**Void and `object` paths.**
- The void path adds an `async` state machine (`request-handler-wrapper.cs:316-322`).
- `Send(object)` caches by request type and calls `GetInterfaces()` on first use (`mediator.cs:91-114`).
- The `object` wrapper wraps the typed call in `async`/`await` to box (lines 288-290, 309-311).

**Generated mediator** (`mediator-emitter.cs`):
- It emits one `switch (request)` for `Send(object)` (line 306).
- It emits a monomorphic typed `Send` returning `ValueTask<T>` (lines 274-281).
- Host profile: `GetRequiredService<Handler>(scope)` for each handler and each closed behavior (lines 426, 492, 498), with the behavior chain emitted as nested lambdas (`EmitNextDelegate`, 521-540).
- Aot profile: `ServiceGen.<field>` static readonly handler (lines 172-217, 483), with no `IServiceProvider` and no behaviors.

**Benchmarks** (`documentation/benchmarks/comparison-baseline.md`, commit `d478415`, 2026-10-12):

| Method | Mean | Allocated |
|---|---|---|
| TimeWarpLegacyMakeGenericType | 69.0 ns | 224 B |
| TimeWarpGeneratedMonomorphicSend | 29.9 ns | 96 B |
| TimeWarpCallSiteInliningPrototype | 28.9 ns | 96 B |
| MartinothamarGenerated | 8.77 ns | 24 B |

Generated is 2.3x faster and allocates 43% as much. The remaining gap to the third-party generated mediator (MartinothamarGenerated, about 3.4x faster than TimeWarp's generated) is not explained in the baseline file itself. The doc points to `m1-benchmark-gap.md` for that, which I did not read.

## 4. Notification publishers

- **Interface** (`i-notification-publisher.cs`): `Publish(IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken)`.
- **`ForeachAwaitPublisher`** (22 lines): sequential `await`. The first exception aborts the rest. It is the default.
- **`TaskWhenAllPublisher`** (38 lines): invokes all handlers, converts a synchronous throw to a faulted task via `Task.FromException`, then `Task.WhenAll`. All handlers run and every failure is observed.
- **`Mediator.PublishCore`** (`mediator.cs:148`) is a `protected virtual` extension point, an additional override route.
- **Per publish** (`notification-handler-wrapper.cs:257-261`): `GetServices` then a lazy `Select` that allocates an executor record and a closure per handler. The enumerable is lazy and is evaluated by the publisher.
- **Usage:** the publish-strategies sample registers four handlers and swaps publishers. The tests cover both publishers.

## 5. Exception processors, ObjectDetails, HandlersOrderer

These implement MediatR 12's exception-handling pipeline:
- `RequestExceptionProcessorBehavior` is the "handler" form. A handler can mark the exception handled via `RequestExceptionHandlerState.SetHandled(response)` and supply a response.
- `RequestExceptionActionProcessorBehavior` is the "action" form. It performs side effects, always rethrows (`throw;`), and cannot swallow the exception.
- Both walk the exception type chain from the concrete type up to `object`. For each type they resolve `IEnumerable<IRequestExceptionHandler<TReq,TRes,TExc>>` through `MakeGenericType` (cached per exception type in `HandlerReflectionCache`), de-duplicate by handler type, and invoke through `MethodInfo.Invoke`, unwrapping `TargetInvocationException`.
- `HandlersOrderer.Prioritize` (47 lines) removes overridden handlers when one handler type is assignable from another, then sorts with `ObjectDetails.Compare` (148 lines).
- `ObjectDetails.Compare` ranks handlers by assembly equality with the request's assembly, then namespace proximity, then namespace-prefix length. This is the MediatR "closest handler to the request wins" semantics.
- Total: about 460 lines (104 + 94 + 29 + 47 + 148 + 4 interfaces).
- Both behaviors are annotated `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` on their constructors.
- **Registration quirk:** a behavior is registered only if an implementation already exists in `services` at `AddMediator` time (`service-registrar.cs:444-459`).
- The generator does not support any of this (TWM009, `diagnostics.md:88-90`).

## 6. Streams

- **Code:** `stream-request-handler-wrapper.cs` (71 lines), `Mediator.CreateStream` x2 (`mediator.cs:164-208`), 5 `AddStreamBehavior` / `AddOpenStreamBehavior` overloads in the configuration class, plus the interfaces in Contracts (`i-stream-*.cs`). Runtime total is about 150 lines. Tests are `create-stream-tests.cs` 155 lines, `stream-pipeline-tests.cs` 366, `pipeline/streams/` 90, and `service-collection-stream-pipeline-tests.cs` 109.
- **Mechanics:** the wrapper resolves the handler eagerly. It then builds the stream pipeline per call with the same `Reverse().Aggregate` pattern. It re-wraps every `IAsyncEnumerable` in an `NextWrapper` async iterator at each layer (lines 388-399), which adds an iterator allocation per behavior plus one more.
- **Docs:** `generated-vs-legacy.md` and `migration.md:189` mark streams as reflection-only. The generated mediator throws `NotSupportedException` and reports TWM009. The docs do not use the word "legacy" for streams specifically. `mediator.cs:6-8` labels the whole `Mediator` class "Legacy reflection dispatcher", and the samples readme labels the whole stack "legacy".

## 7. Unit

- **Definition:** `source/timewarp-mediator-contracts/unit.cs`, a `readonly struct Unit` with `IEquatable`, `IComparable`, a `Value` singleton, and a cached `Task<Unit>` (lines 8-20). It is type-forwarded in the runtime (`type-forwardings.cs:7`).
- **Runtime use:** only the void-request wrapper (`request-handler-wrapper.cs:51-65`). It adapts `IRequestHandler<TRequest>` (void) onto the `IPipelineBehavior<TRequest, Unit>` pipeline.
- **Generator use:** `mediator-emitter.cs:514` and `:533` emit `Unit.Value` for `IsUnitResponse` requests, so `ISender.Send<TRequest>` (void) can funnel through `Send(object)`, and there is one `Task Send` shim at lines 240-253. The analyzer comment at `message-graph-builder.cs:18-19` records that void dispatch comes from matching `IRequestHandler<T>`, not from `TResponse == Unit`.
- **Net:** `Unit` is a compatibility device that makes void and typed requests share the `IPipelineBehavior<,>` shape. `IRequest` is a separate path in both dispatchers.

## 8. Other legacy markers

- **Apache notices:** 36 files under `source/` carry `// Modified by Steven T. Cramer`. The csproj states "Derived from Apache-2.0 MediatR code" and uses `Unlicense AND Apache-2.0` (`timewarp-mediator.csproj`).
- **`InternalsVisibleTo`:** one in the runtime, to `timewarp-mediator-tests` with a strong-name key (csproj line 221). The only stated reason is that unit tests exercise `ServiceRegistrar.GenerateCombinations` directly.
- **`TODO` / `HACK` / `FIXME` / `#if` / `[Obsolete]`:** none found in `source/`.
- **Type forwards:** 38 forwards in `type-forwardings.cs` keep binary compatibility for 13.x consumers who compiled against types that moved into Contracts.
- **Docs position on the runtime:**
  - `mediator.cs:6-8`: "Legacy reflection dispatcher ... Not trim or NativeAOT safe."
  - `samples/readme.md`: "kept, not extended."
  - `generated-vs-legacy.md`: `AddMediator` does not delegate to the generated type. The "shim" is deferred (`m1-generated-mediator.md` deferred table).
- **Test weight:** `tests/timewarp-mediator-tests` is 6232 lines. The registrar and DI-configuration tests are the largest part, including `service-collection-pipeline-tests.cs` at 1060 lines. The wrapper-cache, type-forwarding and trimming-annotation tests (115, 23, 36 lines) guard the reflection mechanics.

## Summary for the redesign discussion

- **Essential:** about 300 lines of dispatch.
- **DI plumbing:** about 1080 lines, of which the options class and registrar are about 900. Only `RegisterServicesFromAssemblies` shows up in a sample. Every other knob appears only in tests.
- **MediatR-parity features:** about 560 lines (pre/post processors, exception cascade, orderer) plus about 150 for streams. None of these are supported by the generated stack.
- **Hot path:** per-call LINQ pipeline construction plus DI lookups cost 224 B / 69 ns against 96 B / 30 ns generated. Both are far from the 24 B / 8.8 ns floor.
