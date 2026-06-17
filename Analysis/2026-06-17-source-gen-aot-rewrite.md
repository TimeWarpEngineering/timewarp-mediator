# Source-Generated AOT Rewrite — Analysis

**Date:** 2026-06-17  
**Context:** Brainstorm for rewriting TimeWarp.Mediator with source generators and Native AOT compatibility, using [martinothamar/Mediator](https://github.com/martinothamar/Mediator) as a baseline and [TimeWarp.Nuru](https://github.com/TimeWarpEngineering/timewarp-nuru) as the bar for how far to push compile-time code generation.

---

## Where We Are Today

The runtime `Mediator` is classic reflection-era MediatR:

```csharp
var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(request.GetType(), static requestType =>
{
    var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
    var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException(...);
    return (RequestHandlerBase)wrapper;
});
```

Every `Send(object)` path does interface reflection. Wrappers call `GetServices<IPipelineBehavior<,>>().Reverse().Aggregate(...)`, which is AOT-hostile and allocates. `ServiceRegistrar` does assembly scanning with combinatorial open-generic closure — powerful, but expensive, reflection-heavy, and hard to trim.

[martinothamar/Mediator](https://github.com/martinothamar/Mediator) is the obvious baseline: monomorphized `Send<T>`, generated `AddMediator`, compile-time handler discovery, closed pipeline registrations for AOT. Good reference, but it still largely keeps the *runtime pipeline composition* model.

The Nuru kanban (tasks 443/444) signals intent to go **further** than that.

---

## Architectural North Star

From the Nuru plans, the end state looks like a **three-layer compile-time stack**:

```
TimeWarp.ServiceGen     → compile-time DI (constructor graphs, lifetimes, circular-dep errors)
TimeWarp.Mediator       → compile-time dispatch + optional inlined pipelines
TimeWarp.Nuru           → routes/interceptors on top
```

Mediator should not try to be its own DI container long-term — it should **consume** ServiceGen and emit dispatch code that calls `new Handler(dep1, dep2)` or static singleton fields directly.

---

## Package Split (Beyond martinothamar)

| Package | Role |
|---------|------|
| `TimeWarp.Mediator.Contracts` | Marker interfaces only (`IRequest`, `INotification`, `Unit`) — stays tiny, netstandard |
| `TimeWarp.Mediator` | Runtime abstractions, optional MS.DI bridge, no reflection |
| `TimeWarp.Mediator.SourceGenerator` | Installed only in host/executable projects |

Consider a fourth package later: `TimeWarp.Mediator.Analyzers` (diagnostics-only, no codegen) for fast IDE feedback in library projects that only define messages/handlers.

---

## Core Generator Ideas

### 1. Monomorphized dispatch (table stakes)

Per discovered message type, emit:

```csharp
public ValueTask<Pong> Send(Ping request, CancellationToken ct)
    => __Dispatch_Ping.Handle(request, ct);
```

For `Send(object)`, emit a **compile-time switch** or perfect-hash lookup — not `Dictionary<Type, ...>` with lazy `MakeGenericType`. martinothamar does this above a threshold; do it always for AOT.

### 2. Kill the wrapper layer entirely

`RequestHandlerWrapperImpl<,>` exists only because the runtime does not know types. The generator **is** the wrapper. Each message gets a generated `__PingDispatcher` with a single `Handle` method. Delete `Wrappers/`, `ConcurrentDictionary`, `Activator.CreateInstance`.

### 3. Inlined pipelines (real differentiator)

Nuru task 244 describes this well. Instead of resolving `IPipelineBehavior<TRequest,TResponse>` at runtime:

```csharp
// Generated per request type
static ValueTask<Pong> Handle_Ping(Ping req, CancellationToken ct)
{
    // behavior 1 (LoggingBehavior<Ping,Pong>) — resolved at compile time
    // behavior 2 (ValidationBehavior<Ping,Pong>)
    return __PingHandler.Handle(req, ct);
}
```

The generator:

- Reads `AddMediator(o => o.PipelineBehaviors = [...])` or assembly attributes at **compile time** (Nuru-style DSL interpretation)
- Specializes open generics (`LoggingBehavior<,>`) into closed types per message
- Emits an unrolled chain — zero `GetServices`, zero `Aggregate`, JIT can inline

This alone could beat martinothamar on benchmarks and aligns with how Nuru thinks.

### 4. Scoped mediators (`ISender<TScope>`)

Nuru task 443 calls this out explicitly. Generate **separate dispatch tables per scope marker**:

```csharp
ISender<ServerPipeline>   // server handlers + server behaviors only
ISender<ClientPipeline>   // WASM/Blazor handlers only
```

No runtime filtering in behaviors. Each scope is a separate generated class. Huge win for TimeWarp.State-style apps where one shared `IMediator` forces every behavior to check "is this my message?".

### 5. ServiceGen integration (not MS.DI reflection)

Emit handler resolution like Nuru's `ServiceResolverEmitter`:

- Singleton handlers → static `Lazy<T>` fields
- Transient handlers → `new Handler(resolvedDeps)`
- Scoped deps → explicit scope parameter on `Send`

Optional bridge: also populate `IServiceCollection` for apps that want MS.DI, but the **fast path** is static.

---

## CQRS and Semantic Message Types

martinothamar splits `ICommand` / `IQuery` / `IRequest`. The Nuru extraction plan goes further:

- `ICommand<T>`, `IQuery<T>`, `IIdempotentCommand<T>`
- Separate handler interfaces (`ICommandHandler<,>`, etc.)

**Out-of-the-box idea:** the generator enforces semantics at compile time:

| Message kind | Generated constraints |
|--------------|----------------------|
| `IQuery<T>` | Must be side-effect-free (analyzer: no `DbContext.SaveChanges`, no `ICommandBus.Send`) |
| `ICommand` | Must have exactly one handler |
| `IIdempotentCommand` | Must implement idempotency key; generator wires dedup store from ServiceGen |

Even if only the easy rules are enforced first (handler count, naming conventions), Mediator becomes a **CQRS contract enforcer**, not just a dispatch library.

---

## Notification Publishing — Go Further Than ForeachAwait

Today notifications resolve handlers via `GetServices<INotificationHandler<T>>()` at runtime.

**Generated approach per notification type:**

```csharp
static async ValueTask Publish_Pinged(Pinged n, CancellationToken ct)
{
    await __PingedHandler1.Handle(n, ct);
    await __PingedHandler2.Handle(n, ct);
    // or Task.WhenAll unrolled
}
```

Pick strategy at compile time (`ForeachAwait` vs `TaskWhenAll` vs `Parallel` for CPU-bound). No `IEnumerable<NotificationHandlerExecutor>`, no indirection.

**Polymorphic notifications:** if base-type handlers (`INotificationHandler<BaseEvent>`) are kept, generate a **visitor/dispatch table** at compile time rather than runtime interface scanning.

---

## Stream Requests

Generate `IAsyncEnumerable<TResponse>` dispatch the same way, with **inlined stream behaviors** (pre once, post after buffering). martinothamar's stream post-processors need buffering — the generator can emit that buffer as a `List<TResponse>` with known capacity hints if the handler's yield count is analyzable (rare but possible for simple generators).

---

## Compile-Time Discovery Model

Three tiers, from conservative to aggressive:

1. **Explicit markers** (recommended default): `[MediatorAssembly]` or `options.Assemblies = [typeof(AppMarker)]`
2. **Assignability roots**: `options.Types = [typeof(IModuleRequests)]` — include all assignable request types
3. **Call-graph pruning** (bold): scan the compilation for `sender.Send(...)` / `publisher.Publish(...)` and only generate dispatch for messages actually used. Shrinks AOT binaries for large domain models where only a fraction of commands are wired in a given host.

Tier 3 is very "Nuru" — treat the app like a DSL and interpret it at compile time.

---

## Diagnostics as a Product Feature

Emit build errors/warnings for things MediatR discovers at runtime:

- Request with no handler
- Multiple handlers for a request (unless opt-in)
- Handler registered but message type never implements `IRequest`
- Circular handler dependencies (via ServiceGen)
- Open behavior registered but no matching messages
- Notification handler for sealed type that can never be published
- Pipeline behavior order conflicts

This is where TimeWarp.Mediator can beat both MediatR and martinothamar for developer experience.

---

## API Evolution Ideas

### `ValueTask` everywhere

martinothamar already does this. Especially valuable with singleton handlers where sync completion is common.

### Concrete `Mediator` over `IMediator`

Keep both; document that the generated concrete type avoids interface dispatch.

### Dual registration modes

| Mode | When |
|------|------|
| `AddMediator()` | MS.DI apps, gradual migration |
| `MediatorHost.Configure(...).Build()` | Pure source-gen, AOT CLI/tools (Nuru path) |

### Handler authoring ergonomics

**Partial handler generation:**

```csharp
public partial class PingHandler : IRequestHandler<Ping, Pong>
{
    // user writes only Handle body; generator emits ctor + DI
    public partial ValueTask<Pong> Handle(Ping request, CancellationToken ct);
}
```

Or invert it: user writes a record `Ping`, generator scaffolds `PingHandler` stub with correct signature.

### Interceptor-based Send (very Nuru)

For apps not using `IMediator` injection everywhere, use C# interceptors to rewrite:

```csharp
await mediator.Send(new Ping());
```

into a direct static dispatch call at the call site. Zero indirection even through the mediator instance. Probably v2, but fits the toolchain philosophy.

---

## Performance / AOT Knobs

| Option | Effect |
|--------|--------|
| `CachingMode.Eager` / `Lazy` | martinothamar pattern — lazy for serverless cold start |
| `ServiceLifetime.Singleton` default | Handlers as singletons; document that transient is pessimization |
| `GenerateTypesAsInternal` | Host-only generated types |
| Pre-composed pipeline delegates | One delegate field per message type, built in static ctor |
| `struct` mediator option | Mediator as readonly struct holding only pre-resolved dispatch table refs |

---

## Migration Strategy

Do not try to keep 100% MediatR runtime compatibility. The current `migration.md` promises full API compatibility today — the source-gen rewrite is a **major version** with a staged path:

**Phase A — Coexistence**

- Keep `TimeWarp.Mediator` (reflection runtime) as `Legacy` or behind `#if`
- New packages: Contracts + SourceGenerator
- `AddMediator()` detects generator and uses generated implementation when present

**Phase B — Generator parity**

- Match existing test suite against generated implementation
- Benchmarks become the gate (`TimeWarp.Mediator.Benchmarks` vs martinothamar numbers)

**Phase C — Nuru integration**

- Extract abstractions per Nuru task 443
- Nuru stops owning `IMessage` / dispatch; calls into TimeWarp.Mediator

**Phase D — Delete**

- Remove `ServiceRegistrar`, `Wrappers/`, assembly scanning, generic closure explosion

---

## What to Steal vs What to Skip from martinothamar

**Steal:**

- Two-package abstractions + host-only generator
- Monomorphized `Send<T>`
- Compile-time `AddMediator` registration
- `MediatorOptions` as compile-time constants
- OpenTelemetry hooks in generated code
- `CachingMode`, singleton-by-default guidance

**Skip or defer:**

- Keeping runtime open-generic `IPipelineBehavior` resolution as the primary AOT path
- Scanning all referenced assemblies by default (prefer explicit markers like Nuru)
- Feature parity with every MediatR edge case (generic handler combinatorics, `MaxTypesClosing` — replace with compile-time errors or explicit closed registrations)

**Go beyond:**

- ServiceGen-owned DI
- Inlined pipelines
- Scoped `ISender<TScope>` / `IPublisher<TScope>`
- Nuru-style Locator → Extractor → IR → Emitter pipeline for `AddMediator` configuration
- Call-graph pruning
- CQRS/idempotency semantics

---

## Suggested Generator Pipeline (Mirror Nuru)

```
Locator      → find AddMediator(...) / [assembly: MediatorOptions]
Extractor    → handlers, behaviors, services, scopes from SemanticModel
Interpreter  → "execute" configuration at compile time
IR           → MessageGraph, PipelineGraph, ScopeGraph
Emitter      → DispatchEmitter, PipelineEmitter, RegistrationEmitter, Diagnostics
```

This architecture is proven in Nuru. Reuse the patterns — Mediator's DSL is simpler (`AddMediator` vs `NuruApp.CreateBuilder`).

---

## Bold "Think Outside the Box" Ideas

1. **Message graph visualization** — generator emits a Mermaid/Graphviz file of request→handler→dependencies as a build artifact.
2. **Compile-time chaos testing** — analyzer simulates "what if handler X throws" and verifies exception behaviors exist.
3. **Handler latency budgets** — `[Sla(50)]` on `IQuery<T>`; generator injects timeout behavior only for those types.
4. **Feature flags per message** — `#if` or constants in `MediatorOptions`; generator excludes dead dispatch paths from AOT binary.
5. **Source-generated test doubles** — for each handler, emit `PingHandlerStub` for unit tests (record/replay).
6. **WASM size mode** — only `ValueTask`, no `object Send`, no notifications, no streams — trim everything else.
7. **Mediator as middleware** — ASP.NET Core middleware generated per endpoint that calls dispatch directly without resolving `IMediator` from DI.
8. **Convention-based handlers without classes** — static methods with `[Handles(typeof(Ping))]`; generator wraps them (like Nuru delegates).

---

## Recommended First Milestone

Smallest slice that proves the architecture:

1. `TimeWarp.Mediator.SourceGenerator` in the host test project
2. Scan explicit assembly marker
3. Generate `Mediator` with monomorphized `Send<TRequest,TResponse>` only
4. No pipeline yet — direct handler call via ServiceGen-style `new Handler(...)`
5. AOT publish test project passes trim analysis
6. Benchmark shows clear win over current `MakeGenericType` path

Then add inlined pipelines and scoped senders — that is where TimeWarp.Mediator differentiates from martinothamar and aligns with the Nuru ecosystem.

---

## Related Work

- Nuru task 443: Extract Mediator abstractions with source-generated dispatch
- Nuru task 444: TimeWarp.ServiceGen — source-generated AOT-friendly DI container
- Nuru task 244: Emit inlined pipeline middleware
- Reference implementation: [martinothamar/Mediator](https://github.com/martinothamar/Mediator)