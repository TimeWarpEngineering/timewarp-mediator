# Round 2 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 3 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/benchmarks/comparison-baseline.md, tests/timewarp-mediator-benchmarks-comparison/dispatch-benchmarks.cs
- Description: The gate re-record and the martinothamar lifetime fix were uncommitted and disagreed. The baseline was also noisy.
- Source: general (round 1)
- Disposition notes: Lifetime fix committed in a5129e1. Re-recorded from a5129e1 on a clean tree in 3c5c08c. `--gate` passed. InterfaceSend 8.480 ns ±0.261.

### M2 — Severity: bug — Status: fixed
- File: documentation/benchmarks/comparison-baseline-aot.md
- Description: The required AOT record was missing.
- Source: general (round 1)
- Disposition notes: Committed at 3c5c08c. Round 2 reproduced its allocations independently.

### M3 — Severity: bug — Status: fixed
- File: kanban/in-progress/020-improve-mediator-send-performance/task.md
- Description: There was no `## Results`, no singleton numbers, no recommendation, and no gate, workflow, or audit record.
- Source: general (round 1)
- Disposition notes: All of these are now in `## Results`. `architecture.md` and the emitter Design comment cite the paired numbers.

### M4 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-benchmarks-comparison/aot-benchmark-runner.cs
- Description: The boxing sink inflated AOT allocations, and the delegate overhead was undocumented.
- Source: general (round 1)
- Disposition notes: The runner now uses a generic static `Consumed<T>` sink, and the overhead is stated in the headers. MonomorphicSendUnit is 0 B and ArchitectureSend is 24 B.

### M5 — Severity: nit — Status: fixed
- File: tools/dev-cli/endpoints/benchmark-command.cs
- Description: The exit-code ternary could never pick 1.
- Source: general (round 1)
- Disposition notes: The exit code is assigned directly at both sites.

## New findings

None.

## Duplicates / conflicts

- None. One reviewer.
