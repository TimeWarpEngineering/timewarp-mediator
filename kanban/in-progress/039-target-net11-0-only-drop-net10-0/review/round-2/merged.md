# Round 2 — merged findings
**Date:** 2026-11-14
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/15.0.0-beta.7.md:30
- Description: `15.0.0-beta.7.md:8` (`## Breaking`) states that the generator and analyzer move to `net11.0`, the compiler that loads them must run on .NET 11 (`dotnet build`), and Visual Studio and `.NET Framework MSBuild` are not supported. However, `15.0.0-beta.7.md:30` still states that `Directory.Build.props` sets `TimeWarpEnableGitMetadata=false` when `$(MSBuildRuntimeType)` is not `Core` "so those builds still compile", and `Directory.Build.props:31-35` retains that non-Core MSBuild workaround from task 036. Non-Core MSBuild is unsupported and cannot load the `net11.0` generator/analyzer in solution builds (`CS8032`).
- Suggestion: Remove `<PropertyGroup Condition="'$(MSBuildRuntimeType)' != 'Core'">` in `Directory.Build.props:31-35` (letting `TimeWarp.Build.Tasks` 2.0.0 fail fast with `TWBT0001` on non-Core MSBuild) and update `documentation/release-notes/15.0.0-beta.7.md:30` so it no longer claims `.NET Framework MSBuild` / Visual Studio builds compile.
- Source: general
- Disposition notes: Verified fixed in round 2 (`9815fa7`). `Directory.Build.props` removes the non-Core MSBuild `TimeWarpEnableGitMetadata` block, and `documentation/release-notes/15.0.0-beta.7.md:30` states that `.NET Framework MSBuild` (`Visual Studio`, `MSBuild.exe`) stops with `TWBT0001` without workaround because those hosts cannot load the `net11.0` generator and analyzer either.

### M2 — Severity: suggestion — Status: fixed
- File: documentation/architecture.md:229
- Description: `documentation/architecture.md:229-240` (`## Benchmarks`) still describes `[benchmarks/comparison-baseline.md](benchmarks/comparison-baseline.md)` in present tense as the task 021-003 `net10.0` record at commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`, says the `Ratio` column uses the removed `TimeWarpLegacyMakeGenericType` row as `1.00`, and quotes the old `net10.0` / BenchmarkDotNet `0.15.8` means (`8.243 ns`, `9.360 ns`, `131.117 ns`, `8.314 ns`), contradicting the re-recorded `net11.0` `comparison-baseline.md` file (`TimeWarpGeneratedInterfaceSend` = `1.00`).
- Suggestion: Update `documentation/architecture.md:229-240` to match the committed `net11.0` / BenchmarkDotNet `0.16.0-preview.2` baseline in `benchmarks/comparison-baseline.md` (commit SHA, `TimeWarpGeneratedInterfaceSend` as `1.00`, and `net11.0` means).
- Source: general
- Disposition notes: Verified fixed in round 2 (`fc1fc1f`). `documentation/architecture.md:229-240` cites `benchmarks/comparison-baseline.md` on `net11.0` with BenchmarkDotNet `0.16.0-preview.2` at commit `9815fa71fec6ce990965f33c808e6d4d03d91510`, names `TimeWarpGeneratedInterfaceSend` as `1.00`, and quotes exact `net11.0` means and allocations (`12.932 ns` / `24 B`, `9.942 ns` / `0 B`, `133.462 ns` / `0 B`, `11.701 ns` / `24 B`).

### M3 — Severity: suggestion — Status: fixed
- File: documentation/benchmarks/comparison-baseline.md:31
- Description: `documentation/benchmarks/comparison-baseline.md:31` records a noisy `+37%` transient spike on `TimeWarpCallSiteInliningPrototype` (`31.351 ns ± 4.3194 ns`, versus `22.901 ns` on the prior `net10.0` baseline, `22.965 ns` on the October `net11.0` record, and `22.597 ns` on the immediately following gate run noted in `task.md:125`), silently loosening that gated row's `+50%` ceiling from `~34.4 ns` to `47.027 ns`. In addition, `documentation/benchmarks/comparison-baseline-aot.md:8` carries `(working tree had uncommitted changes)` because `--aot` ran on a dirty tree.
- Suggestion: On a clean tree (after committing the product/doc edits), re-run `./bin/dev benchmark --record` on a quiet host so `TimeWarpCallSiteInliningPrototype` reflects steady-state (`~22–23 ns`) with a clean commit header; commit `comparison-baseline.md`, run `./bin/dev benchmark --aot` on the clean tree so `comparison-baseline-aot.md` also has a clean commit header, re-verify `./bin/dev benchmark --gate`, and update `task.md` `## Results` and `documentation/architecture.md` `## Benchmarks` with the clean numbers.
- Source: general
- Disposition notes: Verified fixed in round 2 (`a0abd36`, `6351b9c`, `fc1fc1f`). `comparison-baseline.md` carries clean commit `9815fa71fec6ce990965f33c808e6d4d03d91510` with `TimeWarpCallSiteInliningPrototype` at `22.850 ns ± 0.3075 ns` (`+50%` ceiling `34.275 ns`), `comparison-baseline-aot.md` carries clean commit `a0abd367a410b8ae80c24a6658796295e2e1a4bd` with no uncommitted-changes suffix, `./bin/dev benchmark --project comparison --gate` passed against the clean baseline, and `task.md` `## Results` and `documentation/architecture.md` match every recorded number.

## Duplicates / conflicts

- None. Single `general` reviewer; no new findings in round 2.
