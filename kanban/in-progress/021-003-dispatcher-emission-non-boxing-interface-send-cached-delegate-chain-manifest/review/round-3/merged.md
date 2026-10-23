# Round 3 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 3 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 3 | 0 |

## Issues

### M1–M7 — Status: fixed (rounds 1–2)
- Carried from `round-2/merged.md`. This delta does not regress them: the compilation and generator suites pass.

### M8 — Severity: bug — Status: fixed
- File: source/timewarp-mediator-generators/mediator-emitter.cs:973
- Description: `HandlerCache` eagerly resolved every no-behavior handler on the first send in a scope (round 2).
- Source: general (round 2)
- Disposition notes: Each slot is `field ??= GetRequiredService<X>(Scope)` and the constructor only stores the scope. `UnresolvableHandler_DoesNotBlockTheOtherRequest` shows the other request still dispatches and the unsent handler is not constructed. Baseline re-recorded from clean commit c5e4a3a; the gate rows meet the targets.

## Duplicates / conflicts

- None. No new findings this round.
