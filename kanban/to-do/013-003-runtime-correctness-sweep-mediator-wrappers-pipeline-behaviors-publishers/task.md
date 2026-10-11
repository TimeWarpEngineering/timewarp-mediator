# Runtime correctness sweep: Mediator, wrappers, pipeline behaviors, publishers

## Description

Small, verified defects in the shipped reflection runtime. Parent review task 013, findings C-8, C-9, C-10, C-11, C-12, C-13, C-15, C-16, C-17, C-22, C-26, C-27, C-30.

- C-9 (bug): `mediator.cs` `Publish(object)` message has a stray `$` ("does not implement $INotification") and no `paramName`.
- C-22 (risk): `type-forwardings.cs` forwards every moved contract except `IStreamRequest<>`.
- C-13 (risk): `RequestExceptionProcessorBehavior` rethrows when `Handled` is true but `Response` is null, so a handler that legitimately returns null sees its exception rethrown.
- C-15 (risk): `TaskWhenAllPublisher` invokes handlers synchronously inside `Select(...).ToArray()`; a synchronous throw skips remaining handlers and leaves earlier tasks unobserved. Its XML sample does not match the code.
- C-12 (risk): the four `AddOpenBehavior`-family methods check `IsGenericType` instead of `IsGenericTypeDefinition`, so a closed generic passes and fails later in the container.
- C-8 (risk): wrapper caches are keyed by request type only; `Send(object)` then `Send<object>` on the same request type can `InvalidCastException`. Fix or document.
- C-10, C-11 (nit): missing null guards in `Mediator` constructor, `AddMediator`, configuration methods; `open-behavior.cs` passes a sentence as `paramName`.
- C-16, C-17 (nit): stream handler resolved lazily on first `MoveNextAsync`; one missing `ConfigureAwait(false)`.
- C-26, C-27, C-30 (nit): stale "dynamic dispatch" comment; `Compare(null, null)` returns 1 in `object-details.cs`; `RegisterBehaviorIfImplementationsExist` only sees registrations present at `AddMediator` time (document it).

## Checklist

- [ ] Fix C-9 message and paramName
- [ ] Add `IStreamRequest<>` type forward (C-22) and a test that enumerates public contract types against the forwarding list
- [ ] Decide and fix C-13 null-Handled semantics
- [ ] Make `TaskWhenAllPublisher` convert synchronous throws to faulted tasks; fix its doc sample (C-15)
- [ ] `IsGenericTypeDefinition` in open-behavior registration; extract the four duplicate blocks (C-12)
- [ ] Fix or document C-8 cache key
- [ ] Null guards and nits (C-10, C-11, C-16, C-17, C-26, C-27, C-30)
- [ ] Tests for each behavior change; `./bin/dev workflow` green; PR with merge commit

## Notes

Parent: 013. No public API additions. Keep each fix a separate commit for reviewability.

## Session

- Created: 686431 (2026-10-11)
