# Dispatcher emission: non-boxing interface Send, cached delegate chain, manifest

## Description

The generated `Mediator` routes `ISender.Send<TResponse>` through `SendCore` → `Send(object)` →
a type switch → `Box<T>`, so the interface path allocates 240 B and is no faster than
reflection. Behaviors are chained with per-call closures, and `AddGeneratedMediator()` emits
interface-mapped descriptors only so State's logging middleware can inspect the container.
Parent review task 021, findings F2, F4, F7, F12, F13; gate from 021-001.

## Requirements

- `Send<TResponse>(IRequest<TResponse>)` and `Send(IRequest)` are emitted as type switches
  whose arms return the typed `ValueTask` from `Dispatch_*` with no boxing. `Send(object)` and
  `Publish(object)` are the only boxing paths.
- `Dispatch_*` resolves the handler and the scoped behaviors in compile-time order and runs a
  chain of cached static `MessageHandlerDelegate<TRequest, TResponse>` instances; no closure
  allocation per call when behaviors are present.
- `Publish<TNotification>` is typed and awaits handlers sequentially (today's semantics; State
  relies on completion after all handlers finish).
- `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` register the dispatcher, the
  scoped `Sender_{Scope}` / `Publisher_{Scope}` types and the concrete handler and behavior
  types only. The extension class lives in the host's generated namespace.
- Emit a generated `MediatorManifest` static type exposing, per request type, the message kind
  (`Action`, `Command`, `Query`, `IdempotentCommand`, `Request`), the response type, and the
  ordered behavior list per scope. This replaces interface-mapped descriptors for State and
  string matching for Nuru.
- Emit `[assembly: GeneratedMediator(typeof(Mediator))]`.
- Gate: the comparison benchmarks from 021-001 show interface Ping/Pong ≤ 1.5× martinothamar
  and ≤ 24 B, void send 0 B, and the State-shaped pipeline allocation-free apart from the
  handler. Re-record the baseline with `--record` once met.
- Golden files, dispatch tests and the package smoke consumer updated.

## Goal (implementer restatement)

Consumers call `ISender.Send<TResponse>` and the void `Send<TRequest>` where `TRequest : IRequest`. Each of those methods is a type switch whose arms return the `Dispatch_*` `ValueTask` with no box and no `SendCore`. `Send(object)` and `Publish(object)` stay the only paths that erase the message to `object`.

A response arm may reinterpret `ValueTask<TConcrete>` as `ValueTask<TResponse>` only when those types are the same. A covariant send, or a request that implements two `IRequest<>` closures, awaits the concrete task and casts the result. A mismatch throws `InvalidCastException`. A faulted or canceled `ValueTask` stays faulted until the caller awaits it. A null argument, a missing handler, or a handler that throws before returning a `ValueTask` still leaves `Send` as a thrown exception.

When a request has behaviors, `Dispatch_*` resolves one scoped chain object and invokes it. That object creates its `MessageHandlerDelegate<TRequest, TResponse>` steps once per scope and resolves each behavior and the handler from that scope. A behavior may await before it calls `next`, and may call `next` again after an await. Two scopes can send the same request at the same time. A send does not allocate a closure. With no behaviors, the dispatcher resolves one scoped `HandlerCache` and reuses it. Each handler slot stays empty until that request is sent, then `GetRequiredService` fills it once for the scope. A handler that cannot be constructed fails only its own send, and a request that was never sent does not construct its handler. A transient registration is still one instance per scope. Disposing the scope disposes the cache and the next send throws `ObjectDisposedException`. Nothing is stored in a thread-static slot. Per-send resolve was measured at about 24 ns, past 1.5× the martinothamar row, which is why the cache exists. The slot is a null check on the hot path, so the comparison baseline is re-recorded when that check lands.

`Publish<TNotification>` stays typed, does not cast the notification to `object`, and still awaits every assignable handler sequentially.

`AddGeneratedMediator` / `AddGeneratedMediator<TScope>` live on an internal extension class in the host's generated namespace, named from that namespace. They register the dispatcher (including `ISender` / `IPublisher` / `IMediator` and the scoped sender/publisher), the concrete handler and behavior types, a scoped chain type for each request that has behaviors, and a scoped `HandlerCache` when any request has none. They do not register `IRequestHandler<>`, `IPipelineBehavior<,>`, or `INotificationHandler<>`.

The generated `MediatorManifest` keeps its JSON and also exposes, per request, the message kind (`Query`, then `IdempotentCommand`, then `Command`, then `Action`, otherwise `Request`), the response type, the scope, and the ordered behavior list. Those lists are read-only. The host assembly is marked `[assembly: GeneratedMediator(typeof(global::….Mediator))]`.

The comparison benchmark must show interface Ping/Pong at most 1.5× the martinothamar mean and at most 24 B, void interface send at 0 B, and the State-shaped pipeline at 0 B. Re-record that baseline from a clean tree once those numbers hold. Dispatch tests cover the covariant, await-before-next, and faulted-task cases.

Acceptance criteria:

- Typed `Send` arms return `Dispatch_*` without boxing. `Send(object)` and `Publish(object)` remain the boxing paths.
- `Unsafe.As` runs only when `TResponse` is the handler's response type. The other arm awaits and casts.
- Behavior dispatch uses a scoped chain whose delegates are created once per scope, with no per-call closure and no thread-static scope.
- A request with no behaviors uses the scoped `HandlerCache` on the dispatcher instance. Each handler is resolved on the first send of its request and reused for that scope. An unresolvable handler does not fail other no-behavior sends, and an unsent request does not construct its handler. A disposed scope throws. The cache is not thread-static. A transient handler registration is resolved once per cache.
- A faulted or canceled `ValueTask` is not thrown synchronously from `AdaptUnit` or `AsVoid`.
- `Publish<TNotification>` is typed and still awaits handlers sequentially, including contravariant base handlers.
- Registration is concrete handler and behavior types, scoped chain types, a scoped `HandlerCache` for unchained requests, plus the dispatcher and scoped sender/publisher. The extension class is internal in the host generated namespace.
- `MediatorManifest` exposes kind, response type, and the ordered behaviors for each request's scope. `Requests` and `Behaviors` cannot be cast back to a mutable array.
- `[assembly: GeneratedMediator(typeof(global::….Mediator))]` is emitted.
- The 021-001 comparison rows meet the allocation and interface-time targets above, and `./bin/dev benchmark --project comparison --record` rewrites the baseline from a clean tree.
- `./bin/dev workflow` is green.

## Checklist

- [x] Non-boxing typed `Send` switch arms
- [x] Cached static delegate chain for behaviors
- [x] Typed `Publish`
- [x] Concrete-only registration, extension class in host namespace
- [x] `MediatorManifest` emitted and tested
- [x] `[GeneratedMediator]` emitted
- [x] Benchmark gate met and baseline re-recorded
- [x] `./bin/dev workflow` green
- [x] Covariant and dual-interface `Send` await and cast
- [x] Scoped behavior chain; await-before-next and concurrent scopes
- [x] No thread-static handler cache
- [x] Lazy no-behavior handler slots; an unresolvable handler does not block the others
- [x] Faulted `ValueTask` stays faulted; sync-throw note on 021-007
- [x] `global::` on the assembly attribute; read-only manifest
- [x] Baseline re-recorded from a clean tree
- [ ] PR with merge commit

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6, M7
- Round 2: Blocked — open M8
- Round 3: Done
## Notes

Parent: 021. Depends on: 021-002 (contracts). Review sections 3 (F2, F4, F7, F12, F13), 4
(generated dispatcher, registration), 9. Emitter today: `mediator-emitter.cs` lines 161,
228-256, 299-340 (14.0 numbering).

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- 2026-10-24: emitted the non-boxing dispatcher, cached behavior chain, concrete registration, manifest kind, and assembly attribute. `ganda repo audit --fix` inserted `charset = utf-8` under `[*]` in `.editorconfig`. `./bin/dev workflow` exited 0 (clean, build, test, samples, pack, package smoke).
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-23T19:30:05Z
- Review oracle round 1 (2026-10-24): claude (claude-opus-5-5), effort 3, roster general, session not reported. Findings M1–M7 open; fix pass requested.
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-23T19:34:45Z
- 2026-10-24: fix pass for review round 1. Covariant send awaits and casts, behavior chains are scoped, and the no-behavior path reuses a scoped HandlerCache after per-send resolve measured about 24 ns. `ganda repo audit` passed with no fixes.
- Review oracle round 2 (2026-10-24): claude (claude-opus-5-5), effort 3, roster general, session not reported. M1–M7 fixed, new M8 open; fix pass requested.
- 2026-10-24: fix pass for review round 2. Each no-behavior handler slot is filled on the first send of that request. `ganda repo audit` passed with no fixes. `./bin/dev workflow` exited 0. Comparison baseline re-recorded from clean commit `c5e4a3a`.
- Review oracle round 3 (2026-10-24): claude (claude-opus-5-5), effort 1, roster general, session not reported. M8 fixed, no new findings; disposition clean.

## Results

Typed `Send<TResponse>` reinterprets `ValueTask<TConcrete>` only when `TResponse` is that concrete type. A covariant send, and a request that implements two `IRequest<>` closures, awaits the handler result and casts it; a mismatch throws `InvalidCastException`. A faulted or canceled `ValueTask` stays faulted until awaited. A null argument, `NoHandlerException`, and a handler that throws before returning a `ValueTask` still leave `Send` as a thrown exception. That difference from 14.x is noted on 021-007.

Requests with behaviors resolve a scoped chain and invoke delegates created once for that scope. A behavior may await before `next` and may call `next` again. Requests with no behaviors reuse a scoped `HandlerCache` on the dispatcher instance. Each handler slot stays empty until that request is sent, then `GetRequiredService` fills it once for the scope. A handler that cannot be constructed fails only its own send. An unsent request does not construct its handler. A transient registration is still one instance per scope. Disposing the scope disposes that cache and the next send throws `ObjectDisposedException`. Nothing is stored in a thread-static slot. `Publish<TNotification>` stays typed. `MediatorManifest.Requests` and each entry's `Behaviors` are read-only wrappers. The assembly attribute is `[assembly: GeneratedMediator(typeof(global::….Mediator))]`.

Recorded comparison baseline from clean commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a` (`./bin/dev benchmark --project comparison --record`, BDN 0.15.8, .NET 10.0.13):

| Row | Mean | Allocated | Target |
| --- | --- | --- | --- |
| TimeWarpGeneratedInterfaceSend | 8.243 ns | 24 B | ≤ 1.5× MartinothamarGenerated (8.314 ns → 12.471 ns) and ≤ 24 B |
| TimeWarpGeneratedInterfaceSendUnit | 9.360 ns | 0 B | 0 B |
| TimeWarpGeneratedStatePipeline | 131.117 ns | 0 B | 0 B apart from the handler |
| MartinothamarGenerated | 8.314 ns | 24 B | reference |

### How to validate

Smoke: `./bin/dev workflow`

Expect: exit 0. Generator tests cover covariant send, a dual-interface mismatch, await-before-next, concurrent scopes, faulted and canceled `ValueTask`s, and a manifest that cannot be cast back to an array. Compilation tests cover the scoped handler cache, a disposed scope, a synchronous handler throw, and two no-behavior requests where one handler's dependency is unregistered: the other request still dispatches and only the sent handler is constructed. The comparison baseline names commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a` with no uncommitted-changes note, interface Ping/Pong at 8.243 ns / 24 B, void interface send at 0 B, and the State pipeline at 0 B.

Ran 2026-10-24: `./bin/dev workflow` (mode Pr) exited 0. Tests passed (generators 49, compilation 88, roslyn-floor 89, analyzers 42, mediator 191). Eleven samples verified. Package smoke printed `package-pong via TimeWarp.Mediator.Generated.Mediator`. Interface send 8.243 ns / 24 B, void send 0 B, State pipeline 131.117 ns / 0 B.

### Review

Round 1 (effort 3, roster general) left M1–M7 open. The first fix pass implements them.

Round 2 (effort 3, roster general) reviewed the fix-pass delta `231ed4e..91058b9`. M1–M7 are fixed. M8 was open: the `HandlerCache` constructor resolved every no-behavior handler on the first send in a scope. This fix pass fills each slot on the first send of that request. `UnresolvableHandler_DoesNotBlockTheOtherRequest` covers a graph of two no-behavior requests where one dependency is unregistered.

Round 3 (effort 1, roster general) reviewed the fix-pass delta `885e8d2..e69c39d`. It confirmed M8 is fixed, raised no new findings, and reran the compilation (88) and generator (49) tests, which passed.

Final counts: bug 3 fixed, suggestion 2 fixed, nit 3 fixed. Open 0, wontfix 0.

**Disposition: clean** (3 rounds, every raised finding fixed, no escalations).

Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/merged.md`, `review/round-3/general.md`, `review/round-3/merged.md` (final ledger), `review/disposition.md`.
