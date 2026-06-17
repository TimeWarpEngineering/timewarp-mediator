# The Vanishing Mediator — A Bold Source-Gen / AOT Design

**Date:** 2026-06-17
**Author:** Opus 4.8 brainstorm
**Companion to:** [`2026-06-17-source-gen-aot-rewrite.md`](./2026-06-17-source-gen-aot-rewrite.md) (the broader survey of ideas)
**Decision context:** Clean break / major version. No commitment to preserve MediatR's open-generic combinatorics, `MaxTypesClosing`, or full assembly scanning. Edge cases become compile errors.
**Reference points:** [martinothamar/Mediator](https://github.com/martinothamar/Mediator) (the baseline to beat), [TimeWarp.Nuru](https://github.com/TimeWarpEngineering/timewarp-nuru) (proves the interceptor + compile-time DI toolchain in production on the .NET 10 stack we already run).

---

## Thesis

> **The mediator is a compile-time concept, not a runtime object. At runtime there is no mediator, no dispatch table, no pipeline composition, and no container lookup — only the direct, inlined function call you would have written by hand, with the cross-cutting concerns woven in.**

MediatR's indirection was a tax paid at runtime to buy decoupling at authoring time. We move the entire tax to compile time and refund it. The decoupled authoring experience is preserved; the runtime cost is not.

martinothamar generates a *faster mediator* — a real `Mediator` object with a generated dispatch table, still reached through an instance, an interface, and a lookup. **We generate the mediator away.** This is the structural difference, and it is only possible because C# interceptors are now stable on the toolchain Nuru already uses.

---

## The mental model: Mediator is a linker

Stop thinking "dispatcher." Think **linker**.

A native linker takes symbolic references (`call printf`) and resolves them to concrete addresses at link time. TimeWarp.Mediator does the same for message passing:

| Native linker | TimeWarp.Mediator |
|---------------|-------------------|
| `call printf` (symbolic reference) | `Send(new Ping())` (symbolic reference to "whatever handles Ping") |
| Resolve symbol → address at link time | Resolve request → `new PingHandler(deps).Handle(...)` at compile time |
| Undefined symbol → **link error** | No handler → **compile error** (red squiggle), not a runtime exception |
| Duplicate symbol → **link error** | Two handlers for one command → **compile error** |
| Patches the call site | Rewrites the call site via an **interceptor** |
| Dead-code strips unreferenced functions | Strips dispatch for requests never sent in this host |

Once the model is "linker," the design decisions answer themselves. Linkers don't do dictionary lookups at runtime — they patch call sites. Neither do we.

---

## Architecture: four layers, three of them compile-time

```
┌─────────────────────────────────────────────────────────────┐
│  AUTHORING (unchanged)                                       │
│    record Ping(string Msg) : IRequest<Pong>;                │
│    class PingHandler : IRequestHandler<Ping, Pong> { ... }  │
│    await mediator.Send(new Ping("hi"));   ← user writes this │
└─────────────────────────────────────────────────────────────┘
        │  the generator is a LINKER over the whole compilation
        ▼
┌─────────────────────────────────────────────────────────────┐
│  LINK TIME (the generator — mirrors Nuru's proven pipeline)  │
│    Locate  → every Send/Publish call site + every handler    │
│    Resolve → request → handler → ctor deps → behavior chain  │
│    Verify  → no-handler / dup-handler / cycle = COMPILE ERROR│
│    Weave   → inline behaviors per request (pay-as-you-use)   │
│    Emit    → interceptor that rewrites each call site        │
└─────────────────────────────────────────────────────────────┘
        ▼
┌─────────────────────────────────────────────────────────────┐
│  RUNTIME (what's left — almost nothing)                      │
│    the call site IS the handler call. no IMediator on path.  │
└─────────────────────────────────────────────────────────────┘
```

### Pillar 1 — Dispatch is call-site identity

Statically-typed sends are intercepted and rewritten at the call site. `IMediator` becomes a zero-field `readonly struct` — it exists only to be injectable and to give the interceptor a hook. In pure-AOT/CLI mode it disappears entirely behind a static `Dispatch.Send(req)`.

This is what martinothamar structurally cannot do (it predates stable interceptors) and what Nuru already does in production by intercepting `app.RunAsync(args)`.

### Pillar 2 — Pipelines are woven, not composed

No `GetServices<IPipelineBehavior<,>>().Reverse().Aggregate(...)`. Behaviors become inlined nested local functions, emitted **only for the requests they apply to**. A request with no applicable behaviors compiles to a bare handler call. This is pay-for-what-you-use at the IL level — invisible to the MediatR-style author, decisive on benchmarks and trimmed binary size. (Nuru's `behavior-emitter.cs` already does exactly this for its route handlers.)

### Pillar 3 — DI is pre-resolved

Lift Nuru's `DependencyGraphBuilder.TopologicalSort` + static-field emission directly:

- Singleton handlers / deps → cached static fields, constructed in dependency order.
- Transient → inlined `new Handler(resolvedDeps)`.
- Runtime MS.DI stays as an **escape hatch**: emit the captured `ConfigureServices` lambda verbatim into a generated method for consumers who need `AddDbContext`-style extension methods (Nuru's `UseMicrosoftDependencyInjection()` pattern).

This is the `TimeWarp.ServiceGen` layer the companion doc wants — but Nuru has a working implementation to lift rather than design from scratch.

### Pillar 4 — Discovery inverts: link from handlers, not requests

This is the move that **deletes the worst code in the current codebase.**

Today `ServiceRegistrar` scans *requests* and computes open-generic closures (`GenerateCombinations`, `GetConcreteRequestTypes`, `MaxTypesClosing`, `MaxGenericTypeRegistrations`). Instead, enumerate concrete `IRequestHandler<,>` implementations — they are closed, findable classes — and the request → response → handler triple falls out for free.

The entire combinatoric closure engine and its configuration knobs evaporate. Unhandled generic cases become a clean compile error instead of a runtime closure attempt.

---

## What the generated code actually looks like

```csharp
// ── User call site (untouched source) ──────────────────────────
var pong = await mediator.Send(new Ping("hi"));

// ── Generated interceptor rewrites it to ──────────────────────
[InterceptsLocation(1, "…opaque base64…")]
public static ValueTask<Pong> __Send_Ping(
    this IMediator _, Ping request, CancellationToken ct = default)
{
    // Behaviors woven inline — ONLY the ones that apply to Ping.
    // LoggingBehavior is a Lazy<> static singleton, resolved at link time.
    __Behaviors.Logging.Begin(request);
    try
    {
        // ctor deps pre-resolved: __svc_Repo is a topo-sorted static field
        return new PingHandler(__svc_Repo).Handle(request, ct);
    }
    finally { __Behaviors.Logging.End(); }
}

// ── A Ping with ZERO applicable behaviors emits just: ──────────
//     return new PingHandler(__svc_Repo).Handle(request, ct);

// ── Dynamic fallback: Send(object) from a bus / deserializer ───
public static ValueTask<object?> Dispatch(object request, CancellationToken ct) => request switch
{
    Ping p => __Box(__Send_Ping(default, p, ct)),   // generated switch — no MakeGenericType
    // … one arm per linked request type …
    _ => throw new NoHandlerException(request.GetType())  // the ONLY surviving runtime "not found"
};
```

The hot path is allocation-free, interface-free, and lookup-free. The `Wrappers/` directory, the `ConcurrentDictionary` caches, `Activator.CreateInstance`, and every `MakeGenericType` in `Mediator.cs` are not optimized — they are **deleted**.

---

## Notifications and streams, the same way

**Publish** emits an unrolled chain per notification type, with the dispatch strategy chosen at link time:

```csharp
static async ValueTask __Publish_Pinged(Pinged n, CancellationToken ct)
{
    await new Handler1(__svc_A).Handle(n, ct);   // ForeachAwait
    await new Handler2(__svc_B).Handle(n, ct);
    // or Task.WhenAll(...) unrolled, or Parallel — selected per type
}
```

No `IEnumerable<NotificationHandlerExecutor>`, no runtime interface scan. Polymorphic (base-type handler) notifications generate a compile-time visitor table rather than runtime `GetInterfaces()`.

**Streams** generate `IAsyncEnumerable<TResponse>` dispatch with inlined stream behaviors the same way.

---

## The killer features that fall out for free

1. **Missing handler = compile error.** Ship two packages: an **analyzer-only** package for library projects (diagnostics, no emit, instant IDE feedback) and the **full generator** for hosts. This kills MediatR's #1 support burden ("handler not found" at runtime) and is the single most marketable feature here.

2. **Scoped `ISender<TScope>` / `IPublisher<TScope>`** as separate generated classes — server handlers and WASM handlers never share a dispatch table, behaviors never need "is this my message?" checks. The TimeWarp.State / Blazor win, impossible to retrofit onto martinothamar.

3. **The compilation is the documentation.** The linker already built the full message → handler → dependency graph. Emit it as a build artifact: a Mermaid diagram **and** a queryable `mediator.manifest.json` that the rest of the TimeWarp ecosystem (Nuru, State) can consume. Mediator becomes the **symbol table for the whole app's message passing.**

---

## The bold ideas (think outside the box)

1. **`static abstract` self-routing requests (C# 11).** Let the type carry its own handler: `record Ping : IRequest<Pong, PingHandler>`, generator fills `THandler`. Then `Send<T>` resolves `T.Handler` with no registry at all — **the type system *is* the routing table.** Neither MediatR nor martinothamar has this. Research spike; composes with Pillar 1.

2. **One source, three runtime profiles — the generator is a tiered compiler.** The same handler code emits differently per project capability:
   - `WASM-mini` — only `ValueTask`, no `object` path, no streams, no notifications; trim everything else.
   - `AOT-static` — static-field DI, no container.
   - `MS.DI-bridge` — gradual migration, runtime container.

   The emission *strategy* is chosen by the linker from project properties, not by the author.

3. **Per-message dead-code elimination via call-graph pruning.** The linker found every `Send` call site. A `Ping` that is never sent in this host generates **no dispatch code** and is stripped from the AOT binary. Large shared domain + small host → tiny binary. Log what was pruned (no silent truncation).

4. **Source-generated test doubles + a chaos analyzer.** For each handler, emit a `PingHandlerStub` (record/replay) for unit tests. Add an analyzer that simulates "what if this handler throws" and verifies the registered exception behaviors actually cover it — compile-time fault injection.

5. **CQRS semantics as link-time contracts.** `IQuery<T>` analyzer-enforced side-effect-free (no `SaveChanges`, no command sends); `ICommand` must have exactly one handler; `[Sla(50)]` injects a timeout behavior *only* on the marked types. Mediator stops being a dispatch library and becomes a **CQRS contract enforcer.**

---

## What this deletes from the current codebase

| Removed | Why it can go |
|---------|---------------|
| `Wrappers/` (Request/Notification/StreamRequest wrapper impls) | The generator *is* the wrapper |
| `ConcurrentDictionary` handler caches in `Mediator.cs` | Dispatch is a static call, nothing to cache |
| `MakeGenericType` + `Activator.CreateInstance` (Mediator.cs, exception behaviors) | Types are known at link time |
| `GetInterfaces()` request-type discovery (untyped `Send`/`CreateStream`) | Generated `switch` over known types |
| `ServiceRegistrar.GenerateCombinations` / `GetConcreteRequestTypes` | Discovery inverts to handlers; no closure engine |
| `MaxTypesClosing` / `MaxGenericTypeRegistrations` / `MaxGenericTypeParameters` knobs | No combinatoric closure to bound |
| Runtime assembly scanning (`assembly.DefinedTypes`, `GetTypes`) | Compile-time discovery |

---

## Generator pipeline (mirror Nuru, proven)

```
Locate    → Send/Publish/CreateStream call sites + IRequestHandler<,> / INotificationHandler<> impls
Extract   → handlers, behaviors, ctor deps, scopes from the SemanticModel
Resolve   → request → handler → dependency graph (topological sort)
Verify    → no-handler / duplicate-handler / cycle / orphan-behavior diagnostics
Weave     → per-request behavior chains as inlined local functions
Emit      → InterceptorEmitter, DispatchSwitchEmitter, DI (static fields), ManifestEmitter
```

Use `ForAttributeWithMetadataName` for handler discovery, `SemanticModel.GetInterceptableLocation` for call-site interception, and `InterceptorsNamespaces` (the .NET 10 rename of `InterceptorsPreviewNamespaces`) in `Directory.Build.props` — all exactly as Nuru wires them. Validate the same model that is emitted, so the analyzer never drifts from the generator. Emit generated files to disk (`EmitCompilerGeneratedFiles`) for golden-file tests.

---

## First milestone (decisive)

The first slice must prove the **interceptor + linker** thesis, not merely monomorphization:

1. `TimeWarp.Mediator.SourceGenerator` — discover handlers via `ForAttributeWithMetadataName` (Pillar 4: link from handlers).
2. Intercept statically-typed `Send(new T())` call sites → direct `new Handler(deps).Handle(...)` with static-field DI (Pillars 1 + 3).
3. Generated `switch` for `Send(object)` — the one place runtime dispatch survives.
4. "No handler" / "duplicate handler" **compile errors** (the marketable feature).
5. AOT-publish the test app clean + benchmark vs the current `MakeGenericType` path **and** vs martinothamar. That head-to-head is the proof.

Then inlined pipelines (Pillar 2) and scoped senders are the follow-on differentiators that put daylight between TimeWarp.Mediator and martinothamar.

---

## Migration (clean break, staged)

- **Phase A — Coexistence.** Reflection runtime stays available behind `Legacy`; new Contracts + SourceGenerator packages ship alongside. `AddMediator()` prefers the generated implementation when the generator is present.
- **Phase B — Parity gate.** Run the existing test suite against the generated implementation. Benchmarks (vs martinothamar) become the merge gate.
- **Phase C — Nuru / ServiceGen integration.** Extract abstractions (Nuru task 443); Mediator consumes ServiceGen (task 444) instead of MS.DI reflection.
- **Phase D — Delete.** Remove `Wrappers/`, `ServiceRegistrar` scanning, the closure engine, and the `MakeGenericType` paths.

---

## One-line summary

**A mediator that compiles itself out of existence — framed as a linker for message passing, with the compile-time "no handler" error as its headline feature, the inlined zero-allocation call as its performance story, and the queryable message graph as its ecosystem hook.**
