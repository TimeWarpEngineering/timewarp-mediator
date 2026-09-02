# Source Generator + AOT Rewrite Analysis for TimeWarp.Mediator

> **⚠ Superseded** by [`2026-06-17-source-gen-aot-rewrite-spec.md`](./2026-06-17-source-gen-aot-rewrite-spec.md) (consolidated single source of truth). Kept for history — the consumer-requirement material (TimeWarp.State + TimeWarp.Architecture) is carried forward into §2 of the spec.

**Date:** 2026-06-17  
**Author:** Grok (based on exploration and conversation)  
**Context:** Fork of MediatR with goal of efficient source-generated, AOT-compatible implementation. Informed by TimeWarp.Nuru ambition level and real usage in TimeWarp.State.

## Executive Summary

The current implementation follows the classic MediatR model: runtime assembly scanning, reflection-based handler and behavior resolution via `IServiceProvider`, wrapper types created with `Activator.CreateInstance`, `ConcurrentDictionary` caching of wrappers, and runtime `Reverse().Aggregate` construction of pipeline delegates on every (or first) `Send`/`Publish`.

This approach works but carries:
- Reflection and generic activation costs
- Poor Native AOT / trimming characteristics
- Runtime errors instead of compile-time diagnostics for missing handlers, duplicate handlers, etc.
- Limited ability to inline or specialize dispatch for known closed sets of messages

A rewrite should target a **source-generator-first, closed-world-where-possible** design, matching the aggressiveness demonstrated in TimeWarp.Nuru (interceptors, lambda body capture + re-emission, static `Lazy<T>` service fields, inlined behavior pipelines, command-with-nested-Handler pattern, mixed source-gen vs runtime DI).

TimeWarp.State is the primary consumer and driver. It will use the new mediator to replace its current dependency. Key patterns from TimeWarp.State (and apps like Copic that use the same style) must be first-class.

## Current TimeWarp.Mediator Characteristics

Core pieces observed:
- `IRequest`, `IRequest<TResponse>`, `INotification`, `IStreamRequest<T>`
- `IRequestHandler<,>`, `IRequestHandler<>`, `INotificationHandler<>`, `IStreamRequestHandler<,>`
- `IPipelineBehavior<,>`, `IStreamPipelineBehavior<,>`
- Pre/post processors (`IRequestPreProcessor<>`, `IRequestPostProcessor<,>`)
- Exception handlers/actions (`IRequestExceptionHandler<,,>`, `IRequestExceptionAction<,>`)
- `Mediator` class with `ConcurrentDictionary<Type, Wrapper>` + `Activator` for `RequestHandlerWrapperImpl<,>`, `NotificationHandlerWrapperImpl<>`, etc.
- Runtime pipeline construction inside wrappers: `GetServices<IPipelineBehavior<...>>().Reverse().Aggregate(...)`
- `ServiceRegistrar` with extensive reflection (`FindInterfacesThatClose`, generic type explosion guards, `GenerateCombinations`, assembly scanning)
- Multiple notification publishers (`ForeachAwaitPublisher`, `TaskWhenAllPublisher`)
- `MediatorServiceConfiguration` + `AddMediator` extensions with rich behavior/pre/post registration APIs
- Support for many DI containers via samples (Autofac, Windsor, Lamar, etc.)
- `Send(object)`, `Publish(object)`, and streaming paths rely on reflection for interface detection

Strengths: Extremely flexible at runtime, works with open generics, constrained generics, covariant notifications, and existing DI ecosystems.

Weaknesses for modern .NET: reflection, cold-start cost, AOT incompatibility, missed opportunities for compile-time validation and specialization.

## Reference Points

### TimeWarp.Nuru (Ambition Level)
- Extreme use of incremental source generators.
- `InterceptsLocationAttribute` to literally replace calls to `app.RunAsync(args)` with generated code.
- Captures lambda bodies as source text and re-emits them as local functions.
- Emits static `Lazy<T>` fields for services (topologically sorted).
- Inlines pipeline behaviors as nested lambdas.
- Command/Query pattern with nested `Handler` class inside the message/command type.
- Supports both full source-gen DI and hybrid runtime DI.
- Generates help, completion, telemetry, etc.
- Strong model validation + diagnostics at build time.
- Per-app isolation of generated interceptors/routes.

This sets the bar: we are willing to rewrite call sites, emit large amounts of specialized code, and treat the set of handlers + behaviors as a compilation-time artifact.

### martinothamar/Mediator
- Source generator produces DI registration code + a concrete `Mediator` implementation.
- Monomorphized `Send<TRequest, TResponse>` methods (no boxing, direct dispatch).
- `ValueTask` based.
- Abstractions in a separate small package; generator referenced only in the executable project.
- Build-time diagnostics (e.g., missing handler).
- Excellent AOT and cold-start characteristics.
- Still resolves handlers via DI in many cases but eliminates the generic wrapper + reflection tax of classic MediatR.

Useful baseline, but we can go further by embracing stronger conventions (ActionSet style) and deeper inlining.

## TimeWarp.State Usage Patterns (Primary Driver)

TimeWarp.State (and TimeWarp.State.Plus) has already migrated to TimeWarp.Mediator and layers significant behavior on top. Critical observations:

### ActionSet Pattern
```csharp
public partial class CounterState
{
    internal static class IncrementActionSet
    {
        [TrackAction]
        internal sealed class Action(int Amount = 1) : IAction;

        internal sealed class Handler : ActionHandler<Action>
        {
            public Handler(IStore store, ...) : base(store) { }

            public override ValueTask<Unit> Handle(Action action, CancellationToken ct) { ... }
        }
    }
}
```

- `IAction : IRequest` (most actions are "void" via `Unit`).
- `ActionHandler<TAction> : IRequestHandler<TAction>` with `ValueTask<Unit> Handle(...)`.
- ActionSets are `static` classes, conventionally `internal`, nested inside `partial class *State`.
- Handlers are nested `sealed class Handler` inside the ActionSet.
- Attributes can be placed on the `Action`.
- This pattern is already very close to Nuru endpoints.

### Source Generation on Top
TimeWarp.State ships its own incremental source generators:
- `ActionSetMethodSourceGenerator`: For every `XXXActionSet` nested in a State, generates an ergonomic method on the parent State class:
  ```csharp
  public async Task Increment(int amount = 1, CancellationToken? externalCancellationToken = null) { ... await Sender.Send(new XXXActionSet.Action(...)); }
  ```
- Persistence generator emits `Load()` methods that dispatch a single shared `LoadPersistentStateRequest`.
- Generators parse constructors on `Action` types for parameters.

Consumers rarely write raw `Sender.Send(new SomeActionSet.Action(...))`; they use the generated sugar.

### Pipeline & Cross-Cutting (Heavy Use)
Behaviors and processors are registered in explicit order (order matters):

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StateInitializationPreProcessor<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StateTransactionBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RenderSubscriptionsPostProcessor<,>));
// Plus adds more (ActionTracking, PersistentStatePostProcessor, MultiTimerPostProcessor, ReduxDevToolsBehavior, etc.)
```

Key behaviors:
- `StateTransactionBehavior<TRequest, TResponse>`: Clones state (via `ICloneable` or custom `Clone`), sets it on Store, calls `next`, restores original on exception + publishes `ExceptionNotification`.
- Action tracking behavior sends additional `StartProcessingActionSet.Action` / `CompleteProcessingActionSet.Action` around the real action (re-entrancy).
- Pre-processors wait for state initialization tasks.
- Post-processors trigger re-renders, persist state, manage timers.

Many processors inherit from `MessagePreProcessor<T, R>` / `MessagePostProcessor<T, R>` (which implement `IPipelineBehavior` by delegating to `Handle` before/after `next`).

### Notifications
- `StateInitializedNotification`, `ExceptionNotification`, `TimerElapsedNotification`, etc.
- Handlers for them (some send further actions, e.g. loading persistent state on init).

### Sender Injection & Re-entrancy
- `IStore` and individual State instances receive `ISender`.
- Handlers and behaviors commonly inject `ISender` and call `Send` for other actions while a dispatch is active.
- `JsonRequestHandler` uses the `Send(object)` path for JS interop.

### Other Characteristics
- Mostly scoped lifetimes.
- Constructor DI is the norm (Store, loggers, services, validators, etc.).
- States are created via `ServiceProvider.GetRequiredService<TState>()`.
- Strong analyzers + architectural policies enforce nesting rules ("Action must be in *ActionSet", "ActionSet must be nested in State", etc.).
- Some internal actions are not intended to be sent directly from user code.

TimeWarp.State comments indicate awareness of martinothamar's model (pre/post processors become behaviors, registration order is the pipeline order, generator registration vs runtime).

## Design Principles (Given Constraints)

1. **Choose the elegant long-term path**, not the migration-friendly path.
2. Embrace the **ActionSet + nested Handler** convention as a first-class, primary model (it already exists and is loved).
3. Go **source-generator-first**. Treat the set of actions, handlers, and (ideally) behaviors as a closed world at compile time.
4. Align on `ValueTask` for the core paths.
5. Support static/stateless handler paths for maximum speed where applicable.
6. Preserve the expressiveness of ordered pipelines, pre/post processors, notifications, and re-entrant sends.
7. Enable (and amplify) the existing source generators in TimeWarp.State rather than fight them.
8. Produce excellent compile-time diagnostics.
9. Be AOT and trimming friendly by construction.
10. Dynamic `Send(object)` must still work (for JS bridge and similar).

## Proposed Architecture

### Packages
- `TimeWarp.Mediator.Contracts` — minimal interfaces (`IRequest*`, `INotification`, `IPipelineBehavior`, `ISender`, `IPublisher`, `Unit`, delegates). Keep surface familiar enough for TimeWarp.State to adapt easily.
- `TimeWarp.Mediator` — runtime pieces + the source generator (as analyzer dependency with `PrivateAssets`).
- Generator is referenced primarily in the executable / top-level project (like Nuru and martinothamar).

### Handler Model
Primary declaration:

```csharp
public partial class MyFeatureState
{
    public static class DoSomethingActionSet   // or internal static
    {
        public sealed class Action(...) : IAction;   // or IAction<TResponse>

        public sealed class Handler : IActionHandler<Action>  // or ActionHandler<Action>
        {
            // constructor injection of IStore, services, ISender, etc.
            public ValueTask<Unit> Handle(Action action, CancellationToken ct);
        }
    }
}
```

- Generator correlates by nesting + naming convention (can be augmented with attributes).
- Also support top-level handlers and attribute-based discovery for generality.
- Static handler variant for pure cases:
  ```csharp
  public static ValueTask<Unit> Handle(Action action, IStore store, CancellationToken ct) { ... }
  ```

TimeWarp.State's own generators continue (or are enhanced) to emit the ergonomic methods on State classes that call the (now very fast) sender.

### Dispatch Model
Two complementary paths:

1. **Generated monomorphized paths** (primary hot path)
   - For every discovered `(ActionType, Handler)` the generator emits a specialized method.
   - Pipeline application (known behaviors at generation time) can be inlined as nested calls or a local function chain for that specific action.
   - Direct construction: `new MyActionSet.Handler(deps...).Handle(...)` or static call.
   - Services can be captured in generated static `Lazy<T>` fields (topo-sorted, like Nuru) when using source-gen DI mode.

2. **Generated fallback table** for `Send(object)` and any truly open/dynamic cases.
   - Still generated code, no runtime reflection for dispatch.
   - Used by `JsonRequestHandler`, late-bound scenarios, etc.

`ISender` / `IMediator` can be implemented (or partially intercepted) so that call sites written as `await Sender.Send(new FooActionSet.Action(...))` become direct calls to the generated methods via `InterceptsLocation` where feasible, or through a very thin generated facade.

### Pipeline
- Keep `IPipelineBehavior<,>` as the fundamental extension mechanism.
- Continue allowing ordered registration via DI for flexibility and for behaviors contributed by libraries (TimeWarp.State, .Plus, user code).
- Generator collects registrations visible in the compilation and can emit per-action specialized chains that apply exactly the registered behaviors (in order) around the handler.
- For even tighter coupling, support attributes on Action/ActionSet that declare required behaviors (`[StateTransaction]`, `[TrackAction]`, etc.). This enables full unrolling.
- Pre- and post-processors remain as `MessagePreProcessor<T,R>` / `MessagePostProcessor<T,R>` base classes that implement the behavior interface (consistent with current direction toward martinothamar-style).
- `StateTransactionBehavior` fits naturally: it still needs runtime access to `IStore` for clone/restore, but the "invoke the rest of the pipeline + handler" part can be a generated delegate.

Re-entrancy is supported because the generated `Send`/`Dispatch` entry points can be called recursively; any per-state semaphores or locking remain the responsibility of the state layer.

### Notifications
- Same discovery for `INotification` + handlers.
- Generator can emit direct fan-out or a generated publisher per notification type (no runtime `GetServices` + executor record creation for known notifications).
- Multiple publishers or strategies per notification type remain possible via configuration visible to the generator.

### Service Resolution / DI Strategy
Support a spectrum (like Nuru):
- Full source-gen mode: static `Lazy<T>` fields + direct new in generated code. Topological ordering of dependencies.
- Hybrid: source-gen for known handlers + runtime DI for complex user services.
- Classic DI mode (for interop or dynamic scenarios) — de-emphasized but possible.

`IOptions<T>`, loggers, `ISender` itself, `IStore`, etc. are handled during generation (Nuru already has sophisticated service extraction including `IOptions` + validators).

### Registration Story for TimeWarp.State Consumers
```csharp
builder.Services.AddTimeWarpState(cfg => { ... });

// Inside AddTimeWarpState (or a new AddMediator equivalent):
// - Registers IStore, Subscriptions, Render contexts, etc.
// - Registers the ordered IPipelineBehavior entries (StateTransaction, pre/post processors, Plus behaviors, etc.)
// - The source generator (pulled in by referencing the package) does the rest for dispatch.
```

No more massive assembly scanning at startup for handlers.

## Specific Technical Opportunities

- Use `InterceptsLocation` on generated `Sender.Send(...)` call sites produced by TimeWarp.State's own generators (or on user code) for zero-overhead dispatch.
- Emit per-State or per-ActionSet invoker types when useful.
- Generate a "capabilities" or metadata artifact consumable by Redux DevTools, analyzers, or MCP tools.
- Strong diagnostics + code fixes for the entire ActionSet contract.
- Support for "internal only" actions (generator can emit `internal` dispatch paths).
- Subset generation or trimming hints for specific feature slices.
- Full unrolling of simple pipelines when all behaviors are known and pure.

## Migration Considerations (for TimeWarp.State)

Since backward compat is deprioritized:
- TimeWarp.State can update its base classes (`ActionHandler`, `MessagePreProcessor`, etc.) to the new delegate/return signatures (mostly `ValueTask` alignment).
- Its source generators will emit calls that the new mediator makes extremely cheap.
- Architectural policies and analyzers remain valuable and can be strengthened.
- Samples and tests will need updates to the new registration + possible removal of classic `AddMediator` scanning calls.
- The `Send(object)` path must be preserved for `JsonRequestHandler`.

## Risks & Open Questions

- How to best surface pipeline ordering to the generator (DI registration scanning vs. a declarative builder that can be intercepted vs. attributes on ActionSets)?
- How much behavior inlining is practical when behaviors are contributed by multiple assemblies (TimeWarp.State + .Plus + app)?
- Strategy for behaviors that are fundamentally runtime (they inspect attributes on the request at runtime, e.g. `[TrackAction]` check inside ActiveActionBehavior).
- Exact naming (keep `IRequestHandler` or introduce `IActionHandler`? Delegate names?).
- Whether to keep a "classic" reflection implementation as a separate package/flag for extreme interop cases.
- Interaction with existing strong-named assemblies or custom DI containers (lower priority).

## Recommended Next Steps

1. Define the minimal new Contracts surface (ValueTask-centric).
2. Prototype generator discovery for ActionSet + nested Handler (and notifications).
3. Emit a simple monomorphized `Send` path for one concrete action with no behaviors.
4. Add one known behavior (StateTransaction) and show inlined or chained generated code.
5. Update TimeWarp.State's base classes and one sample generator emission in a throwaway branch to validate fit.
6. Add diagnostics for the common mistakes (missing handler, etc.).
7. Measure cold-start + steady-state vs. current implementation (and vs. martinothamar).

## Conclusion

The opportunity is to build a mediator whose primary implementation is generated, specialized code rather than generic runtime machinery. By making the ActionSet pattern that TimeWarp.State already uses first-class, and applying Nuru-level generation ambition, we can deliver dramatically better performance, AOT compatibility, and compile-time safety while actually improving the developer experience for the state-management use case.

This is not an incremental speedup of MediatR. It is a purpose-built dispatcher for the patterns the TimeWarp ecosystem has converged on.

---

## Additional Requirements from TimeWarp.Architecture

The new library will also replace Mediator usage inside the TimeWarp.Architecture templates and libraries (`/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-architecture/dev`).

### Observed Usage Patterns

**1. Rich CQRS Abstractions (beyond plain IRequest)**
- Heavy use of `ICommand<TResponse>` and `ICommandHandler<TCommand, TResponse>`.
- Example from dev tools (AOT-published CLI):
  ```csharp
  [NuruRoute("build", Description = "...")]
  internal sealed class BuildCommand : ICommand<Unit>
  {
      internal sealed class Handler : ICommandHandler<BuildCommand, Unit>
      {
          public async ValueTask<Unit> Handle(BuildCommand command, CancellationToken ct) { ... }
      }
  }
  ```
- Commands are frequently defined as nested types inside static/partial container classes (very similar to TimeWarp.State ActionSets and Nuru endpoints).
- `IRequest<OneOf<TSuccess, ValidationResult, Exception, SharedProblemDetails>>` is common for API/domain requests that need rich error union returns.

**2. Domain Events**
- `public abstract class BaseEvent : INotification { }`
- Notifications are used for domain events.

**3. Pipeline Behaviors**
- `FluentValidationBehavior<TRequest, TResponse> : IPipelineBehavior<...>` — performs validation and short-circuits by constructing `OneOf<Success, Problem>` error responses.
- Other behaviors registered (e.g. `GenericPipelineBehavior`).
- Registration pattern:
  ```csharp
  .AddMediator(cfg => cfg.RegisterServicesFromAssemblies(serverAsm, applicationAsm));
  services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));
  ```

**4. Sender Usage in Endpoints**
- `BaseEndpoint<TRequest, TResponse>` (and FastEndpoints variants) resolve `ISender` and do:
  ```csharp
  OneOf<TResponse, SharedProblemDetails> response = await Sender.Send(request);
  return response.Match(Ok, problem => StatusCode(...));
  ```

**5. AOT & Tooling Requirements**
- The `dev-cli` (tools/dev-cli) is published with `<PublishAot>true</PublishAot>`.
- It combines Nuru `[NuruRoute]` + Mediator `ICommand`/`ICommandHandler` for CLI commands.
- The new Mediator must be fully AOT compatible when used in such mixed environments.

**6. Analyzers & Source Generators**
- Architecture analyzers enforce "Query/Command class must implement IRequest<>".
- Fast-endpoint source generator references `IRequest<OneOf<...>>`.
- Coexistence with multiple generators and analyzers is required.

**7. Registration & Migration Status**
- Still uses scanning-based `RegisterServicesFromAssemblies`.
- Some legacy non-nested handlers (`public class GetWeatherForecastsHandler : IRequestHandler<...>`).
- In-progress migration away from Mediator for pure HTTP (FastEndpoints), but Mediator remains important for:
  - Internal application services
  - Automation contracts
  - Dev tooling
  - Domain events

### Implications for New Design

- The cleanest long-term contracts should include the full martinothamar-style set:
  - `IRequest` / `IRequest<T>`
  - `ICommand` / `ICommand<T>` / `IQuery` / `IQuery<T>` (and stream variants)
  - Corresponding `*Handler` interfaces
- Source generator **must** first-class support the **nested `Handler` pattern** inside command/query classes (whether named `ActionSet`, `Command`, or other).
- Support for `OneOf<...>` return types is natural (no special casing), but the current reflection-based error wrapping in `FluentValidationBehavior` is fragile — a generated or cleaner base behavior would be better.
- INotification support remains essential.
- The generator should work in AOT tool projects that also use Nuru.
- When source-gen mode is active, `AddMediator` + assembly registration can become a no-op or thin shim for the discovered handlers.
- Behavior registration order (via DI) should be visible to the generator for optimal inlining where possible.

This usage validates the direction of embracing the nested-handler convention and Nuru-level generation ambition. The architecture templates will benefit enormously from compile-time dispatch, AOT cleanliness, and build-time validation of command/handler pairs.

---

*Document created from direct codebase exploration (avoiding pre-existing Analysis docs), reference to Nuru and martinothamar/Mediator patterns, and clarifications on priorities and usage (including TimeWarp.State, Copic, and TimeWarp.Architecture).*
