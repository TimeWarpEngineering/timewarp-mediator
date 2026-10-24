# Round 2 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 2 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 3 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:335
- Description: `Unsafe.As` across different `ValueTask` result types (round 1).
- Disposition notes: Guarded by `typeof(TResponse) == typeof(TConcrete)`. Otherwise the arm runs `AdaptResponse`, which awaits and casts. The async covariant and dual-interface tests pass.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:727
- Description: A thread-static chain scope was lost across an await (round 1).
- Disposition notes: The scoped `Chain_*` instance creates its delegates once per scope. The await-before-next and concurrent-scope tests pass.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:472
- Description: A thread-static handler cache kept disposed scopes alive (round 1).
- Disposition notes: Replaced by a scoped, disposable `HandlerCache` on the dispatcher instance. The disposed-scope test passes. The replacement's eager resolution is tracked as M8.

### M4 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:820
- Description: `IsCompleted` turned a faulted `ValueTask` into a synchronous throw (round 1).
- Disposition notes: The branch is removed. The faulted and canceled test passes. The sync-throw note is on 021-007.

### M5 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:139
- Description: The assembly attribute lacked `global::` (round 1).
- Disposition notes: It now emits `typeof(global::<ns>.Mediator)`.

### M6 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-generators/manifest-emitter.cs:184
- Description: The manifest exposed mutable arrays (round 1).
- Disposition notes: `Requests` and `Behaviors` are now `ReadOnlyCollection` wrappers. A test asserts that neither can be cast back to an array.

### M7 — Severity: nit — Status: fixed
- File: documentation/benchmarks/comparison-baseline.md:6
- Description: The baseline was recorded from a dirty tree (round 1).
- Disposition notes: Re-recorded from clean commit 2a2a10d. The gate rows meet the targets.

### M8 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:514
- Description: `HandlerCache`'s constructor eagerly resolves every no-behavior handler in the dispatcher graph on the first `Send` of each scope. If any one handler cannot be resolved (for example, an unregistered dependency or a constructor that throws), every no-behavior `Send` in that scope fails. Per-scope cost grows with the graph size, and handlers for requests that were never sent are constructed. Master and round 1 resolved only the handler of the sent request.
- Suggestion: Make each handler slot lazy (`X ??= GetRequiredService<X>(Scope)`) while keeping the scoped, disposable, non-thread-static cache. Add a compilation test with two no-behavior requests, one with an unresolvable handler, and assert that the other still dispatches and that only the sent handler is constructed. Re-record the comparison baseline if the hot path changes.
- Source: general (round 2, Issue 1)
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. M8 is a regression introduced by the M3 fix and is tracked separately so M3 can close.
