# Round 1 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 2 | 0 | 0 |
| nit | 3 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:319
- Description: `Unsafe.As<ValueTask<TConcrete>, ValueTask<TResponse>>` in the typed `Send<TResponse>` arms. A covariant send (`IRequest<object>` for an `IRequest<string>` request) throws `EntryPointNotFoundException` when the handler completes asynchronously; a probe reproduced this. A request that implements two `IRequest<>` closures gets type confusion.
- Suggestion: Guard with `typeof(TResponse) == typeof(TConcrete)`, fall back to an await-and-cast adapter, add async covariant dispatch tests, and correct the Design region.
- Source: general (Issue 1)
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:712
- Description: The `Chain_*` `[ThreadStatic] Scope` is lost (`ArgumentNullException`), or belongs to another call (wrong-scope resolution), when a behavior awaits before it calls `next` or calls `next` again after an await. The `IPipelineBehavior` contract does not forbid that, and no test covers it.
- Suggestion: Make the chain a scoped object whose instance delegates are created once per scope; alternatively, record an operator decision that restricts the contract and fails fast. Add await-before-next and concurrent-scope tests.
- Source: general (Issue 2)
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:771
- Description: The unrequested thread-static handler cache keeps disposed scopes and their graphs alive on each thread. It returns stale handlers for disposed scopes and overrides transient registrations.
- Suggestion: Remove it and re-measure, or scope it to the DI scope.
- Source: general (Issue 3)
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:829
- Description: The `IsCompleted` branch in `AdaptUnit` / `AsVoid` turns a faulted or canceled `ValueTask` into a synchronous throw. Non-async typed `Send` now throws synchronously, where 14.x returned a faulted task.
- Suggestion: Drop the branch. Decide on the synchronous-throw semantics and record them for 021-007.
- Source: general (Issue 4)
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:136
- Description: The assembly attribute `typeof(<ns>.Mediator)` lacks the `global::` prefix.
- Suggestion: Emit `typeof(global::<ns>.Mediator)`.
- Source: general (Issue 5)
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/manifest-emitter.cs:214
- Description: The manifest `Behaviors` / `Requests` expose mutable static arrays.
- Suggestion: Use `IReadOnlyList<string>` or read-only wrappers.
- Source: general (Issue 6)
- Disposition notes:

### M7 — Severity: nit — Status: open
- File: documentation/benchmarks/comparison-baseline.md:6
- Description: The baseline was recorded from a dirty tree and names the base commit 1c7f2b0, not 902e5d5.
- Suggestion: Re-record from a clean tree after the fix pass.
- Source: general (Issue 7)
- Disposition notes:

## Duplicates / conflicts

- Single reviewer; no duplicates. M3 and M2 share a remedy (a scope-bound chain or cache object), so one fix may close both.
