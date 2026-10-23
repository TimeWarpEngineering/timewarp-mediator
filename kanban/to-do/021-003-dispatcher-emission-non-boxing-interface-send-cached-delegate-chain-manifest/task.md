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

## Checklist

- [ ] Non-boxing typed `Send` switch arms
- [ ] Cached static delegate chain for behaviors
- [ ] Typed `Publish`
- [ ] Concrete-only registration, extension class in host namespace
- [ ] `MediatorManifest` emitted and tested
- [ ] `[GeneratedMediator]` emitted
- [ ] Benchmark gate met and baseline re-recorded
- [ ] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-002 (contracts). Review sections 3 (F2, F4, F7, F12, F13), 4
(generated dispatcher, registration), 9. Emitter today: `mediator-emitter.cs` lines 161,
228-256, 299-340 (14.0 numbering).

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
