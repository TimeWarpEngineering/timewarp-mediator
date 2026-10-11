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
- [ ] Runtime and Contracts to net10.0; drop Bcl.AsyncInterfaces
- [ ] Tests, benchmarks, samples, shared sample library to net10.0; Lamar to 16.0.0
- [ ] CI SDK matrix trimmed to 10.0.x
- [ ] LangVersion decision recorded and applied
- [ ] Docs, agent.md, release notes
- [ ] State, Architecture, Nuru build against packed nupkgs
- [ ] `./bin/dev workflow` green with zero TFM warnings; `ganda nuget outdated` clean; PR with merge commit

## Notes

Parent: 013. The original finding proposed keeping netstandard2.0 as the lowest common floor; Steven rejected that on 2026-10-11: the new line requires net10.0 or newer, older consumers use 13.x.

## Session

- Created: 686431 (2026-10-11)
