**TimeWarp.State depends on the generated mediator surface only, and it uses very little of it.** I made no changes. The checkout is `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-state/master` at 12.0.0-beta.13, and all paths below are relative to it.

## 1. Packages and versions
- `Directory.Packages.props:14-16` pins Contracts, Generators and Analyzers at `14.0.0-beta.5`. Kanban task 119 did that bump on 2026-10-13, and State has not yet moved to 14.0.0 stable.
- There is no MediatR and no `TimeWarp.Mediator` runtime package reference anywhere in State.
- `source/timewarp-state/timewarp-state.csproj:36-46` references Contracts plus Analyzers (private) only. A comment there forbids Generators, because the generator emits a public `AddGeneratedMediator` that would collide with the host's.
- Blazor, Plus and Telemetry reference Contracts and Analyzers the same way.
- `source/timewarp-state-devtools/timewarp-state-devtools.csproj:13` references Generators, and so does every sample host and test host. Each host owns the generator, and the libraries join its compile-time graph through assembly attributes.
- `documentation/overview.md:44` says State uses "TimeWarp.Mediator 14.0.0-beta.1 ... not MediatR, and not reflection-based `AddMediator()`". The version text there is stale.

## 2. Mediator types in use

| Type | Where and how |
|---|---|
| `IAction : IRequest` (Mediator's own) | State has no `IAction` of its own. It is used as a constraint on every behavior, e.g. `state-transaction-behavior.cs:38`, and as the marker on every nested `Action`. |
| `IRequest` (no response) | One direct use: `LoadPersistentStateRequest`, `source/timewarp-state-plus/features/persistence/load-persistent-state-request.cs:26`. |
| `IRequestHandler<T>` | Used directly by `LoadPersistentStateRequestHandler` (same file, line 33). |
| `ActionHandler<TAction>` | The base of `StateActionHandler<TAction>` at `source/timewarp-state/base/state-action-handler.cs:26`. It adds the `IStore` property. All nested `Handler` classes derive from it. |
| `ISender<ClientPipeline>` | Injected into `State` (`state.cs:26,44`), `IState` (`i-state.cs:19`), `Store` (`store.cs:163`), the Blazor components and several behaviors. |
| `IPublisher<ClientPipeline>` | `Store` (`store.cs:31`), `StateTransactionBehavior` (line 41) and `TimerState` (line 26). |
| `INotification` | `StateInitializedNotification`, `ExceptionNotification` and `TimerElapsedNotification`. |
| `INotificationHandler<T>` | `StateInitializedNotificationHandler` (Plus) and `InitializedStateSnapshotHandler` (Telemetry). |
| `IPipelineBehavior<,>` and `RequestHandlerDelegate<T>` | Every behavior listed in section 3. |
| `MediatorAssembly`, `MediatorScope` | Declared in each library's `assembly-marker.cs`: State, Blazor, Plus, Telemetry and Devtools. |
| `MediatorBehavior` | Declared in State, Blazor and Telemetry. Plus declares none, because its behaviors are opt-in for the host. |
| `AddGeneratedMediator<ClientPipeline>()` | Called by hosts, e.g. `source/timewarp-state-devtools/program.cs:35`, and by all samples. |
| `ServerPipeline` and `ISender<ServerPipeline>` | Defined in State. Only the test server (`tests/test-app/test-app-server/program.cs:36,70`) calls them. |
| `NoHandlerException` | Mentioned only in a comment. |
| `Sender.Send(object)` | The non-generic overload, called once at `source/timewarp-state-blazor/features/javascript-interop/json-request-handler.cs:113` (JavaScript dispatch). |

State itself does not use `AddMediator`, `RegisterServicesFromAssembly`, `MediatorServiceConfiguration`, `AddOpenBehavior`, notification publisher strategies, `IRequestPreProcessor`, `IRequestPostProcessor`, exception handlers or exception actions, or `IStreamRequest`. The only stream references are `CreateStream` stubs in hand-written `ISender` fakes in tests.

`ClientPipeline` is a plain marker class (`source/timewarp-state/pipelines/client-pipeline.cs`).

## 3. Action model and behaviors
**Action model.** Each `FooState.BarActionSet` nests an `Action : IAction` and a `Handler : StateActionHandler<Action>`. The handler is typically `public sealed` and nested, with the `IStore` injected through a primary constructor. The Mediator generator links each handler at compile time. State does not resolve handlers by reflection. `source/timewarp-state-policies/policies.action-handler-policy.cs:39-69` enforces the base type. It also requires handlers shipped in libraries to be `public sealed`, because the host's generated code names them by concrete type.

The generated `State.Foo(...)` methods come from `source/timewarp-state-source-generator/action-set-method-generator.cs:124`. They emit `await Sender.Send(new X.Action(...), token)`.

**Behaviors.** All are open generics woven through `[assembly: MediatorBehavior]`, ordered lower to outermost. Each one is registered only in the ClientPipeline scope.

| Order | Behavior | Path |
|---|---|---|
| 200 | `StateInitializationPreProcessor<,>`, awaits state init | `source/timewarp-state/features/state-initialization/state-initialization-pre-processor.cs` |
| 300 | `StateTransactionBehavior<,>`, clones state, rolls back on failure, publishes `ExceptionNotification` | `source/timewarp-state/features/pipeline/state-transaction-behavior.cs` |
| 350 | `TelemetryBehavior<,>` | `source/timewarp-state-telemetry/telemetry-behavior.cs` (marker line 16) |
| 400 | `RenderSubscriptionsPostProcessor<,>` | `source/timewarp-state-blazor/features/render-subscriptions/render-subscriptions-post-processor.cs` |
| host-chosen | `ActiveActionBehavior<,>`, `MultiTimerPostProcessor<,>`, `PersistentStatePostProcessor<,>` (all Plus, opt-in) | `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`, `.../timers/multi-timer-post-processor.cs`, `.../persistence/pipeline/persistent-state-post-processor.cs` |

- The first four are woven by State's own attributes.
- Every behavior constrains `TRequest : notnull, IAction`, except `MultiTimerPostProcessor`, which is `notnull` only.
- `ActiveActionBehavior` and `MultiTimerPostProcessor` call `Sender.Send` from inside the pipeline and guard recursion with `IInternalAction` (`source/timewarp-state/state/i-internal-action.cs:19`).
- The behaviors read `typeof(TRequest).GetEnclosingStateType()`, which only works because every action is nested in its state.
- Behaviors are also tested with hand-written `ISender` fakes, for example `tests/timewarp-state-tests/store/store-get-or-add-tests.cs:177`.

## 4. Unit, IMediator and notifications
- **Unit:** State never names it. Actions are `IAction`, which is an `IRequest` with no response, and they come back as `Unit` from `ActionHandler`. The generated manifest in the test app shows `"response":"TimeWarp.Mediator.Unit"`.
- **Response types:** The behaviors are generic over `TResponse`, so a future change to the response type would touch them.
- **IMediator:** No runtime use. Only the analyzer matches it by metadata name (`handler-must-not-send-action-analyzer.cs:42`).
- **Publish:** Used for `StateInitializedNotification` (`source/timewarp-state/store/store.cs:178`), for `ExceptionNotification` (`state-transaction-behavior.cs:163`) and for `TimerElapsedNotification` (`timer-state.cs:126`).
- **Notification handlers:** Two exist in the libraries: Plus's persistence loader and Telemetry's snapshot handler. The test app also has several.
- **Notification ordering and publisher strategy:** State does not depend on either. `Store` keeps the `Publish` task and awaits it in the init pre-processor, so it relies on publish completing only after all handlers finish.

## 5. Reflection-only features
**None.** State uses no assembly scanning, no open-generic handlers, no ordering through DI order and no exception processors. It relies on exceptions propagating through behaviors, and `StateTransactionBehavior` does its own try/catch. It also relies on compile-time membership (`MediatorScope`, per-scope behaviors). Hosts must reference the generator, so the generated profile is a hard requirement.

The one State-side leftover is `LogTimeWarpStateMiddleware`, which inspects `IServiceCollection` for `IPipelineBehavior<,>` registrations (`service-collection-extensions.log-timewarp-state-middleware.cs:21`). It depends on the generator registering closed behaviors in pipeline order. It uses `PipelineBehaviorRegistry` (`source/timewarp-state/features/pipeline/pipeline-behavior-registry.cs`), which a State generator populates.

## 6. State's own generators (`source/timewarp-state-source-generator`)
- `pipeline-behavior-registry-source-generator.cs` matches `TimeWarp.Mediator.IPipelineBehavior`2` by metadata name (line 18). It emits a module initializer that calls `PipelineBehaviorRegistry.Register(typeof(Behavior<,>), typeof(IPipelineBehavior<,>))`.
- `action-set-method-generator.cs` emits the `Sender.Send` convenience methods on each state.
- `persistence-state-source-generator.cs:100` emits a `Sender.Send` call.
- `source/timewarp-state-analyzer/*` has metadata-name constants for `ActionHandler`1`, `IActionHandler`1`, `IAction`, `ISender`, `ISender`1` and `IMediator`.

**Caveat on generated output.** The Mediator generator cannot see another generator's output. Task 80 and `load-persistent-state-request.cs:5-8` record that a generated handler was never linked and threw `NoHandlerException`. State therefore hand-writes the load request and handler. Anything State's generators emit must not need to be a Mediator handler, request or behavior.

## 7. Docs and kanban
- `kanban/done/080-001-*` through `080-003-*` cover the move to the generated mediator, `AddGeneratedMediator` and named `ClientPipeline`/`ServerPipeline` senders.
- `kanban/done/119-*` records the dogfood pass: zero TWM001–TWM011 diagnostics in build, samples and the trimmed sample 05 publish.
- `documentation/overview.md:44-58` and `documentation/topics/analyzers.md:28-37` describe the model. The analyzer docs list `ISender.Send` and `IMediator.Send` of `IAction` as what handlers must not do.
- The open kanban items (`kanban/to-do/118-*`, `kanban/backlog/110-*`) mention nothing about the mediator, AOT or streams.
- Nobody in State has asked for a different mediator API.

## Conclusions for 15.0
**State needs:**
- `IRequest`/`IAction` marker semantics, with a no-response request returning `Unit` or its equivalent.
- `IRequestHandler<T>`, and an `ActionHandler<T>` base.
- `IPipelineBehavior<TReq,TRes>` with `Handle(request, next, ct)`. State needs open-generic, per-scope, ordered behaviors woven at compile time.
- `ISender<TScope>` with typed `Send`, plus the untyped `Send(object)` for JS dispatch.
- `IPublisher<TScope>.Publish` with `INotification` and `INotificationHandler<T>`.
- `AddGeneratedMediator<TScope>()`, with the attributes `MediatorAssembly`, `MediatorScope` and `MediatorBehavior(order, Scope)`.
- Behavior registrations in DI that stay observable for `LogTimeWarpStateMiddleware`, or another way to list the pipeline order.
- Handlers reachable by concrete type from a host's generated code, and the TWM diagnostics through the Analyzers package.

**State does not need:**
- Streams, the reflection runtime, `AddMediator`, pre/post processor interfaces, exception handlers or actions, publisher strategies, or the composite `IMediator` at runtime.
- A response-bearing `IRequest<T>` in any library code. Only the test app uses `IRequest<T>`, for server queries through `ISender<ServerPipeline>`.
