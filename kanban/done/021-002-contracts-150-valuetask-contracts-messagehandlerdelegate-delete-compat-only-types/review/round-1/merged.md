# Round 1 — merged findings
**Date:** 2026-10-23
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 1 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-generators/mediator-emitter.cs:511 (also source/timewarp-mediator/wrappers/request-handler-wrapper.cs:796-806)
- Description: No test calls `next` with a request other than the one the behavior received.
  Both dispatchers could regress to forwarding the captured outer `request`, as the old emitter
  did, and every test would still pass. Carrying the request is the reason
  `MessageHandlerDelegate<TRequest, TResponse>` exists.
- Suggestion: Add a test per dispatcher in which a behavior calls `next(replacement, cancellationToken)`
  and the handler asserts it received `replacement`. Generated side:
  `tests/timewarp-mediator-generators-tests` with an `[assembly: MediatorBehavior]` behavior, plus
  a two-behavior chain so the nested lambda emission is covered. Reflection side:
  `tests/timewarp-mediator-tests/pipeline-tests.cs` with `AddBehavior`.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: source/timewarp-mediator-contracts/unit.cs:18-21
- Description: The XML comment implies `using static TimeWarp.Mediator.Unit` is now safe. The
  property named `ValueTask` collides with `System.Threading.Tasks.ValueTask` under that import,
  which is the same problem Nuru hit with `Task` (parent research `timewarp-nuru-usage.md:68,111`).
  The name `Unit.ValueTask` is required by the task, so renaming it is not asked for.
- Suggestion: Reword the comment so it says it no longer hides `Task` but does shadow `ValueTask`
  under `using static`, and recommend qualified `Unit.Value`/`Unit.ValueTask`. Add a line to
  021-007's notes so the migration page says the same.
- Source: general
- Disposition notes:

### M3 — Severity: nit — Status: wontfix
- File: documentation/diagnostics.md:88-90
- Description: The TWM009 help text still lists stream handler kinds and `CreateStream`.
  `generated-vs-legacy.md`, `m1-generated-mediator.md` and `m2-named-pipelines.md` still
  describe the deleted `MediatorAssemblies` and `MediatorModule` attributes and handler
  interfaces.
- Suggestion: Fix in 021-007.
- Source: general
- Disposition notes: wontfix on 021-002, deferred to 021-007. That task's Requirements own the
  architecture page (contracts, membership attributes, diagnostics) that replaces
  `generated-vs-legacy.md` and the profile docs, plus the 15.0 migration page and package
  readmes. This task's docs requirement is limited to `migration.md` and XML docs, and both
  are done. Decided by the review oracle (round 1) based on task scope.

## Duplicates / conflicts

- None. Single reviewer.
