# Profiles and packaging: one emission, two packages, SDK-10 Roslyn floor

## Description

The generator emits three profiles (Host, Aot, Link) with `ServiceGen` and diagnostics TWM005
and TWM006 to describe them; the Aot and Link profiles drop pipeline behaviors. Four packages
ship where two are needed, and the Roslyn 4.8 floor plus its test project exist only to support
hosts no TimeWarp consumer uses. Parent review task 021, findings F11, F14, F15; decisions D4
and D5 signed off 2026-10-23.

## Goal (implementer restatement)

Ship one generated mediator. The host always supplies `IServiceProvider`, pipeline behaviors
are always woven, and the Aot and Link emission paths, `ServiceGen`, `TimeWarpMediatorProfile`,
TWM005, and TWM006 are gone. TWM009 goes with them because the stream feature is already gone.

The contracts assembly becomes the `TimeWarp.Mediator` package (id, assembly name, and project
folder). That package carries the TWM001–TWM004 analyzer DLL as an analyzer asset. The separate
`TimeWarp.Mediator.Analyzers` nupkg is deleted; the analyzer project stays because it is
netstandard2.0 and the library is net10.0. `TimeWarp.Mediator.Generators` stays and reports
TWM007, TWM008, TWM010, and TWM011. It no longer bundles the analyzer DLL.

The Roslyn floor for the shipped components is the stable compiler package that matches the
oldest .NET 10 SDK (Microsoft.CodeAnalysis.CSharp 5.0.0). The Roslyn 4.8 floor test project
is deleted. `dev pack` fails unless the artifacts directory contains exactly the two expected
nupkgs, and the package-smoke consumer references only those two packages. The AOT sample
publishes with PublishAot, IsAotCompatible, trim and AOT analyzers on, no NoWarn of IL2026 or
IL3050, and a behavior that actually runs. `documentation/diagnostics.md` matches the new
diagnostic ownership. TreatWarningsAsErrors, the banned-API analyzer, and Source Link stay.

Done when `./bin/dev workflow` is green on this branch and the changes are committed and
pushed. Opening the pull request is the host open-pr node, not this walk.

## Requirements

- Remove `TimeWarpMediatorProfile`, the Aot and Link emission paths, `ServiceGen`, TWM005 and
  TWM006. One emission; the host supplies `IServiceProvider`.
- The AOT sample publishes with `PublishAot`, `IsAotCompatible`, trim and AOT analyzers on, no
  `NoWarn` on IL2026/IL3050, with behaviors woven (gate from review section 9).
- Rename the contracts package to `TimeWarp.Mediator` (`PackageId`, `AssemblyName`, project
  folder via `git mv`). The analyzer DLL (TWM001-TWM004) ships as an analyzer asset inside it;
  the separate `TimeWarp.Mediator.Analyzers` package is deleted. `TimeWarp.Mediator.Generators`
  stays, bundling TWM007-TWM011. TWM009 is deleted with the stream feature.
- Raise the Roslyn floor to the version shipped with the .NET 10 SDK; delete
  `tests/timewarp-mediator-generators-roslyn-floor-tests`; update `Directory.Packages.props`.
- `dev pack` asserts exactly two nupkgs with the expected layout; package smoke consumer
  references the two packages only.
- `documentation/diagnostics.md` updated for the removed and moved diagnostics.
- Keep `TreatWarningsAsErrors`, banned-API analyzer and Source Link intact.

## Checklist

- [x] Profiles, `ServiceGen`, TWM005, TWM006 removed
- [x] AOT sample publishes with behaviors woven and analyzers on
- [x] Contracts package renamed to `TimeWarp.Mediator`, analyzer bundled, Analyzers package deleted
- [x] Roslyn floor raised, floor test project deleted
- [x] `dev pack` asserts two packages; smoke consumer updated
- [x] `documentation/diagnostics.md` updated
- [x] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. Depends on: 021-004 (the old `TimeWarp.Mediator` package id must be free before
the rename). Review sections 3 (F11, F14, F15), 4 (packages, diagnostics), 7 (D4, D5).
Nuru reads `build_property.TimeWarpMediatorProfile` today; its replacement is the
`[GeneratedMediator]` attribute from 021-002/021-003, tracked in the Nuru consumer task.

## Results

One emission. The generated mediator takes `IServiceProvider`, weaves behaviors, and no longer
emits `ServiceGen` or a profile field. TWM005, TWM006, and TWM009 are removed. The contracts
folder is `source/timewarp-mediator` with package id `TimeWarp.Mediator`, and that nupkg carries
`analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`. `TimeWarp.Mediator.Generators` is the
second nupkg and does not bundle the analyzer. Roslyn is pinned at `Microsoft.CodeAnalysis.CSharp`
5.0.0. The AOT sample publishes with trim and AOT analyzers as errors and fails unless
`StampBehavior` runs on both sends. `documentation/diagnostics.md` and the packed readmes match.
`agent.md`, the skill, migration, overview, and release notes stay for 021-007. The pull request
is the host open-pr node.

`ganda repo audit`: Passed 32, Failed 0, Skipped 2.

### How to validate

Smoke: `./bin/dev workflow`

Expect: the log ends with `Pipeline SUCCEEDED`. Analyzers 42 passed, generators 57 passed,
compilation 53 passed, failed 0. NativeAOT publish of `timewarp-mediator-examples-aot` exits 0
and prints `aot-pong` (the process exits 1 unless `StampBehavior` ran twice). Pack reports
layout verified for `TimeWarp.Mediator.14.0.0.nupkg` and `TimeWarp.Mediator.Generators.14.0.0.nupkg`
only. Package smoke prints `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
- Implementation: implement oracle (2026-10-24). `./bin/dev workflow` Pipeline SUCCEEDED.
