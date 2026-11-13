# Round 2 — general
**Date:** 2026-11-14
**Scope reviewed:** delta since round 1 (`git diff 94ad4022971c4c19df9633fc6f2253aee8fb3921..HEAD`, commits `9815fa7`, `a0abd36`, `6351b9c`, and `fc1fc1f`): `Directory.Build.props`, `documentation/release-notes/15.0.0-beta.7.md`, `documentation/benchmarks/comparison-baseline.md`, `documentation/benchmarks/comparison-baseline-aot.md`, `documentation/architecture.md`, and `kanban/in-progress/039-target-net11-0-only-drop-net10-0/task.md`

## Summary

The round-1 fix pass (`9815fa7..fc1fc1f`) resolves all three open `suggestion` findings (`M1`, `M2`, `M3`) cleanly without introducing new issues. `## Goal (implementer restatement)` (including the fix-pass paragraph) matches the original task and Steven's 2026-11-14 superseded directive with no misread. `Directory.Build.props` removes the obsolete non-Core MSBuild `TimeWarpEnableGitMetadata=false` workaround and `documentation/release-notes/15.0.0-beta.7.md:30` accurately states that Visual Studio and `MSBuild.exe` stop with `TWBT0001` (`M1`); both `comparison-baseline.md` (commit `9815fa71fec6ce990965f33c808e6d4d03d91510`, `TimeWarpCallSiteInliningPrototype` = `22.850 ns ± 0.3075 ns`) and `comparison-baseline-aot.md` (commit `a0abd367a410b8ae80c24a6658796295e2e1a4bd`) are re-recorded on clean trees with green post-record gates (`M3`); and `documentation/architecture.md:229-240` (`## Benchmarks`) and `task.md` `## Results` match the `net11.0` baseline row-for-row (`M2`).

Verification notes on the delta:
- Confirmed no `MSBuildRuntimeType` or `TimeWarpEnableGitMetadata` references remain in `Directory.Build.props` or `documentation/`.
- Cross-checked every row of `documentation/benchmarks/comparison-baseline.md` and `comparison-baseline-aot.md` against `documentation/architecture.md:229-240`, `task.md:112-133`, `tests/timewarp-mediator-generators-tests/benchmark-gate-tests.cs` structural assertions (`at most 50%` tolerance sentence and required `TimeWarp*` rows), and the `--gate` report in `artifacts/benchmarks/comparison/net11.0/results/MediatorBenchmarks.DispatchBenchmarks-report-github.md` (`TimeWarpCallSiteInliningPrototype` `22.604 ns`, `TimeWarpGeneratedInterfaceSend` `11.600 ns`, `TimeWarpGeneratedStatePipeline` `132.284 ns`).
- Confirmed `task.md` `## Results` and `### How to validate` record green post-fix `./bin/dev benchmark --project comparison --gate`, `./bin/dev build` (0 warnings, 0 errors), `./bin/dev test` (61 + 50 + 80 on `net11.0`), and `ganda repo audit` after the final product edit.

<!-- Zero open or new issues in round 2. -->
