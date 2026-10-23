# Round 2 — general
**Date:** 2026-10-24
**Scope reviewed:** fix delta since round 1 (`37966ba..a6da9a4`):
`tests/timewarp-mediator-tests/benchmark-gate-tests.cs`, `task.md`, `review/round-1/merged.md`.

## Summary

The fix pass rewrites `Parse_should_read_the_recorded_comparison_baseline` to assert structure
only: a clean parse, the five rows this task added are present in
`documentation/benchmarks/comparison-baseline.md` and are gated by `BenchmarkGate.IsGated`, and
the file's tolerance sentence matches `BenchmarkGate.MeanTolerancePercent`. The `Rows.Count == 13`
pin and the allocated-byte pins are gone, so a later `--record` that changes the numbers does not
fail the suite. This resolves M1 as suggested.

Verified against the repo: the five row names exist in the baseline file (lines 31–35);
`BenchmarkTable.Rows` is a `Dictionary<string, BenchmarkRow>` and `IsGated` is reachable from the
test project; `dotnet test ... --filter FullyQualifiedName~BenchmarkGateTests` passed 8 of 8 on
this tree. The Design region note describes the new test accurately. The dash-as-zero case remains
covered by its own fixture test.

The revised `## Goal (implementer restatement)` matches the task's Description and Requirements
(interface, scoped, publish, State and Architecture rows; `--record`; `--gate` with documented
tolerance; 15.0 targets as an unenforced table). Its new paragraph about surviving a re-record is
the M1 fix, not a scope change. No misread.

## Issues

None. M1 (round 1) re-verified as fixed.
