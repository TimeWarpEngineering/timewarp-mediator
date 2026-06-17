# The Vanishing Mediator — A Bold Source-Gen / AOT Design

**Date:** 2026-06-17
**Author:** Opus 4.8 brainstorm
**Status:** Revised 2026-06-17 after Composer review — see [`composer-feedback-for-opus.md`](../.agents/collaboration/2026-06-17-source-gen-aot-rewrite/composer-feedback-for-opus.md) and [`opus-response-for-composer.md`](../.agents/collaboration/2026-06-17-source-gen-aot-rewrite/opus-response-for-composer.md). Interceptors and "vanishing" demoted from default to the **Link profile**; the **MessageGraph** is the architectural center; DI defaults to **scope-resolved**.
**Companion to:** [`2026-06-17-source-gen-aot-rewrite.md`](./2026-06-17-source-gen-aot-rewrite.md) (the broader survey of ideas)
**Decision context:** Clean break / major version. No commitment to preserve MediatR's open-generic combinatorics, `MaxTypesClosing`, or full assembly scanning. Edge cases become compile errors.
**Reference points:** [martinothamar/Mediator](https://github.com/martinothamar/Mediator) (the baseline to beat), [TimeWarp.Nuru](https://github.com/TimeWarpEngineering/timewarp-nuru) (proves the interceptor + compile-time DI toolchain in production on the .NET 10 stack we already run).

---

## Thesis

> **The mediator is a compile-time *graph*, not a runtime lookup. We verify the request → handler → dependency graph at build time, then dispatch through a generated zero-reflection `sealed Mediator : IMediator` resolved from the ambient scope — and in single-exe AOT, that same graph lets the mediator collapse into static, call-site-inlined dispatch.**
>
> The graph is the architecture. The *vanishing* — no instance, no interface, no allocation — is a **profile** (Link), true where it's true (single-exe AOT) and not forced on everyone.

MediatR's indirection was a tax paid at runtime — *reflection* (`MakeGenericType`, `Activator`, wrapper allocation, dictionary lookup) — to buy decoupling at authoring time. We move that tax to compile time and refund it. **Reflection is the tax, not the `Mediator` object:** martinothamar keeps a concrete `Mediator` and wins benchmarks precisely because it deleted the reflection, not the instance. So do we — and then go further with build-time verification, scoped senders, and an optional Link profile that martinothamar's pre-interceptor design can't reach.

> **Revision note.** The original draft made *call-site interceptors* + a phantom `IMediator` the default and the M1 proof. Composer's review (correctly) showed that's the wrong default for the MediatR ecosystem: `Send` is called from hundreds of call sites across assemblies, the generator only rewrites the compilation it runs in, and recursive sends/mocks/decorators need a real instance. The interceptor is now an **opt-in optimization** (`MediatorOptions.CallSiteInlining`), and the generated `Mediator` is the default. The sections below retain the original "Pillar" framing but each is annotated with where the default landed after review.

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
| Emits a call into the resolved target | Emits `Mediator.Send(Ping)` → `__Dispatch_Ping(...)`; **optionally** patches the call site via an interceptor (Link profile) |
| Dead-code strips unreferenced functions | **Optionally** strips dispatch for requests never sent in this host (opt-in pruning) |
| **Linkage** — internal/external symbols, translation units, explicit exports | Handlers are symbols with linkage; `[MediatorAssembly]` / `[MediatorModule]` is the export declaration |

Once the model is "linker," the design decisions answer themselves — including the ones the first draft got wrong. A linker resolves symbols at build time and **emits a call to the resolved target**; it does *not* require rewriting every caller's machine code in place. So our default is a generated `Mediator` whose methods *are* the resolved targets — and call-site patching (interceptors) is an extra pass we run only where it pays (single-exe AOT). And a linker has **linkage rules**: symbols are visible only when exported. Handler-first discovery needs the same — an explicit assembly/module membership rule — or a multi-project solution cross-links by accident.

---

## Architecture: the MessageGraph is the center

Everything is one IR — the **MessageGraph** — built once from the code, verified once, read by several emitters. The call site is *one consumer*, not the architecture. (Composer's reframe, adopted.) Default emission is a real `sealed Mediator : IMediator` whose monomorphic `Send(Ping)` calls `__Dispatch_Ping(...)` — no `MakeGenericType`, no wrapper, no dictionary. The interceptor emitter (Link profile) reuses the *same* `__Dispatch_Ping` bodies and additionally patches statically-typed call sites. Same graph, second pass.

```
        Handlers + Behaviors + MediatorOptions
                         |
                         v   build MessageGraph (IR from SemanticModel)
        +----------------+----------------+
        v                v                v
    [ Verify ]    [ Emit default ]   [ Emit optional ]
    TWM001...     sealed Mediator    interceptors +
    compile       : IMediator        static Dispatch
    errors        + manifest.json    (Link profile)
```

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
│  RUNTIME — DEFAULT: generated Mediator, reflection deleted   │
│    sender.Send(req) → Mediator.Send(Ping) → __Dispatch_Ping  │
│  RUNTIME — LINK PROFILE: the mediator vanishes               │
│    call site IS the handler call. static dispatch, no instance│
└─────────────────────────────────────────────────────────────┘
```

### Pillar 1 — Dispatch identity is resolved at build time *(default: generated `Mediator`; Link profile: intercepted call site)*

**Default.** The graph resolves every request to a concrete `__Dispatch_*` body. The generated `sealed Mediator : IMediator` exposes monomorphic `Send(Ping)` over those bodies and a `switch` for `Send(object)`. A real instance is injected and dispatches normally — which is what recursive sends (sagas, process managers, domain-event → command), mocks, decorators, and gradual MediatR migration all require. **Reflection is the tax, not the object** (Composer, correct).

**Link profile (`MediatorOptions.CallSiteInlining = true`).** For single-exe AOT hosts, a second emitter additionally intercepts statically-typed call sites and rewrites them to the `__Dispatch_*` bodies directly, removing the last interface hop and letting `IMediator` shrink to a phantom / static `Dispatch.Send`. This is the **benchmark-trophy + single-file-AOT** knob — turn it on to win martinothamar's microbenchmark and ship one trimmed binary; leave it off everywhere else.

Why not default? Composer's coverage argument settles it: Nuru intercepts **one** `RunAsync` per app, but MediatR-shaped code calls `Send` from hundreds of call sites across assemblies, and **the generator only rewrites the compilation it runs in** — a library injecting `IMediator` is never intercepted from the host. The generated-`Mediator` default reaches that library across the assembly boundary for free; interceptors cannot. So interceptors are an optimization pass, not the architecture.

### Pillar 2 — Pipelines are resolved at build time *(default: composed-at-init delegate; opt-in: inline weave)*

Either way, the win is the same: **no `GetServices<IPipelineBehavior<,>>().Reverse().Aggregate(...)` at send time, no runtime composition.** The graph knows the behavior chain per message; the only question is how it's emitted.

**Default — composed-at-init delegate.** Per message, build one cached `RequestHandlerDelegate<T>` in static init: shared behavior instances, uniform decorate/test surface, one small build per message. This is Composer's hybrid and it's right for the LOB common case (hundreds of messages where startup IL matters more than per-send nanoseconds).

**Opt-in — inline weave (`[Inline]` behavior, or `MessageCount < threshold`, or perf/Link profile).** Behaviors emitted as inlined nested local functions per message; a request with no applicable behaviors compiles to a bare handler call. Faster per send (inlinable, zero-alloc on sync `ValueTask`) at the cost of method-body IL.

> **Sharpening (vs the first draft and the review).** The IL-size driver is *closed behavior-type instantiation* — `LoggingBehavior<,>` → `LoggingBehavior<Ping,Pong>` is N×M distinct closed types — and that cost is **identical** in both approaches (the cached delegate still closes over a closed behavior). So "full inline doesn't scale" isn't a *size* argument: the real tradeoff is **per-call inlining vs. uniform decoratable surface**, not type-count. Inline weave adds linear method-body IL, not "thousands of chains." Default to composed-at-init for uniformity; reach for inline weave when per-send latency is the product. (Nuru's `behavior-emitter.cs` demonstrates the inline form.)

### Pillar 3 — DI resolution is generated, but the container owns lifetime *(default: scope-resolved; profile: static fields)*

The first draft made static-field DI the default. That's **backwards for the MediatR ecosystem** (Composer, correct): ASP.NET is overwhelmingly *scoped* — `DbContext`, `HttpContext`, per-request state. Emitting a scoped dep as a static singleton is a captive-dependency bug and a threading hazard. Defaults flip:

| Host | Default DI |
|------|-----------|
| ASP.NET / generic host | **MS.DI scope-resolved** — `scope.GetRequiredService<PingHandler>()` at `Send` |
| AOT CLI / Nuru / Link | **ServiceGen static fields** — Nuru's `DependencyGraphBuilder.TopologicalSort`, singletons cached, transients inlined `new` |

Key point that keeps the win: the AOT-hostile thing is *open-generic resolution + `MakeGenericType` + `Activator`*, **not** `GetRequiredService<ConcreteHandler>()`. The generator knows the closed handler type, so even the MS.DI default emits a trim-safe, scope-correct, reflection-free resolve — the wrapper/dictionary/`Activator` are still deleted. We keep ~all the perf without owning lifetime.

**Precision (mine, on top of Composer's flip):** don't resolve *everything* from scope. The graph knows each participant's lifetime — emit **singleton behaviors as cached static fields, scoped handler + scoped deps from scope**. "Resolve from current scope" is right for the handler, too coarse for the pipeline.

**Linker safety diagnostic:** when the ServiceGen profile would emit a *scoped* service into a *static* field without an explicit scope boundary, the linker **errors** (TWM-DI). Static DI is a profile, not the universal path.

The runtime MS.DI escape hatch (emit the captured `ConfigureServices` lambda verbatim, Nuru's `UseMicrosoftDependencyInjection()` pattern) covers consumers who need `AddDbContext`-style extension methods. This is the `TimeWarp.ServiceGen` layer the companion doc wants — Nuru already has a working implementation to lift.

### Pillar 4 — Discovery inverts: link from handlers, not requests

This is the move that **deletes the worst code in the current codebase.**

Today `ServiceRegistrar` scans *requests* and computes open-generic closures (`GenerateCombinations`, `GetConcreteRequestTypes`, `MaxTypesClosing`, `MaxGenericTypeRegistrations`). Instead, enumerate concrete `IRequestHandler<,>` implementations — they are closed, findable classes — and the request → response → handler triple falls out for free.

The entire combinatoric closure engine and its configuration knobs evaporate. Unhandled generic cases become a clean compile error instead of a runtime closure attempt.

---

## What the generated code actually looks like

```csharp
// ── User call site (untouched source) ──────────────────────────
var pong = await mediator.Send(new Ping("hi"));   // mediator is the injected generated instance

// ── DEFAULT: generated sealed Mediator : IMediator ─────────────
public sealed class Mediator(IServiceProvider scope) : IMediator
{
    // monomorphic overload — no MakeGenericType, no wrapper, no dictionary
    public ValueTask<Pong> Send(Ping request, CancellationToken ct = default)
    {
        // ASP.NET default: handler + scoped deps from the ambient scope;
        // singleton behaviors are cached static fields (resolved once at link time).
        var handler = scope.GetRequiredService<PingHandler>();   // trim-safe: closed type
        return __Pipeline_Ping.Invoke(request, handler.Handle, ct);  // composed-at-init delegate
    }

    // Dynamic path: Send(object) from a bus / deserializer
    public ValueTask<object?> Send(object request, CancellationToken ct = default) => request switch
    {
        Ping p => __Box(Send(p, ct)),   // generated switch — no MakeGenericType
        // … one arm per linked request type …
        _ => throw new NoHandlerException(request.GetType())  // the ONLY surviving runtime "not found"
    };
}

// ── LINK PROFILE (CallSiteInlining): a SECOND emitter, same bodies ──
// Patches the call site above directly to the dispatch body, dropping the interface hop:
[InterceptsLocation(1, "…opaque base64…")]
public static ValueTask<Pong> __Send_Ping(this IMediator _, Ping request, CancellationToken ct = default)
    => __Dispatch_Ping(request, ct);   // and __Dispatch_Ping may inline-weave behaviors + static-field DI
```

The hot path is allocation-free and lookup-free in both profiles; the Link profile additionally drops the interface hop. The `Wrappers/` directory, the `ConcurrentDictionary` caches, `Activator.CreateInstance`, and every `MakeGenericType` in `Mediator.cs` are not optimized — they are **deleted**.

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

## The product pitch (lead here — *not* with interceptor nanoseconds)

What actually makes a MediatR user switch is DX and capability, not benchmark footnotes (Composer, agreed):

1. **Missing handler = compile error.** Ship two packages: an **analyzer-only** package for library projects (diagnostics, no emit, instant IDE feedback) and the **full generator** for hosts. This kills MediatR's #1 support burden ("handler not found" at runtime) and is the single most marketable feature here. Red squiggle in the *domain* project, before you even run.

2. **Scoped `ISender<TScope>` / `IPublisher<TScope>`** as separate generated classes — server handlers and WASM handlers never share a dispatch table, behaviors never need "is this my message?" checks. The TimeWarp.State / Blazor win, impossible to retrofit onto martinothamar. Worth designing in M1 even if full emit is M2.

3. **The compilation is the documentation.** The graph already holds message → handler → dependency. Emit it as a build artifact: a Mermaid diagram **and** a queryable `mediator.manifest.json` that the rest of the TimeWarp ecosystem (Nuru, State) can consume. Mediator becomes the **symbol table for the whole app's message passing.**

### Host membership (the gap the first draft skipped)

Handler-first discovery (Pillar 4) needs an explicit visibility rule or a multi-project solution cross-links by accident — Composer caught this. A linker has **linkage**; so do we. Membership is declared by one of:

- `MediatorOptions.Assemblies = [typeof(OrdersMarker)]` — include this assembly's handlers, or
- `[assembly: MediatorAssembly]` — opt the whole assembly in, or
- `[MediatorModule("Orders")]` on a handler/message — name a module for scoped senders.

No marker → not linked. Explicit exports, exactly like a linker.

---

## The bold ideas (think outside the box)

1. **`static abstract` self-routing requests (C# 11).** Let the type carry its own handler: `record Ping : IRequest<Pong, PingHandler>`, generator fills `THandler`. Then `Send<T>` resolves `T.Handler` with no registry at all — **the type system *is* the routing table.** Neither MediatR nor martinothamar has this. Research spike; composes with Pillar 1.

2. **One source, three runtime profiles — the generator is a tiered compiler.** The same handler code emits differently per project capability:
   - `WASM-mini` — only `ValueTask`, no `object` path, no streams, no notifications; trim everything else.
   - `AOT-static` — static-field DI, no container.
   - `MS.DI-bridge` — gradual migration, runtime container.

   The emission *strategy* is chosen by the linker from project properties, not by the author.

3. **Per-message dead-code elimination via call-graph pruning — *opt-in only*.** The linker found every `Send` call site, so a `Ping` never sent in this host *could* be stripped from the AOT binary (large shared domain + small host → tiny binary). But pruning by static reachability is **unsound** the moment a host uses `Send(object)` from a bus/deserializer — every type is reachable through a path the linker can't see, and silent truncation becomes a production `NoHandlerException`. So (Composer, agreed): **off by default; opt-in; bound to the `Send(object)` switch; the manifest lists every pruned type; strict mode errors if a pruned type appears in `MediatorOptions.Types` or the handler graph.** Opting in means "I accept excluded types throw from the dynamic path" — a deliberate host decision.

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
Locate    → IRequestHandler<,> / INotificationHandler<> impls (membership-filtered) + Send/Publish call sites
Extract   → handlers, behaviors, ctor deps (with lifetimes), scopes from the SemanticModel
Build     → the MessageGraph IR (request → handler → deps → behaviors; topological sort)
Verify    → no-handler / duplicate-handler / cycle / orphan-behavior / scoped-into-static diagnostics
Emit      → MediatorEmitter (default) + ManifestEmitter           ← always
            + ScopedSenderEmitter (per [MediatorModule])
            + InterceptorEmitter (Link profile, CallSiteInlining) ← optional, same dispatch bodies
            + PruningFilter (opt-in)
```

Note the order: discovery is **handler-first** and **membership-filtered**; the call site is located for diagnostics and for the optional interceptor pass, not as the primary input. Every emitter reads the *same* verified graph, so the analyzer-only package and the generator never drift.

Use `ForAttributeWithMetadataName` for handler discovery, `SemanticModel.GetInterceptableLocation` for call-site interception, and `InterceptorsNamespaces` (the .NET 10 rename of `InterceptorsPreviewNamespaces`) in `Directory.Build.props` — all exactly as Nuru wires them. Validate the same model that is emitted, so the analyzer never drifts from the generator. Emit generated files to disk (`EmitCompilerGeneratedFiles`) for golden-file tests.

---

## First milestone (revised after review — Composer's M1, adopted)

The first slice proves the **graph + verification + generated `Mediator`** core, not interceptors.

| In M1 | Out of M1 |
|-------|-----------|
| `TimeWarp.Mediator.Analyzers` — TWM001 (no handler), TWM002 (duplicate) | Interceptors / `CallSiteInlining` |
| Handler-first discovery + assembly membership rule | Call-graph pruning |
| Generated `sealed Mediator : IMediator` with monomorphic `Send` | Self-routing `IRequest<T, THandler>` |
| `Send(object)` → generated `switch` | Chaos analyzer |
| Scoped `ISender<TScope>` — design + basic emit | Full inline weave for all behaviors |
| `mediator.manifest.json` v1 | |
| MS.DI scope-resolved in Host sample; static DI in AOT CLI sample | |
| AOT publish + benchmark vs current fork **and** martinothamar | |

**M1 exit criteria:** AOT sample runs trim-clean; TWM001 fires in a library project that has the analyzer but not the generator; benchmark beats the current `MakeGenericType` fork; the gap vs martinothamar is documented honestly. **And** the benchmark reports both the generated-`Mediator` default number *and* a `CallSiteInlining` prototype number — so the interceptor's marginal value is *measured*, not asserted. If it's noise on a realistic workload, that's evidence for keeping it a niche profile; if it wins the trophy, that's the AOT headline.

Then composed-vs-inline pipeline tuning (Pillar 2), full scoped-sender emit, and the optional interceptor profile are the follow-on work that puts daylight between TimeWarp.Mediator and martinothamar.

---

## Migration (clean break, staged)

- **Phase A — Coexistence.** Reflection runtime stays available behind `Legacy`; new Contracts + SourceGenerator packages ship alongside. `AddMediator()` prefers the generated implementation when the generator is present.
- **Phase B — Parity gate.** Run the existing test suite against the generated implementation. Benchmarks (vs martinothamar) become the merge gate.
- **Phase C — Nuru / ServiceGen integration.** Extract abstractions (Nuru task 443); Mediator consumes ServiceGen (task 444) instead of MS.DI reflection.
- **Phase D — Delete.** Remove `Wrappers/`, `ServiceRegistrar` scanning, the closure engine, and the `MakeGenericType` paths.

---

## One-line summary (merged thesis)

**The mediator is a compile-time graph: verify the request → handler → dependency graph at build (the "no handler" red squiggle is the headline), dispatch through a generated zero-reflection `sealed Mediator : IMediator` resolved from the ambient scope — and in single-exe AOT, let that graph collapse the mediator into static, call-site-inlined dispatch. The graph is the architecture; the *vanishing* is a profile.**
