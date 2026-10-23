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

Make the generated dispatcher the path consumers actually call. `ISender.Send<TResponse>` and the void `Send<TRequest>` where `TRequest : IRequest` each emit a type switch whose arms return the `Dispatch_*` `ValueTask` with no box and no `SendCore`. `Send(object)` and `Publish(object)` stay the only paths that erase the message to `object`.

When a request has behaviors, `Dispatch_*` resolves the handler and the closed behaviors in compile-time order (outermost first) and runs a chain of cached static `MessageHandlerDelegate<TRequest, TResponse>` fields. A send with behaviors does not allocate a closure. `Publish<TNotification>` stays typed, does not cast the notification to `object`, and still awaits every assignable handler sequentially.

`AddGeneratedMediator` / `AddGeneratedMediator<TScope>` live on an internal extension class in the host's generated namespace, named from that namespace. They register the dispatcher (including `ISender` / `IPublisher` / `IMediator` and the scoped sender/publisher), plus concrete handler and behavior types. They do not register `IRequestHandler<>`, `IPipelineBehavior<,>`, or `INotificationHandler<>`.

The generated `MediatorManifest` keeps its JSON and also exposes, per request, the message kind (`Query`, then `IdempotentCommand`, then `Command`, then `Action`, otherwise `Request`), the response type, the scope, and the ordered behavior list. The host assembly is marked `[assembly: GeneratedMediator(typeof(Mediator))]`.

The comparison benchmark must show interface Ping/Pong at most 1.5× the martinothamar mean and at most 24 B, void interface send at 0 B, and the State-shaped pipeline at 0 B. Re-record that baseline once those numbers hold. Golden files, dispatch tests, and the package smoke consumer follow the new emission.

Acceptance criteria:

- Typed `Send` arms return `Dispatch_*` without boxing. `Send(object)` and `Publish(object)` remain the boxing paths.
- Behavior dispatch uses cached static `MessageHandlerDelegate<TRequest, TResponse>` instances, with no per-call closure.
- `Publish<TNotification>` is typed and still awaits handlers sequentially, including contravariant base handlers.
- Registration is concrete handler and behavior types plus the dispatcher and scoped sender/publisher. The extension class is internal in the host generated namespace.
- `MediatorManifest` exposes kind, response type, and the ordered behaviors for each request's scope, and tests cover that.
- `[assembly: GeneratedMediator(typeof(Mediator))]` is emitted.
- The 021-001 comparison rows meet the allocation and interface-time targets above, and `./bin/dev benchmark --project comparison --record` rewrites the baseline.
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
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-002 (contracts). Review sections 3 (F2, F4, F7, F12, F13), 4
(generated dispatcher, registration), 9. Emitter today: `mediator-emitter.cs` lines 161,
228-256, 299-340 (14.0 numbering).

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- 2026-10-24: emitted the non-boxing dispatcher, cached behavior chain, concrete registration, manifest kind, and assembly attribute. `ganda repo audit --fix` inserted `charset = utf-8` under `[*]` in `.editorconfig`. `./bin/dev workflow` exited 0 (clean, build, test, samples, pack, package smoke).
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-23T19:30:05Z
- Review oracle round 1 (2026-10-24): claude (claude-opus-5-5), effort 3, roster general, session not reported. Findings M1–M7 open; fix pass requested.

## Results

Typed `Send<TResponse>` and void `Send<TRequest>` arms return `Dispatch_*` through `Unsafe.As` / `AsVoid`. `Send(object)` still boxes. Behaviors run a static `MessageHandlerDelegate` chain with a thread-static scope. No-behavior host dispatch caches the handler per scope. `Publish<TNotification>` stays typed and still awaits every assignable handler. `AddGeneratedMediator` lives in the host generated namespace and registers concrete handlers and behaviors only. `MediatorManifest` entries carry kind, response, scope, and ordered behaviors. `[assembly: GeneratedMediator(typeof(Mediator))]` is emitted.

Recorded comparison baseline (`./bin/dev benchmark --project comparison --record`, BDN 0.15.8, .NET 10.0.13):

| Row | Mean | Allocated | Target |
| --- | --- | --- | --- |
| TimeWarpGeneratedInterfaceSend | 9.731 ns | 24 B | ≤ 1.5× MartinothamarGenerated (8.670 ns → 13.005 ns) and ≤ 24 B |
| TimeWarpGeneratedInterfaceSendUnit | 11.765 ns | 0 B | 0 B |
| TimeWarpGeneratedStatePipeline | 141.016 ns | 0 B | 0 B apart from the handler |
| MartinothamarGenerated | 8.670 ns | 24 B | reference |

### How to validate

Smoke: `./bin/dev workflow`

Expect: exit 0. The generator compilation tests and generator runtime tests pass, including concrete-only registration, manifest kinds, the assembly attribute, and contravariant publish. The comparison baseline in `documentation/benchmarks/comparison-baseline.md` shows interface Ping/Pong at 9.731 ns / 24 B, void interface send at 0 B, and the State pipeline at 0 B.

Ran 2026-10-24: exit 0. Tests passed (generators 43, compilation 83, roslyn-floor 84, analyzers 42, mediator 191). Eleven samples verified. Package smoke printed `package-pong via TimeWarp.Mediator.Generated.Mediator`.

### Review

Round 1 (effort 3, roster general): 2 bug, 2 suggestion, 3 nit, all open. Disposition pending; a fix pass is needed.

- M1 bug: the `Unsafe.As` covariant `Send` throws `EntryPointNotFoundException` when the handler completes asynchronously.
- M2 bug: the thread-static chain scope breaks a behavior that awaits before `next`.
- M3–M7: handler-cache retention, synchronous throws, `global::` on the attribute, mutable manifest arrays, baseline provenance.

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md` (live ledger).
