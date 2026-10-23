# Round 2 — merged findings
**Date:** 2026-10-23
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 1 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-generators-tests/replacement-request-tests.cs, tests/timewarp-mediator-tests/pipeline-tests.cs:525-574
- Description: No test called `next` with a request other than the one the behavior received.
- Source: general (round 1)
- Disposition notes: Fixed in 20a559e. Both dispatchers have a single-behavior test and a
  two-behavior chain test. In each, the handler (and the inner behavior) must receive the
  replacement instance. Round 2 ran the tests and they pass.

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-mediator-contracts/unit.cs:17-23
- Description: The XML comment implied `using static TimeWarp.Mediator.Unit` was safe, but it shadows `ValueTask`.
- Source: general (round 1)
- Disposition notes: Fixed in 20a559e. The comment was reworded and a matching note was added to 021-007.

### M3 — Severity: nit — Status: wontfix
- File: documentation/diagnostics.md:88-90
- Description: Stale TWM009 help text and stale docs pages that describe deleted types.
- Source: general (round 1)
- Disposition notes: Deferred to 021-007, which owns the docs rewrite. Decided by the review
  oracle in round 1 based on task scope.

## Duplicates / conflicts

- None. Single reviewer. The delta raised no new findings.
