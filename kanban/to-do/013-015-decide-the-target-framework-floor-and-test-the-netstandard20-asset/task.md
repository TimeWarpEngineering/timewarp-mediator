# Decide the target-framework floor and test the netstandard2.0 asset

## Description

TimeWarp.Mediator targets `netstandard2.0;net6.0` against 10.0.12 dependencies whose support floor is net8.0; the build emits TFM-support warnings and the non-net8.0 assets are never executed. Parent review task 013, findings B-12, C-24, part of D-11.

- Library: `netstandard2.0;net6.0`. Contracts, Analyzers, Generators: `netstandard2.0`. Tests and benchmarks: `net8.0`. Windsor sample: `net6.0`. Shared sample library: `netstandard2.1`. CI installs SDK 10.0.x and 8.0.x only.
- No `#if NET*` code exists in the library; the net6.0 asset only changes dependency resolution.
- net6.0 and net8.0 are out of support as of 2026-10.

## Requirements

Decision (Steven, 2026-10-11): **net10.0 or newer only** for everything that ships runtime code. No netstandard2.0, no net6.0, no net8.0. Consumers who cannot move stay on 13.x. Do not add compatibility targets to make upgrading optional.

- `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts`: `<TargetFramework>net10.0</TargetFramework>`. Drop the `Microsoft.Bcl.AsyncInterfaces` reference (netstandard2.0 only) and the netstandard2.0 conditions.
- `TimeWarp.Mediator.Generators` and `TimeWarp.Mediator.Analyzers` **stay `netstandard2.0`**. This is not a compatibility choice; Roslyn loads analyzer and generator assemblies inside the compiler process (including the .NET Framework MSBuild and Visual Studio hosts), and the analyzer packaging contract requires netstandard2.0. Their `analyzers/dotnet/cs` payload has no runtime dependency on the Mediator TFM. State's own Roslyn projects are netstandard2.0 for the same reason.
- Tests, benchmarks, benchmark-comparison, all 12 samples, and the shared sample library: `net10.0`. This retires the Windsor `net6.0` and shared-library `netstandard2.1` targets.
- Lamar can move to 16.0.0 (net9.0/net10.0 assets), which removes the last `ganda nuget outdated` holdout from task 012.
- CI: drop the 8.0.x SDK install in `workflow.yml`; keep 10.0.x.
- `LangVersion` is pinned to 10.0 in `Directory.Build.props`. Decide whether to lift it to the net10.0 default (C# 14); the generator-emitted code must still compile under whatever consumers use, so keep emitted syntax conservative regardless.
- agent.md "Target Frameworks" and "Key Dependencies", readme install section, and `documentation/generated-vs-legacy.md` state the net10.0 floor. Release-notes entry under 14.0.0-beta: breaking, net10.0 minimum.
- Consumer check before merge: Nuru is net10.0, Architecture is net10.0 and net11.0, State is net11.0 (verified 2026-10-11 on local masters). Build all three against the packed nupkgs.

Side effects this unlocks: the TFM-support warnings from the 10.0.12 dependency set disappear; 013-006 needs no netstandard polyfills for the AOT attributes; the "netstandard2.0 execution leg" from the original finding is no longer needed.

## Checklist

- [x] Decision recorded (net10.0 floor; Roslyn components stay netstandard2.0)
- [x] Runtime and Contracts to net10.0; drop Bcl.AsyncInterfaces
- [x] Tests, benchmarks, samples, shared sample library to net10.0; Lamar to 16.0.0
- [x] CI SDK matrix trimmed to 10.0.x
- [x] LangVersion decision recorded and applied
- [x] Docs, agent.md, release notes
- [x] State, Architecture, Nuru build against packed nupkgs
- [x] `./bin/dev workflow` green with zero TFM warnings; `ganda nuget outdated` clean (PR with merge commit: host open-pr node)

## Notes

Parent: 013. The original finding proposed keeping netstandard2.0 as the lowest common floor; Steven rejected that on 2026-10-11: the new line requires net10.0 or newer, older consumers use 13.x.

## Results

- `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` target `net10.0` only. Removed `Microsoft.Bcl.AsyncInterfaces`, `IsExternalInit` and `source/timewarp-mediator/internal/trimming-attributes.cs` (BCL types exist on net10.0). Packed nuspecs: one `net10.0` group each; Mediator depends only on Contracts + DI.Abstractions 10.0.12.
- Generators and Analyzers unchanged at `netstandard2.0` (Roslyn host contract); nupkg layout asserts pass.
- All 7 test/benchmark projects, 11 samples and the shared sample library to `net10.0` (Windsor `net6.0` and shared `netstandard2.1` retired). Lamar 15.0.1 → 16.0.0. xunit.runner.visualstudio 4.0.0 → 4.0.1 so `ganda nuget outdated` is clean.
- CI (`workflow.yml`, both jobs): only `10.0.x` SDK installed.
- **LangVersion decision:** removed the repo-wide `<LangVersion>10.0</LangVersion>` pin. net10.0 projects use the SDK default (C# 14); the netstandard2.0 Roslyn projects and several tests already set `latest`. Generator-emitted code is unaffected by the host project's LangVersion and stays conservative (the Roslyn 4.8 floor leg still passes).
- Docs: agent.md (Target Frameworks, Key Dependencies, Build Properties), readme (new "Target framework" section + SDK 10 note), `documentation/generated-vs-legacy.md` (Target framework section), release notes 14.0.0 (breaking: net10.0 minimum).
- `./bin/dev workflow`: clean → build → test (455 passed, 2 pre-existing skips) → pack, SUCCEEDED. Zero NETSDK/NU TFM-support warnings; only the pre-existing RS0030 banned-API warnings in samples/benchmarks remain (those projects do not treat warnings as errors).
- Consumer check (2026-10-12, `git archive` of local masters into /tmp, isolated `NUGET_PACKAGES` seeded from `artifacts/packages`): timewarp-state (net11.0) 0 errors, timewarp-architecture 0 errors, timewarp-nuru 0 errors. Every consumer `project.assets.json` resolved `lib/net10.0/TimeWarp.Mediator.Contracts.dll`; the restored dll hash matches this branch's build.
- Environment note: if the shared compiler server has a stale `TimeWarp.Mediator.Analyzers` 14.0.0.0 loaded from another worktree, the generator fails with CS8784 `TypeLoadException` (MembershipOptions) and the build shows `Generated` namespace errors. Fix with `dotnet build-server shutdown` or `UseSharedCompilation=false`; this is not caused by the change.

### How to validate

Smoke:

```bash
UseSharedCompilation=false ./bin/dev workflow
unzip -l artifacts/packages/TimeWarp.Mediator.14.0.0-beta.4.nupkg | grep lib/
unzip -l artifacts/packages/TimeWarp.Mediator.Contracts.14.0.0-beta.4.nupkg | grep lib/
grep -rn "<TargetFramework" --include=*.csproj source tests samples
ganda nuget outdated
```

Expect:

- Workflow prints `Pipeline SUCCEEDED`, all five test assemblies `(net10.0)` with 0 failed, and `Package layout verified` for Analyzers and Generators.
- Both runtime nupkgs contain only `lib/net10.0/`.
- `<TargetFramework>` is `net10.0` everywhere except `timewarp-mediator-generators` and `timewarp-mediator-analyzers` (`netstandard2.0`); no `<TargetFrameworks>` multi-target remains.
- No `NETSDK`/`NU` target-framework-support warnings in the build output.
- `ganda nuget outdated` reports `All packages are up to date!`.

## Session

- Created: 686431 (2026-10-11)
- Implement (2026-10-12): retarget to net10.0, CI/docs/release notes, workflow green, consumers verified
