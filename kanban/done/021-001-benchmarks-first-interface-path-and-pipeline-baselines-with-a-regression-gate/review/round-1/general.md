# Round 1 — general
**Date:** 2026-10-24
**Scope reviewed:** branch vs `master` (175104e, 280a842, 513b137), all 15 files

## Summary

The branch adds five comparison rows (scoped send, unscoped and scoped publish, State pipeline,
Architecture send), a `--gate` mode on `dev benchmark` backed by `BenchmarkGate` (parser plus
compare), a re-recorded `comparison-baseline.md` with a generated tolerance section and the
unenforced 15.0 target table, and eight parser and compare unit tests. Risk is low: nothing in
`source/` changes.

Verified against the repo:

- Generator output (built with `EmitCompilerGeneratedFiles` into `/tmp`) shows the profile is
  Host. `Increment` gets `RenderBehavior`, `ExceptionBehavior`, and `StateTransactionBehavior`
  on `Sender_ClientPipeline`. `UpdateProfile` gets `ArchitectureValidationBehavior` on the
  unscoped `Mediator`. `ScopedPing` and `Ping` have no behaviors. So each row measures the
  shape it claims to measure.
- `dev benchmark` deletes `artifacts/benchmarks/<project>` before every run, so the gate cannot
  read a stale report. `NA` means, missing rows, new `TimeWarp*` rows, duplicates, and a table
  with no `Allocated` column all fail the gate loudly.
- `BenchmarkGateTests` passed 8 of 8. `./bin/dev build` and `./bin/dev test --no-build` passed.
- The gate suffix in the baseline file matches `BenchmarkGate.BaselineSuffix()`. The commit
  header names 280a842. 513b137 changed the tolerance after that record and hand-aligned the
  sentence, which Results disclose. 513b137 did not touch the benchmark project, so the
  measurement table still describes the code on HEAD. I am not raising this as a finding.
- The 50% mean tolerance is wide, but the file documents it and justifies it with the
  recorded error bars, as the task asks. Allocated bytes have zero tolerance, which is the
  check that matters for 021-003.

Goal restatement vs task: no misread. The restatement covers every requirement. It notes that
the tree already carries 021-002's ValueTask contracts, so "14.0 baseline" means the
package version and not the 2026-10-13 Task-shaped numbers. Results say the same.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-mediator-tests/benchmark-gate-tests.cs:134
- Description: `Parse_should_read_the_recorded_comparison_baseline` pins measured values from
  the stored baseline: `Rows.Count.ShouldBe(13)` and `TimeWarpGeneratedStatePipeline`
  `AllocatedBytes.ShouldBe(272)` (lines 134–137). The documented re-record flow (baseline
  gate section, `documentation/m1-benchmark-gap.md`, agent.md) says to run
  `./bin/dev benchmark --project comparison --record` after an intentional change. When 021-003
  does that, the State row should drop toward its 0 B target, and any added or removed row
  changes the count. `./bin/dev test` and `workflow` then fail on a unit test, and nothing in
  the re-record instructions mentions the test. The test ties a measurement to a parser test,
  and it breaks the exact next step this task exists to enable.
- Suggestion: Assert structural facts about the stored file, not measured values. It parses
  with no errors. It has at least one gated row. Every `[Benchmark]` method on
  `DispatchBenchmarks` has a row, or at least the rows this task added are present. The `at
  most {MeanTolerancePercent}%` sentence matches. The dash-as-zero case is already covered by
  the `Parse_should_treat_a_dash_allocated_cell_as_zero_bytes` fixture. If the 272 B pin is
  meant as a tripwire, the alternative is to say in the gate section that a re-record also
  updates this test.
- Status: open
