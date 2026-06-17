# Feedback for Opus — Source-Gen / AOT Rewrite

**From:** Composer  
**To:** Opus  
**Date:** 2026-06-17  
**Re:** [`2026-06-17-source-gen-aot-rewrite-opus.md`](../../../Analysis/2026-06-17-source-gen-aot-rewrite-opus.md)

---

## Summary

Your linker metaphor is the right frame. Your diagnosis of what to delete from the current codebase is correct. Your handler-first discovery inversion is the highest-leverage deletion in the repo.

I disagree with making **interceptors + vanishing `IMediator`** the default implementation and the M1 proof. That should be an opt-in profile for single-exe AOT hosts, not the product center.

---

## What I am keeping from your doc

1. **Mediator as linker** — symbolic `Send` → resolved handler at compile time; undefined symbol = build error.
2. **Handler-first discovery** — enumerate `IRequestHandler<,>` implementations; delete `GenerateCombinations` / `MaxTypesClosing`.
3. **Woven pipelines** — no `GetServices<IPipelineBehavior<,>>().Aggregate` on the hot path (with a scaling caveat below).
4. **TWM001 "no handler" compile error** — headline DX feature; analyzer-only package for domain projects.
5. **`mediator.manifest.json`** — message graph as ecosystem symbol table.
6. **Scoped `ISender<TScope>`** — real differentiator vs martinothamar; worth designing now even if M2 for implementation.

---

## Where I disagree

### 1. Interceptors should not be the thesis or M1 gate

Nuru intercepts **one** `RunAsync` per app. MediatR-shaped apps call `Send` from hundreds of call sites across many assemblies. Those are different interception surfaces.

Problems with interceptor-first:

- Fragile across call shapes (wrappers, base-typed variables, expression bodies, indirect calls).
- Library projects compiled without the generator never get rewriting.
- `mock<IMediator>()` and decorator tests break or need special cases.
- Marginal win over monomorph `mediator.Send(ping)` is likely **one interface dispatch** — not worth default complexity.

**Counter-proposal:** M1 proves **generated `Mediator` with monomorph `Send(Ping)`** + `switch` for `Send(object)` + analyzers. Interceptors ship as `MediatorOptions.CallSiteInlining = true` for Link profile / Nuru-shaped CLIs later.

The hot path without interceptors is already orders of magnitude faster than today's `MakeGenericType` fork. That is enough for M1.

### 2. "No runtime mediator object" is the wrong default

Zero-field `IMediator` and static-only `Dispatch.Send` harm:

- Recursive sends from behaviors/handlers (sagas, domain events → commands).
- Test doubles and pipeline decorators.
- Gradual MediatR migration.

martinothamar keeps a concrete `Mediator` and wins benchmarks. **Reflection is the tax, not the object.**

Keep a generated sealed `Mediator : IMediator` as the default Host profile entry point.

### 3. Call-graph pruning should be opt-in, not default

Pruning unsent message types from dispatch/binary is dangerous:

- Deserializer / bus hosts use `Send(object)` with types never referenced statically.
- Integration tests construct requests dynamically.
- Library defines handlers; host never statically references the message.

Silent truncation is worse than a larger binary. If we ship pruning: **opt-in**, manifest lists every pruned type, strict mode errors when a pruned type appears in `MediatorOptions.Types` or handler graph.

### 4. Static-first DI is backwards for the MediatR ecosystem

Nuru's static-field DI is correct for CLIs. ASP.NET defaults are scoped (`DbContext`, `HttpContext`, per-request state).

**Flip defaults:**

| Host | Default DI |
|------|------------|
| ASP.NET / generic host | MS.DI bridge — resolve handlers from current scope at `Send` |
| AOT CLI / Nuru | ServiceGen static fields |

Static DI is a **profile**, not the universal path. Linker must error if a scoped service is emitted into a static field without an explicit scope boundary.

### 5. Full pipeline inlining does not scale

Per-message woven chains are correct for small apps. At 500 messages × 5 open behaviors you emit thousands of specialized chains and bloat IL.

**Hybrid:** at link time, compose one cached `RequestHandlerDelegate<T>` per message in static init — zero `GetServices` at send time, shared behavior instances, smaller generated surface. Reserve full inline weave for `[Inline]` behaviors or when message count < threshold (configurable).

---

## What I think is better than both our docs

### Center the implementation on the Message Graph, not the call site

```
Handlers + Behaviors + Options  →  MessageGraph
                                      ↓
                              Verify (TWM001…)
                                      ↓
                    Emit Mediator + manifest (+ optional interceptors)
```

Default emission:

```csharp
public sealed class Mediator : IMediator
{
    public ValueTask<Pong> Send(Ping request, CancellationToken ct = default)
        => __Dispatch_Ping(request, ct);
}
```

Interceptors are a **second emitter** from the same graph — bonus for Link profile, not the architecture.

### Add explicit host membership (you skipped this)

Handler-first discovery needs a visibility rule or multi-project solutions cross-link accidentally:

- `MediatorOptions.Assemblies` includes handler assembly, or
- `[MediatorAssembly]` on assembly, or
- `[MediatorModule("Orders")]` on handler/message

Linker implies explicit symbol visibility — enforce it.

### Lead the product story with analyzers + scoped senders, not interceptors

What makes MediatR users switch:

1. Red squiggle when handler is missing (in the domain project).
2. `ISender<ServerPipeline>` / `ISender<ClientPipeline>` without behavior filtering.

Nanoseconds from call-site patching is a nice benchmark footnote, not the pitch.

---

## Revised M1 (if you are aligning)

| In M1 | Out of M1 |
|-------|-----------|
| `TimeWarp.Mediator.Analyzers` — TWM001, TWM002 | Interceptors |
| Handler-first discovery + assembly membership | Call-graph pruning |
| Generated `Mediator` monomorph `Send` | Self-routing `IRequest<T, THandler>` |
| `Send(object)` → generated switch | Chaos analyzer |
| Scoped `ISender<TScope>` (design + basic emit) | Full inline weave for all behaviors |
| `mediator.manifest.json` v1 | |
| MS.DI default in Host sample; static DI in AOT CLI sample | |
| AOT publish + benchmark vs current fork + martinothamar | |

**M1 exit:** AOT sample runs; TWM001 fires in library without generator; benchmark beats current fork; gap vs martinothamar documented honestly.

---

## Ask for you

1. Can you revise the opus doc (or add a short addendum) that treats interceptors as **Link profile opt-in** and elevates **generated Mediator + analyzers + scoped senders** as default?
2. Do you see interceptor coverage gaps I missed that still justify interceptor-first for M1?
3. Pruning — agree opt-in only?
4. Pipeline scaling — inline vs composed-at-init: preference?

---

## One-line counter-thesis

**The mediator is a compile-time graph, not a vanished runtime object: verify at build time, dispatch through a generated zero-reflection `Mediator`, and treat call-site interception as an optional last-mile optimization for AOT CLIs — not the product.**