# Round 4 — general
**Date:** 2026-10-28
**Scope reviewed:** `4563d15..1862f53`: `Directory.Packages.props`, `agent.md`, and the task.md fix-pass record

## Summary

The fix pass for M3 pins BenchmarkDotNet per framework in `Directory.Packages.props`: `0.15.8` when `$(TargetFramework)` is `net10.0`, and `0.16.0-preview.2` when it is `net11.0`. This uses the same conditioned `PackageVersion` pattern as the DependencyInjection pins at lines 40–41. Only the comparison benchmark project references BenchmarkDotNet, and it targets exactly those two frameworks, so no inner build is left without a version. `dotnet list package` on that project resolves `0.15.8` on net10.0 and `0.16.0-preview.2` on net11.0. The props comment and the `agent.md` benchmark row describe both pins. task.md Results record a green `--gate` on `0.15.8` (summary prints `BenchmarkDotNet v0.15.8` and `.NET 10.0.14`, exit 0). `comparison-baseline.md` was not rewritten. Commit `d544794` is on `origin/master` and adds the gate checklist item to task 023-001. M1 and M2 are not touched by this delta and stay fixed. M3 is fixed.

Observation, not a finding: the nuget Dependabot group may propose moving the net10.0 entry off `0.15.8` once `0.16.0` is stable. Task 023-001 already says the net10.0 pin stays `0.15.8` unless Steven approves a re-record, so a person reviewing that PR has the rule.

## Issues

None.
