# Round 2 — merged findings
**Date:** 2026-10-24
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Resolved prior

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-tests/benchmark-gate-tests.cs:134
- Description: the recorded-baseline parser test pinned the row count and measured allocated
  bytes, so a documented `--record` would fail the suite.
- Source: general (round 1)
- Disposition notes: fixed in a6da9a4. Re-verified in round 2: the test asserts a clean parse,
  presence and gating of the five added rows, and the tolerance sentence; no count or byte pins.
  BenchmarkGateTests passed 8 of 8.

## Issues

No new findings on the fix delta.

## Duplicates / conflicts

- None (single reviewer).
