# Round 2 — general
**Date:** 2026-10-24
**Scope reviewed:** fix-pass delta `231ed4e..91058b9` (product commit 2a2a10d, baseline commit d768a68, kanban commit 91058b9). The emitter, manifest emitter, new dispatch tests, comparison baseline, and the 021-007 migration note.

## Summary

The fix pass addresses all seven round-1 findings. Typed response arms guard `Unsafe.As` with `typeof(TResponse) == typeof(TConcrete)` and otherwise await and cast through `AdaptResponse`. `Chain_*` is now a scoped instance whose delegates bind once per scope, so it uses no thread-static slot. The `IsCompleted` throw branch is gone from `AdaptUnit` / `AsVoid`, and the 021-007 note records the remaining synchronous-throw behavior. The manifest returns `ReadOnlyCollection` wrappers, and the assembly attribute carries `global::`. The baseline is re-recorded from clean commit 2a2a10d. Tests cover covariant and dual-interface sends, await-before-next, concurrent scopes, faulted and canceled tasks, the read-only manifest, the scoped cache, a disposed scope, and a synchronous throw. Generator tests (49) and compilation tests (87) pass locally.

The fix introduces one new defect. The new scoped `HandlerCache` resolves every no-behavior handler in the dispatcher's graph eagerly in its constructor.

## Prior findings re-verified

| ID | Status | Evidence |
|----|--------|----------|
| M1 | fixed | `mediator-emitter.cs:335-345`: `Unsafe.As` sits under `if (typeof(TResponse) == typeof(<concrete>))`. Otherwise the arm calls `AdaptResponse<TConcrete, TResponse>`, which awaits and casts or throws `InvalidCastException`. Covered by `AsyncCovariantSend_ReturnsTheHandlerResult` (async `SlowEcho`) and `DualInterfaceSend_ThrowsInvalidCastExceptionForTheUnboundResponse`. |
| M2 | fixed | `EmitChain`: `internal sealed class Chain_*` has a readonly `Scope` field and instance `Next*` delegates assigned in its constructor. It is registered `AddScoped` and resolved per send in `Dispatch_*`. No `[ThreadStatic]` is emitted (asserted in `NoBehaviorDispatch_DoesNotCacheOnTheThread`). `Behavior_CanAwaitBeforeNext_AndCallNextAgain` and `ConcurrentSends_KeepEachScope` cover the contract. |
| M3 | fixed | The thread-static `CachedScope_*` / `CachedHandler_*` are removed. The replacement is a scoped `HandlerCache` held on the dispatcher instance. It is `IDisposable`, so disposing the scope makes the next send throw `ObjectDisposedException` (`ScopedHandler_StaysInsideTheScope_AndDisposedScopeThrows`). The cache now overrides a transient handler registration within a scope. The goal restatement records that. New defect M8 below. |
| M4 | fixed | The `IsCompleted` branches are removed from `AdaptUnit` / `AsVoid` (`mediator-emitter.cs:820-846`). `FaultedValueTask_StaysFaultedUntilAwaited` covers this, and the 021-007 task now carries the sync-throw migration note. |
| M5 | fixed | `mediator-emitter.cs:139` emits `typeof(global::<ns>.Mediator)`. |
| M6 | fixed | `manifest-emitter.cs`: `Requests` returns a `ReadOnlyCollection<RequestEntry>`, and `Behaviors` is `IReadOnlyList<string>` over a `ReadOnlyCollection`. Covered by `Manifest_RequestsAndBehaviorsCannotBeCastToArrays`. |
| M7 | fixed | `documentation/benchmarks/comparison-baseline.md` names commit `2a2a10d…` with no uncommitted-changes note. The interface send row is 8.855 ns / 24 B (≤ 1.5 × 8.657 ns), the void interface send row is 0 B, and the State pipeline row is 0 B. |

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-mediator-generators/mediator-emitter.cs:514
- Description: `HandlerCache`'s constructor calls `GetRequiredService` for **every** handler of every no-behavior request in that dispatcher (`foreach (INamedTypeSymbol handler in handlers)`). That code runs on the first `Send` in each scope. Before this pass, on master and in round 1, a send resolved only the handler for its own request. The change has three consequences:
  1. **Failure coupling.** If any one no-behavior handler cannot be constructed, every no-behavior `Send` in that scope throws, including sends for unrelated requests. A handler cannot be constructed when its dependency is not registered in this host, is registered conditionally, or has a constructor that throws.
  2. **Per-scope cost grows with the size of the graph.** An ASP.NET request scope that sends one query builds every no-behavior handler and its dependency graph (DbContexts, clients, and so on). The comparison benchmark has one handler per dispatcher, so the baseline does not show this cost.
  3. **Side effects.** Handler constructors run for requests that were never sent.

  No test covers a graph with more than one no-behavior handler, or with a handler that cannot be resolved.
- Suggestion: Resolve each handler lazily, once per cache. For example, give each handler a nullable field and an accessor `X ??= GetRequiredService<X>(Scope)`, and keep the scope on the cache. That keeps the scoped lifetime, the disposal check, the absence of a thread-static slot, and the "a transient handler is resolved once per cache" behavior, and it adds only a null check to the hot path. Add a compilation test with two no-behavior requests in which one handler's dependency is unregistered. The other request must still dispatch, and only the sent handler may be constructed. Re-run `./bin/dev benchmark --project comparison --record` if the hot path changes.
- Status: open

## Notes (not findings)

- The task requirement says "cached **static** `MessageHandlerDelegate` instances". The fix pass moved to per-scope instance delegates to resolve round-1 M2, and the goal restatement was updated to match. Because the review drove this deviation, it is not a misread. The no-closure-per-call property holds: the State pipeline row allocates 0 B.
- The Host-profile dispatcher is registered transient. A dispatcher resolved from the root provider resolves `HandlerCache` and `Chain_*` (both scoped) from the root. That matches the existing scoped-handler registration, so this pass does not change it.
