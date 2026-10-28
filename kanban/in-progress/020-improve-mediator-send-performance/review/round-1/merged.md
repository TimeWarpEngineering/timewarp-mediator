# Round 1 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 3 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: documentation/benchmarks/comparison-baseline.md, tests/timewarp-mediator-benchmarks-comparison/dispatch-benchmarks.cs:66 (both uncommitted)
- Description: The gate re-record and the martinothamar Singleton fix are uncommitted, and they disagree. The baseline was recorded with martinothamar forced to Transient (42.790 ns / 112 B), and the code was edited afterwards. The record is also noisy: InterfaceSend went from 8.243 to 10.116 ns with ±2.31 ns error, which raises the bar the gate compares against. Nothing shows `--gate` ran green.
- Suggestion: Commit the lifetime fix. Re-record on a quiet machine from the clean committed tree, run `./bin/dev benchmark --gate`, commit, and record the output in `## Results`.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: documentation/benchmarks/comparison-baseline-aot.md (missing)
- Description: This is a required deliverable and it was never produced. The docs and the baseline suffix already link to it.
- Suggestion: Run `./bin/dev benchmark --aot` after M4 and commit the output.
- Source: general
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: kanban/in-progress/020-improve-mediator-send-performance/task.md
- Description: There is no `## Results`. The singleton and reused-transient numbers and the recommendation to Steven are not recorded anywhere, even though `architecture.md` claims a measurement. The gate result, the CI decision, `dev workflow` on both TFMs, and the audit result are missing. The checklist is unticked.
- Suggestion: Measure the singleton row again or recover the numbers. Write `## Results` with those numbers and the recommendation, cite them from `architecture.md`, run and record `./bin/dev workflow`, and tick the checklist.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: tests/timewarp-mediator-benchmarks-comparison/aot-benchmark-runner.cs:186-194
- Description: The AOT runner boxes value-type results into `object? Consumed` and calls each row through an `Action`. This overstates allocations: MonomorphicSendUnit shows 24 B in AOT and 0 B under BDN, and ArchitectureSend shows 56 B in AOT and 24 B under BDN. The timings include delegate overhead, and the file does not say so.
- Suggestion: Use a generic non-boxing sink, and state the runner overhead in the header.
- Source: general
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: tools/dev-cli/endpoints/benchmark-command.cs:210, :229
- Description: The ternary `X.ExitCode == 0 ? 1 : X.ExitCode` is inside `if (X.ExitCode != 0)`, so it can never pick 1.
- Suggestion: Assign the exit code directly.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One reviewer, so nothing to collapse. The noisy-baseline concern is part of M1 because the same re-record fixes both.
