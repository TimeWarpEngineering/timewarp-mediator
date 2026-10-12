# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** git diff 0e1e5f8...HEAD -- source tests (013-003 runtime correctness sweep)

## Summary

The reflection-runtime fixes match the parent findings and do not add public API. Typed request and stream wrapper caches are keyed by `(request type, response type)`, with `typeof(void)` for the non-generic `IRequest` path, and `Send(object)` / `CreateStream(object)` use separate caches so an untyped dispatch cannot poison a later typed one. Null guards, `IsGenericTypeDefinition` checks on the four open-registration methods, returning a handled null response, converting synchronous `TaskWhenAllPublisher` throws into faulted tasks before `Task.WhenAll`, resolving stream handlers and behaviors in a non-iterator method (with `ConfigureAwait(false)` on the void-handler await and the object stream enumeration), the `IStreamRequest<>` type forward, `ObjectDetails.Compare(null, null) == 0`, and the `AddMediator`-time note on `RegisterBehaviorIfImplementationsExist` all hold in the implementation. The new tests lock those behaviors, including the type-forward diff (no nested contract types for the `IsNested` filter to hide), and the 22 targeted tests passed.
