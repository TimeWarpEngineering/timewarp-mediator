# TimeWarp.Mediator Source-Gen / AOT Rewrite — Consolidated Spec

**Date:** 2026-06-17
**Status:** Consolidated single source of truth. **Supersedes** the three brainstorm docs (see [Provenance](#provenance)); read this one to act.
**Epic:** 004 — Source-gen rewrite with `ISender<TScope>` named pipelines
**Task (this milestone):** 004-002 — M2 `ISender<TScope>` / `IPublisher<TScope>` named pipelines
**Shipped:** 004-001 — M1 generated Mediator, analyzer, and State golden-file
**Consolidated by:** Fable, from the Composer / Opus / Grok analyses + the Composer↔Opus collaboration thread.
**Decision context:** Clean break, major version. No obligation to preserve MediatR's runtime open-generic combinatorics, `MaxTypesClosing`, or assembly scanning. Edge cases become compile-time diagnostics.

---

## Provenance

This doc merges and replaces:

| Source doc | Author | What it contributed |
|-----------|--------|--------------------|
| [`…-rewrite.md`](./2026-06-17-source-gen-aot-rewrite.md) | Composer (survey) | Option space; martinothamar steal/skip; CQRS semantics; three-layer ServiceGen north star |
| [`…-rewrite-opus.md`](./2026-06-17-source-gen-aot-rewrite-opus.md) | Opus | Linker metaphor; MessageGraph IR; profiles; the interceptor/DI/pipeline decisions |
| [`…-rewrite-grok.md`](./2026-06-17-source-gen-aot-rewrite-grok.md) | Grok | **Consumer requirements** — TimeWarp.State (ActionSets, re-entrancy) + TimeWarp.Architecture (CQRS, OneOf) |
| [collaboration thread](../.agents/collaboration/2026-06-17-source-gen-aot-rewrite/) | Composer↔Opus | The five resolved design decisions |

The prior docs remain for history; each now carries a "superseded" banner pointing here.

---

## 1. Executive summary

Replace classic-MediatR runtime machinery (reflection, `Activator`, wrapper types, `ConcurrentDictionary`, `Reverse().Aggregate` pipelines, assembly scanning) with a **source generator that treats the set of handlers + behaviors as a compile-time graph**, verifies it (missing/duplicate handler = build error), and emits a real, zero-reflection `sealed Mediator : IMediator`.

The design is **graph-first, profile-tuned**:

- **Architecture:** one IR — the **MessageGraph** — built once, verified once, read by several emitters. The generated `Mediator` + a `manifest.json` are always emitted; interceptors, static-field DI, and pruning are **opt-in profiles**.
- **Runtime shape (default):** an injectable `Mediator`/`ISender` object, handlers resolved from the ambient DI scope, behaviors applied in compile-time-fixed order. Reflection is deleted; the object is not.
- **Consumer-driven:** the **nested-`Handler` convention** (State's `ActionSet`, Architecture's `ICommand`) is the first-class primary model, not an afterthought.

This is not a faster MediatR clone — it is a purpose-built dispatcher for the patterns the TimeWarp ecosystem (State, Architecture, Nuru) has converged on, that happens to stay MediatR-shaped enough to adopt.

---

## 2. Consumer requirements (the primary drivers)

These are hard constraints from the two real consumers. The design is validated against them, not against generic MediatR assumptions.

### 2.1 TimeWarp.State (primary)

- **ActionSet + nested `Handler`.** `Action : IAction (: IRequest)` and `sealed class Handler : ActionHandler<Action>`, nested in a `static class XxxActionSet` inside a `partial class *State`. Most actions are void (`ValueTask<Unit>`). This convention is loved and analyzer-enforced — make it first-class.
- **State's own generators emit the call sites.** `ActionSetMethodSourceGenerator` emits `await Sender.Send(new XxxActionSet.Action(...))` sugar on the State class. Consumers rarely hand-write `Send`. *(Implication: many call sites are generated in a known shape — see [OQ-A](#oq-a-interception-across-generators).)*
- **Heavy, ordered, scoped pipeline.** `StateTransactionBehavior`, `StateInitializationPreProcessor`, `RenderSubscriptionsPostProcessor`, action-tracking, persistence, ReduxDevTools — **all `AddScoped`, order-significant**. *(Implication: the pipeline default must handle scoped behaviors — see [§6](#6-pipeline-model) and [OQ-B](#oq-b-scoped-behavior-composition).)*
- **Re-entrancy is mandatory.** `IStore`, States, handlers, and behaviors inject `ISender` and call `Send` for other actions **while a dispatch is active** (action-tracking wraps each action in `StartProcessing`/`CompleteProcessing` sends). *(Implication: a real, injectable `ISender` instance is **required**; a static-only `Dispatch`/"vanished" mediator is disqualified as the default. This independently confirms the Composer correction.)*
- **`Send(object)` must work.** `JsonRequestHandler` dispatches late-bound for JS interop.
- **Runtime-attribute behaviors.** Some behaviors branch on request attributes at runtime (e.g. `[TrackAction]`). *(See [OQ-C](#oq-c-exception--attribute-driven-behaviors).)*

### 2.2 TimeWarp.Architecture

- **Full CQRS surface.** `ICommand<T>` / `ICommandHandler<,>`, `IQuery<T>` / `IQueryHandler<,>`, alongside `IRequest`. Commands as nested types in a container class (same shape as ActionSets / Nuru endpoints).
- **`OneOf<TSuccess, ValidationResult, Exception, SharedProblemDetails>` returns** are common. Must be a plain response type — no special-casing — and the current reflection-based error wrapping in `FluentValidationBehavior` should become a cleaner/generated base behavior.
- **Domain events** via `abstract class BaseEvent : INotification` — polymorphic notification handlers.
- **Mixed AOT tooling.** `dev-cli` publishes `PublishAot=true` combining Nuru `[NuruRoute]` + `ICommand`/`ICommandHandler`. The generator must be clean in a project that also runs the Nuru generator. *(See [OQ-A](#oq-a-interception-across-generators).)*
- **Coexists with other analyzers/generators**; still uses scanning `RegisterServicesFromAssemblies` today (migration source).

---

## 3. Architecture — the MessageGraph is the center

```
     Handlers + Behaviors + Notifications + MediatorOptions
                          │
                          ▼   build MessageGraph (IR from SemanticModel)
        ┌─────────────────┼───────────────────────────┐
        ▼                 ▼                            ▼
   ┌─────────┐     ┌───────────────┐          ┌──────────────────────┐
   │ Verify  │     │ Emit (always) │          │ Emit (opt-in profile)│
   │ TWM001… │     │ sealed Mediator│         │ interceptors         │
   │ build   │     │ : IMediator    │         │ static-field DI      │
   │ errors  │     │ + manifest.json│         │ pruning              │
   └─────────┘     └───────────────┘          └──────────────────────┘
```

**Mental model — the mediator is a linker.** Symbolic `Send(new Ping())` resolves to a concrete handler call at build time; undefined symbol (no handler) = link error; duplicate symbol (two handlers) = link error; handlers are symbols with **linkage** (visibility via explicit assembly/module membership — [§9](#9-discovery--membership)). A linker *emits a call to the resolved target*; it does not require rewriting every caller. So the default is a generated `Mediator` whose methods are those targets. Call-site patching (interceptors) is an optional extra pass.

Default emission:

```csharp
public sealed class Mediator(IServiceProvider scope) : IMediator
{
    public ValueTask<Pong> Send(Ping request, CancellationToken ct = default)
        => __Dispatch_Ping(scope, request, ct);   // monomorphic; no MakeGenericType, no wrapper, no dictionary
}
```

Every emitter (generated `Mediator`, interceptor, analyzer-only package) reads the **same verified graph**, so analyzer diagnostics never drift from generated code.

---

## 4. Contracts surface

`TimeWarp.Mediator.Contracts` — minimal, `ValueTask`-centric, netstandard2.0-friendly:

- `IRequest`, `IRequest<TResponse>`, `IBaseRequest`
- `ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>` (+ handlers `ICommandHandler<,>`, `IQueryHandler<,>`) — semantic CQRS split (Architecture)
- `IAction : IRequest`, `IActionHandler<TAction>` / `ActionHandler<TAction>` base — first-class ActionSet support (State)
- `INotification`, `INotificationHandler<>`
- `IStreamRequest<T>`, `IStreamRequestHandler<,>`
- `IPipelineBehavior<,>`, `IStreamPipelineBehavior<,>`; pre/post processor base classes as behaviors
- `ISender`, `IPublisher`, `IMediator`, `Unit`, delegate types

`OneOf<…>` needs no special support — it is just a `TResponse`.

**Open naming decision:** keep `IRequestHandler` as the base and layer `IActionHandler`/`ICommandHandler` on top (recommended), vs. rename. Deferred, low-risk. Both State and Architecture want the nested-`Handler` convention regardless of the interface name.

---

## 5. Dispatch model

Two paths, both generated, no runtime reflection:

1. **Monomorphized static path (hot path).** Per `(RequestType, Handler)` the generator emits `__Dispatch_<Request>(scope, request, ct)` that resolves the handler, applies the behavior chain in fixed order, and calls `Handle`. The generated `Mediator.Send(TRequest)` overloads call these directly.
2. **Generated `switch` fallback for `Send(object)` / `Publish(object)`.** A type switch (or `FrozenDictionary` on net8+) over all linked types → the same `__Dispatch_*` bodies. Serves `JsonRequestHandler`, late-bound bus/deserializer paths. Unknown type → `NoHandlerException` (the only surviving runtime "not found").

`ISender`/`IMediator` is a **real injected object** (re-entrancy requires it). The Link profile (below) *additionally* intercepts statically-typed call sites; it never replaces the object.

---

## 6. Pipeline model

Behavior **order is fixed at compile time**; behavior **instances follow their DI lifetime**. No `GetServices<IPipelineBehavior<,>>().Reverse().Aggregate(...)`, ever.

Because State's behaviors are **all scoped**, the default is **not** an init-time static delegate (that can only hold singletons — the flaw in the first Opus draft). Instead, per request the generator emits a chain that resolves each *known* behavior in *known* order:

```csharp
static ValueTask<Unit> __Dispatch_Increment(IServiceProvider scope, Action a, CancellationToken ct)
{
    // order baked at compile time; instances honor lifetime
    var b0 = scope.GetRequiredService<StateInitializationPreProcessor<Action, Unit>>();   // scoped
    var b1 = scope.GetRequiredService<StateTransactionBehavior<Action, Unit>>();          // scoped
    var handler = scope.GetRequiredService<IncrementActionSet.Handler>();                 // scoped
    return b0.Handle(a, () => b1.Handle(a, () => handler.Handle(a, ct), ct), ct);
}
```

| Behavior lifetime | Emission |
|-------------------|----------|
| Singleton | cached static field (resolved once) |
| Scoped / transient | resolved from the ambient scope at send, in fixed order |

**Emission strategies (the graph is the same; only the codegen differs):**

- **Fixed-order scope-resolved chain** — *default.* Correct for scoped behaviors; zero enumeration/aggregate; order known at build. Uniform, decoratable.
- **Full inline weave** — opt-in via `[Inline]` behavior attribute, or `MessageCount < threshold`, or perf/Link profile. Behaviors inlined as nested local functions; a request with no applicable behaviors compiles to a bare handler call. Faster per-send (inlinable, zero-alloc on sync `ValueTask`) at the cost of method-body IL. **Only valid for behaviors whose lifetime/state permits inlining** (pure or singleton).

> The IL-size driver is closed behavior-type instantiation (`Behavior<Action,Unit>` per message = N×M) — **identical** in every strategy. So "inline doesn't scale" is a per-call-latency vs. uniformity tradeoff, not a type-count one. Default to the fixed-order chain; reach for inline weave only for pure/singleton behaviors where latency is the product.

Pre/post processors remain `MessagePreProcessor<,>` / `MessagePostProcessor<,>` base classes implementing the behavior interface. `StateTransactionBehavior` keeps runtime `IStore` access for clone/restore; only the "invoke the rest of the chain" part is generated.

---

## 7. DI model

The AOT-hostile thing is *open-generic resolution + `MakeGenericType` + `Activator`*, **not** `GetRequiredService<ConcreteType>()`. The generator knows every closed handler/behavior type, so even container-backed resolution is trim-safe and reflection-free.

| Host | Default DI | Rationale |
|------|-----------|-----------|
| ASP.NET / generic host / **State** | **MS.DI, scope-resolved** — handler + scoped deps from the ambient scope; singleton behaviors cached | Scoped `DbContext`/`HttpContext`/`IStore` demand it; static fields would be a captive-dependency bug |
| **AOT CLI / Nuru / dev-cli** | **ServiceGen static fields** — topo-sorted `Lazy<T>`, singletons cached, transients inlined `new` | No container; single-exe AOT |

- **Linker safety diagnostic (TWM-DI):** emitting a *scoped* service into a *static* field without an explicit scope boundary is a build error. Static DI is a profile, not universal.
- **Runtime-DI escape hatch:** emit the captured `ConfigureServices` lambda verbatim (Nuru's `UseMicrosoftDependencyInjection()` pattern) for consumers needing `AddDbContext`-style extension methods.
- **Registration shim:** when the generator is active, `AddMediator(...)` / `RegisterServicesFromAssemblies(...)` becomes a thin no-op that registers only the discovered handlers/behaviors — no assembly scanning. This is the migration bridge for Architecture.
- Long-term this is the `TimeWarp.ServiceGen` layer; Nuru's `DependencyGraphBuilder.TopologicalSort` is the implementation to lift.

---

## 8. Notifications, streams, exceptions (previously hand-waved — now explicit)

**Notifications.** Per notification type, emit a direct fan-out (no `GetServices` + `NotificationHandlerExecutor` allocation). Strategy (ForeachAwait / TaskWhenAll / Parallel) selected at compile time per type. **Polymorphic events** (`BaseEvent : INotification`): the generator builds a compile-time visitor/dispatch table over the known handler set for the base type and each derived type — no runtime `GetInterfaces()`.

**Streams.** `IAsyncEnumerable<T>` dispatch with the same fixed-order behavior chain (`IStreamPipelineBehavior<,>`); pre once, post after buffering where required.

**Exception handlers/actions (`IRequestExceptionHandler<,,>`, `IRequestExceptionAction<,>`).** This is the current code's most reflection-heavy corner (`MakeGenericType` + `GetMethod` + `Invoke` per `TRequest×TResponse×TException`). Generated replacement: per request, the generator knows every registered exception handler and its `TException`, and emits a typed cascade:

```csharp
try { /* handler + pipeline */ }
catch (Exception ex)
{
    switch (ex)
    {
        case ValidationException v: /* call ordered IRequestExceptionHandler<Req,Resp,ValidationException> */ break;
        case Exception e:           /* call ordered IRequestExceptionHandler<Req,Resp,Exception> */ break;
    }
}
```

Zero reflection; the combinatorial closure is resolved at build time. Handlers with no matching request are a diagnostic (orphan handler).

---

## 9. Discovery & membership

**Handler-first discovery** (not request-first): enumerate concrete `IRequestHandler<,>` / `IActionHandler<>` / `ICommandHandler<,>` / notification-handler implementations via `ForAttributeWithMetadataName` + symbol walking. Closed, findable classes → the request→response→handler triple falls out for free. This **deletes** `GenerateCombinations`, `GetConcreteRequestTypes`, `MaxTypesClosing`, and the whole open-generic closure engine.

Discovery is **membership-filtered** (a linker exports symbols explicitly), or multi-project solutions cross-link by accident:

- `[assembly: MediatorAssemblies(typeof(OrdersMarker))]` (compile-time equivalent of `MediatorOptions.Assemblies = [typeof(OrdersMarker)]`), or
- `[assembly: MediatorAssembly]`, or
- `[MediatorModule("Orders")]` on a handler/message (graph membership for the declaring assembly; **not** a pipeline name), or
- MSBuild `TimeWarpMediatorAssembly=true` (the generator package sets this for the host so apps do not need the attribute).

**Confirmed membership rule (004-001):** a compilation is a graph member only when it opts in through one of the markers above. Referenced assemblies are **never** linked just because they appear in `ProjectReference` / `PackageReference`. They join only with `[assembly: MediatorAssembly]` or by being listed as a `MediatorAssemblies` marker type. No marker → not linked.

Behaviors are listed explicitly with `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]` on a member assembly. Attribute order (then optional `order:`) is the compile-time pipeline: first listed is outermost, matching MediatR `GetServices().Reverse().Aggregate`.

### 9.1 Scoped senders (`ISender<TScope>`)

Named pipelines are **marker types**, not strings. `TScope` is an empty class/struct (`ClientPipeline`, `ServerPipeline`). The generator emits a concrete `Sender_{TScope}` implementing `ISender<TScope>` and a `Publisher_{TScope}` implementing `IPublisher<TScope>`, each with its own type-switched dispatch table and behavior chain. Unscoped `ISender` / `IPublisher` / generated `Mediator` is the **default pipeline**.

**Membership (how a handler or behavior is assigned to a scope):**

1. `[MediatorScope(typeof(ClientPipeline))]` on the handler, request, or a containing type. Closest type wins.
2. Else `[assembly: MediatorScope(typeof(ClientPipeline))]` as the default for types in that assembly that do not set their own.
3. Else the type belongs to the unscoped default pipeline.
4. Behaviors: `[assembly: MediatorBehavior(typeof(ClientOnly<,>), Scope = typeof(ClientPipeline))]`. Omitted `Scope` means the unscoped pipeline only. Unscoped behaviors never run on scoped requests; client behaviors never run on server requests.

If a handler and its request both specify a scope and they differ, that is **TWM003** (build error). Binding scope is handler scope if present, otherwise request scope.

**Unscoped vs scoped coexistence:** `AddGeneratedMediator()` registers only the unscoped sender/publisher/mediator and unscoped handlers. `AddGeneratedMediator<TScope>()` registers that pipeline independently. A host that only calls `AddGeneratedMediator()` cannot dispatch scoped requests (`NoHandlerException` on `Send(object)`; **TWM004** on a typed `ISender<TScope>.Send` of a request from another pipeline). Re-entrant `Send` stays in the same scope when the handler injects `ISender<TScope>`; injecting unscoped `ISender` is a different pipeline.

**TimeWarp.State switch (later task, not this repo):** inject `ISender<ClientPipeline>` on the Blazor client and `ISender<ServerPipeline>` on the server. Do not share one `IMediator` and filter inside behaviors.

---

## 10. Diagnostics (the headline DX feature)

Ship **two packages**: an **analyzer-only** package for domain/library projects (diagnostics, no emit, instant IDE feedback) and the **full generator** for hosts. Both read the same graph.

- **TWM001** request with no handler — build error (kills MediatR's #1 support burden; red squiggle in the *domain* project).
- **TWM002** duplicate handler for one command/request.
- **TWM003** handler and request assigned to different `TScope` markers.
- **TWM004** `ISender<TScope>.Send` of a request that is not a member of that pipeline.
- **TWM-DI** scoped service emitted into a static field (profile safety).
- Orphan behavior / orphan exception-handler (registered, matches nothing).
- Cycle in the handler dependency graph.
- ActionSet contract checks can be delegated to / aligned with State's existing analyzers.

---

## 11. Profiles

The graph is fixed; profiles choose *emission*, selected from project properties, not by handler authors.

| Profile | Dispatch | DI | Pipeline | `Send(object)` | Use |
|---------|----------|----|---------|---------------|-----|
| **Host** (default) | generated `Mediator` object | MS.DI scope-resolved | fixed-order scope chain | full switch | ASP.NET, State |
| **Link / AOT** | generated `Mediator` **+** `CallSiteInlining` interceptors | ServiceGen static fields | inline weave (pure/singleton) | switch, prunable | dev-cli, Nuru CLIs |
| **WASM-mini** | generated `Mediator` | either | fixed-order | trim `object` path off if unused | Blazor WASM size mode |

`CallSiteInlining` (interceptors) intercepts statically-typed call sites → `__Dispatch_*` directly, dropping the interface hop. **Never the default** ([OQ-A](#oq-a-interception-across-generators)); an optimization pass whose value must be *measured*, not asserted.

---

## 12. Hard open questions (promoted from hand-waves to design work)

### OQ-A: Interception across generators

Interceptors only rewrite the compilation that runs the mediator generator, and MediatR-shaped `Send` calls come from hundreds of sites across assemblies — so interceptor-first is disqualified as the default. **But** State's own generator emits `Sender.Send(new XxxActionSet.Action(...))` sugar in the *consuming app* project, where the mediator generator also runs. So a large fraction of real call sites are (a) generated and (b) in-scope for interception. **Design work:** define ordering/contract between the State generator and the mediator generator (does the mediator intercept State-generated call sites? does State emit direct `__Dispatch_*` calls instead?); verify behavior when the Nuru generator also runs in `dev-cli`. Cross-generator coupling is fragile — needs an explicit protocol or an agreed "State emits the direct call, mediator emits the target" split.

### OQ-B: Scoped-behavior composition

Resolved in principle ([§6](#6-pipeline-model)): fixed order at compile time, scope-resolved instances at send. **Design work:** confirm the generated chain matches today's `Reverse().Aggregate` semantics exactly (short-circuit, `StateTransactionBehavior` clone/restore + `ExceptionNotification` on throw, action-tracking re-entrant sends). Build a golden-file test from State's real behavior stack before generalizing.

### OQ-C: Exception- & attribute-driven behaviors

Exception handlers: typed cascade ([§8](#8-notifications-streams-exceptions-previously-hand-waved--now-explicit)). **Runtime-attribute behaviors** (`[TrackAction]` inspected at runtime): the generator can read those attributes at *compile time* and specialize — include/exclude the behavior branch per request, turning a runtime check into build-time specialization (a strict improvement). **Design work:** enumerate State's attribute-driven behaviors, decide which are presence-checks (compile-time specializable) vs. data-driven (emit attribute data as constants) vs. genuinely dynamic (keep runtime, document the cost).

### OQ-D: AOT-cleanliness is a gate, not a claim

The Nuru reference `NoWarn`s IL2026/IL3050 ("AOT warnings not yet implemented") — so it is **aspirational, not verified** AOT-clean. This spec must not repeat that. **Design work / M1 gate:** the AOT sample builds with `EnableTrimAnalyzer=true`, `EnableAotAnalyzer=true`, `IsAotCompatible=true`, and **no `NoWarn` on the IL family** — and is warning-clean. Explicitly verify the `Send(object)` switch, `OneOf<…>` returns, and `GetRequiredService<ConcreteType>` roots are trim-safe (the generator must root the concrete types).

---

## 13. What this deletes

| Removed | Why |
|---------|-----|
| `Wrappers/` (Request/Notification/Stream wrapper impls) | The generator *is* the wrapper |
| `ConcurrentDictionary` handler caches in `Mediator.cs` | Dispatch is a static call |
| `MakeGenericType` + `Activator.CreateInstance` (Mediator + exception behaviors) | Types known at build time |
| `GetInterfaces()` request/stream type discovery | Generated `switch` |
| `ServiceRegistrar.GenerateCombinations` / `GetConcreteRequestTypes` | Handler-first discovery; no closure engine |
| `MaxTypesClosing` / `MaxGenericTypeRegistrations` / `MaxGenericTypeParameters` | No combinatoric closure to bound |
| Runtime assembly scanning (`DefinedTypes`, `GetTypes`) | Compile-time discovery |
| `Reverse().Aggregate` pipeline construction | Fixed-order generated chain |

---

## 14. Milestones

**M1 — prove the graph + verification + generated `Mediator` core against State's real shape.**

| In M1 | Out of M1 |
|-------|-----------|
| `TimeWarp.Mediator.Analyzers` — TWM001, TWM002 | Interceptors / `CallSiteInlining` |
| Handler-first discovery + assembly membership | Call-graph pruning |
| Generated `sealed Mediator : IMediator`, monomorphic `Send` | Self-routing `IRequest<T,THandler>` |
| `Send(object)` → generated switch (JsonRequestHandler path) | Chaos analyzer / test doubles |
| **One real State ActionSet end-to-end**: `IncrementActionSet` + `StateTransactionBehavior` (scoped) chain | Full inline weave |
| ValueTask contracts incl. `IAction`/`ICommand` + nested Handler | Streams (design only) |
| `mediator.manifest.json` v1 | |
| MS.DI scope-resolved (State sample) + static-field DI (dev-cli AOT sample) | |

**M1 exit criteria:**
1. AOT sample publishes **trim/AOT-analyzer-clean with no `NoWarn`** on IL2026/IL3050 ([OQ-D](#oq-d-aot-cleanliness-is-a-gate-not-a-claim)).
2. TWM001 fires in a library that has the analyzer but not the generator.
3. The `IncrementActionSet` + `StateTransactionBehavior` golden file matches today's `Reverse().Aggregate` semantics ([OQ-B](#oq-b-scoped-behavior-composition)).
4. Benchmark reports **both** the generated-`Mediator` default *and* a `CallSiteInlining` prototype number, vs. the current `MakeGenericType` fork **and** martinothamar — gap documented honestly, not asserted.

**Then:** streams + exception cascade; the interceptor and pruning profiles; the cross-generator protocol ([OQ-A](#oq-a-interception-across-generators)). Scoped senders shipped in **004-002**.

---

## 15. Migration (clean break, staged)

- **Phase A — Coexistence.** Reflection runtime stays behind `Legacy`; new Contracts + generator ship alongside. `AddMediator()` prefers the generated implementation when present; scanning becomes the no-op shim ([§7](#7-di-model)).
- **Phase B — Parity gate.** Existing test suite + the State golden-file stack run against the generated implementation. Benchmarks are the merge gate.
- **Phase C — Consumer integration.** State updates base classes (`ActionHandler`, `MessagePreProcessor`) to the `ValueTask` signatures; State's generators emit calls the new mediator makes cheap. Architecture drops scanning `RegisterServicesFromAssemblies`.
- **Phase D — Delete.** Remove `Wrappers/`, `ServiceRegistrar` scanning, the closure engine, `MakeGenericType` paths.

---

## 16. One-line thesis

**The mediator is a compile-time graph purpose-built for the TimeWarp ecosystem's nested-`Handler` conventions: verify the request → handler → dependency graph at build (missing-handler = red squiggle), dispatch through a generated zero-reflection `Mediator` object with a compile-time-fixed / scope-resolved pipeline — and offer static-field DI, call-site interception, and pruning as opt-in AOT profiles whose value is measured, not assumed.**
