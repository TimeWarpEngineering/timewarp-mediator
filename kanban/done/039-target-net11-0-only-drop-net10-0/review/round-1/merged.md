# Round 1 — merged findings
**Date:** 2026-11-14
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: documentation/release-notes/15.0.0-beta.7.md:30
- Description: `15.0.0-beta.7.md:8` (`## Breaking`) states that the generator and analyzer move to `net11.0`, the compiler that loads them must run on .NET 11 (`dotnet build`), and Visual Studio and `.NET Framework MSBuild` are not supported. However, `15.0.0-beta.7.md:30` still states that `Directory.Build.props` sets `TimeWarpEnableGitMetadata=false` when `$(MSBuildRuntimeType)` is not `Core` "so those builds still compile", and `Directory.Build.props:31-35` retains that non-Core MSBuild workaround from task 036. Non-Core MSBuild is unsupported and cannot load the `net11.0` generator/analyzer in solution builds (`CS8032`).
- Suggestion: Remove `<PropertyGroup Condition="'$(MSBuildRuntimeType)' != 'Core'">` in `Directory.Build.props:31-35` (letting `TimeWarp.Build.Tasks` 2.0.0 fail fast with `TWBT0001` on non-Core MSBuild) and update `documentation/release-notes/15.0.0-beta.7.md:30` so it no longer claims `.NET Framework MSBuild` / Visual Studio builds compile.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: documentation/architecture.md:229
- Description: `documentation/architecture.md:229-240` (`## Benchmarks`) still describes `[benchmarks/comparison-baseline.md](benchmarks/comparison-baseline.md)` in present tense as the task 021-003 `net10.0` record at commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`, says the `Ratio` column uses the removed `TimeWarpLegacyMakeGenericType` row as `1.00`, and quotes the old `net10.0` / BenchmarkDotNet `0.15.8` means (`8.243 ns`, `9.360 ns`, `131.117 ns`, `8.314 ns`), contradicting the re-recorded `net11.0` `comparison-baseline.md` file (`TimeWarpGeneratedInterfaceSend` = `1.00`).
- Suggestion: Update `documentation/architecture.md:229-240` to match the committed `net11.0` / BenchmarkDotNet `0.16.0-preview.2` baseline in `benchmarks/comparison-baseline.md` (commit SHA, `TimeWarpGeneratedInterfaceSend` as `1.00`, and `net11.0` means).
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: documentation/benchmarks/comparison-baseline.md:31
- Description: `documentation/benchmarks/comparison-baseline.md:31` records a noisy `+37%` transient spike on `TimeWarpCallSiteInliningPrototype` (`31.351 ns ± 4.3194 ns`, versus `22.901 ns` on the prior `net10.0` baseline, `22.965 ns` on the October `net11.0` record, and `22.597 ns` on the immediately following gate run noted in `task.md:125`), silently loosening that gated row's `+50%` ceiling from `~34.4 ns` to `47.027 ns`. In addition, `documentation/benchmarks/comparison-baseline-aot.md:8` carries `(working tree had uncommitted changes)` because `--aot` ran on a dirty tree.
- Suggestion: On a clean tree (after committing the product/doc edits), re-run `./bin/dev benchmark --record` on a quiet host so `TimeWarpCallSiteInliningPrototype` reflects steady-state (`~22–23 ns`) with a clean commit header; commit `comparison-baseline.md`, run `./bin/dev benchmark --aot` on the clean tree so `comparison-baseline-aot.md` also has a clean commit header, re-verify `./bin/dev benchmark --gate`, and update `task.md` `## Results` and `documentation/architecture.md` `## Benchmarks` with the clean numbers.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. Single `general` reviewer.

## Verified without findings

- **Goal restatement alignment:** `## Goal (implementer restatement)` matches `task.md` Description and Steven's 2026-11-14 superseded Requirements (no misread).
- **All 11 `.csproj` targets and Roslyn extensions:** Verified all 11 `.csproj` files under `source/`, `tests/`, and `samples/` use single `<TargetFramework>net11.0</TargetFramework>`. `source/timewarp-mediator-generators` and `source/timewarp-mediator-analyzers` drop `netstandard2.0`, `<LangVersion>latest</LangVersion>`, `<Nullable>enable</Nullable>` (inherited from root `Directory.Build.props`), and `source/timewarp-mediator-generators/is-external-init.cs`. Only `RS1041` is added to `NoWarn` on those two projects, with comments and matching `+1` entries in `.timewarp/suppression-baseline.json`. `RoslynCompilerTests.ReferencedCompiler_IsNotNewerThanTheSdkCompiler` is retained and covers both generator and analyzer assemblies.
- **Generated nuspecs and `dev pack` layout gate:** Inspected `source/timewarp-mediator/obj/Release/TimeWarp.Mediator.15.0.0-beta.7.nuspec` and `source/timewarp-mediator-generators/obj/Release/TimeWarp.Mediator.Generators.15.0.0-beta.7.nuspec` (`<Version>` remains `15.0.0-beta.7`, empty `net11.0` dependency group on `TimeWarp.Mediator`, `lib/net11.0/TimeWarp.Mediator.dll`, `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`, `analyzers/dotnet/cs/TimeWarp.Mediator.Generators.dll`, `buildTransitive/TimeWarp.Mediator.Generators.props`, `notice`, `unlicense`, and per-package `readme.md`). `RepoLayout.GateTargetFramework = "net11.0"` and `LibTargetFrameworks = [GateTargetFramework]`, so `pack-command.cs` rejects any `lib/` folder other than `lib/net11.0` and runs `tests/timewarp-mediator-package-smoke` on `net11.0`.
- **Central pins, CI, and remaining `net10` grep:** `Directory.Packages.props` has unconditioned `Microsoft.Extensions.DependencyInjection` `11.0.0`, `Microsoft.Extensions.Logging` `11.0.0`, and `BenchmarkDotNet` `0.16.0-preview.2`, plus the audit-driven dev-only bumps (`TimeWarp.SourceGenerators` `1.0.0-beta.15`, `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` `3.0.0-beta.2`, `TimeWarp.Terminal` `2.0.0-beta.1`) and `.githooks/*.cs` `--fix` scaffolds (policy per `tw-pr`). `.github/workflows/workflow.yml` installs only `11.0.x` across `ci`, `publish`, and `benchmark`, renames the step to `Benchmark gate (net11.0)`, and updates the `benchmark` mode description. Remaining `net10` hits outside `kanban/` and older history are intentional consumer migration notes (`15.0.0-beta.6 or earlier`) and the `pack-command.cs` comment noting `lib/net10.0` fails.
- **Recorded gates and operator sign-off note for round 2:** `task.md` `## Results` and `### How to validate` record green `./bin/dev build`, `./bin/dev test` (61 + 50 + 80 on `net11.0`), `ganda repo audit`, `./bin/dev pack` (`package-pong`), `./bin/dev verify-samples --no-build` (NativeAOT + named-pipelines), `./bin/dev check-version --strict`, `./bin/dev benchmark --gate`, and rebuilt `bin/dev` (`self-install`). Note for round 2: `task.md:16,43` specifies "Steven approves the new gate numbers at review" once the clean re-record in M3 is in place.
