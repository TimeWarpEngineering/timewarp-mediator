# Task 023: Build on .NET 11 RC3 and cross-target TimeWarp.Mediator for net10.0 and net11.0

## Description

Steven's direction (2026-10-28): move the build to the .NET 11 SDK (currently
`11.0.100-rc.3.26522.148`, installed locally; RC builds carry a go-live license) and ship
`TimeWarp.Mediator` for both `net10.0` and `net11.0`. This adds a newer target; it does not add
an older one, so it is consistent with the net10-or-newer library policy. The generator and
analyzer stay `netstandard2.0` and are unaffected; the Roslyn 5.0.0 floor is a minimum and the
newer compiler in the 11 SDK loads them.

Depends on: 022 (same `Directory.Packages.props`; land 022 first and build on it).

## Goal (implementer restatement)

Build this repo with the .NET 11 RC3 SDK and ship `TimeWarp.Mediator` for both `net10.0` and `net11.0`. The generator and analyzer stay `netstandard2.0`. This adds a newer target; it does not lower the floor.

Acceptance:

- `TimeWarp.Mediator` multi-targets `net10.0;net11.0`. The generator and analyzer projects stay `netstandard2.0`.
- The generator tests, compilation tests, analyzer tests, comparison benchmark, samples, and package-smoke consumer multi-target the same two frameworks. The AOT sample publishes on both, and trim and AOT warnings fail that publish.
- `Microsoft.Extensions.DependencyInjection` is the 10.0 line on `net10.0` and `11.0.0-rc.3.26522.148` on `net11.0`. A `net10.0` host does not restore the 11 RC package.
- `global.json` pins SDK `11.0.100-rc.3` with `rollForward: latestFeature` and `allowPrerelease: true`. CI installs `11.0.x` at preview quality and still installs `10.0.x` so the net10.0 runtime pack restores.
- `dev pack` requires exactly `lib/net10.0` and `lib/net11.0` for `TimeWarp.Mediator`. The smoke consumer pongs on both. Generators layout stays as it is.
- A net11.0 comparison run is stored in `documentation/benchmarks/comparison-baseline-net11.md`. `comparison-baseline.md` stays the net10.0 gate record.
- `./bin/dev workflow` is green on this SDK: clean, a build with 0 warnings, tests and samples on both frameworks, and a pack of the two nupkgs.
- `<Version>` stays `15.0.0-beta.1`. A follow-up task records the GA swap (`11.0.100`, DI `11.0.0`, drop `dotnet-quality: preview`) for when .NET 11 ships. The dev CLI target is stated in Results either way.
- `ganda repo audit` is clean. Product changes are committed and pushed.

## Requirements

- `source/timewarp-mediator/timewarp-mediator.csproj`: `<TargetFrameworks>net10.0;net11.0</TargetFrameworks>`.
  The generator and analyzer projects stay `netstandard2.0`.
- Tests (`timewarp-mediator-generators-tests`, `-compilation-tests`, `-analyzers-tests`), the
  benchmark comparison project, the samples, and the package smoke consumer multi-target
  `net10.0;net11.0` so both lib assets are exercised. The AOT sample publishes on both TFMs with
  trim and AOT warnings as errors.
- `Directory.Packages.props`: `Microsoft.Extensions.DependencyInjection` pinned per TFM, the
  10.0.x line for `net10.0` and `11.0.0-rc.3.*` for `net11.0` (conditioned on
  `$(TargetFramework)`). Do not pull 11 RC bits into the net10.0 hosts.
- SDK: add a `global.json` pinning `11.0.100-rc.3` with `rollForward: latestFeature` and
  `allowPrerelease: true`, or document why not. CI (`.github/workflows/workflow.yml`, both
  `setup-dotnet` steps) installs `11.0.x` with `dotnet-quality: preview` **and** keeps `10.0.x`
  so the net10.0 runtime pack restores. Dependabot config unchanged.
- `tools/dev-cli/endpoints/pack-command.cs`: the layout assertion changes from a single
  `lib/net10.0` folder to exactly `lib/net10.0` and `lib/net11.0` for `TimeWarp.Mediator`;
  Generators layout unchanged. Update `agent.md`'s pack row to match.
- The dev CLI itself (`tools/dev-cli`, a TimeWarp.Nuru runfile) may stay `net10.0`. If
  TimeWarp.Nuru 4.0.0-beta.6 builds on net11.0, moving it is optional and must be noted in
  Results either way.
- Benchmarks: run `./bin/dev benchmark --project comparison` on net11.0 and record the numbers
  in Results. Do **not** overwrite `documentation/benchmarks/comparison-baseline.md` (that is
  the net10.0 gate record from 021-003). If the recorder cannot distinguish TFMs, add a
  `--framework` option to `dev benchmark` and record `comparison-baseline-net11.md` separately,
  or document the gap for a follow-up.
- `./bin/dev workflow` green on the 11 SDK: clean, build with 0 warnings, tests on both TFMs,
  verify-samples on both, pack (two nupkgs, both lib folders asserted, smoke pongs on both).
- `ganda repo audit --fix` applied and committed.
- `<Version>` stays `15.0.0-beta.1` unless Steven says otherwise. Not a release.
- Results must list the GA follow-up: swap `rc.3` pins and the SDK pin for `11.0.0` /
  `11.0.100` and drop `dotnet-quality: preview` when .NET 11 ships (expected November 2026).
  Create that as a child or a to-do task before closing this one.

## Checklist

### Implementation
- [x] `TimeWarp.Mediator` targets `net10.0;net11.0`; generator and analyzer untouched
- [x] Tests, benchmarks, samples and smoke multi-target both TFMs
- [x] DI pins conditioned per TFM; no RC bits in net10.0 hosts
- [x] `global.json` and CI on the 11 RC3 SDK with 10.0.x retained
- [x] Pack assertion and `agent.md` updated for two lib folders
- [x] net11.0 benchmark numbers recorded without overwriting the net10.0 baseline
- [x] `./bin/dev workflow` green on both TFMs
- [x] GA follow-up task created and linked in Results
### Documentation
- [x] `agent.md` Target Frameworks and Key Dependencies sections updated
- [x] `documentation/overview.md` build/CI notes mention the SDK pin
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Notes

- Consumers (TimeWarp.State, TimeWarp.Architecture, TimeWarp.Nuru) stay on net10.0 and keep
  resolving the net10.0 asset; nothing downstream changes until they opt in.
- Policy reference: TimeWarp libraries target net10.0 or newer only; Roslyn host assemblies are
  the one netstandard2.0 exception.
- Local SDKs on Steven's machine today: 11.0.100-preview.6, rc.1, rc.2, rc.3.

## Implementation Notes

- 

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-28)
- Created: cockpit session 04be23b7 (2026-10-28), Steven: "update to dotnet 11 RC3 and cross compile for 10 and 11"
- Implementation: task-work implementer (2026-10-28), SDK `11.0.100-rc.3.26522.148`
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a18173-1861-7083-969b-a43c4d28f820, max-turns 500, Done — 2026-10-28T08:16:46Z

## Results

`TimeWarp.Mediator` and the hosts (generator tests, compilation tests, analyzer tests, the comparison benchmark, both samples, and the package-smoke consumer) multi-target `net10.0;net11.0`. The generator and analyzer projects stay `netstandard2.0`. `<Version>` stays `15.0.0-beta.1`.

`global.json` pins SDK `11.0.100-rc.3` with `rollForward: latestFeature` and `allowPrerelease: true`. `dotnet --version` in this tree is `11.0.100-rc.3.26522.148`. CI installs `10.0.x` and `11.0.x` (`dotnet-quality: preview` on the 11 step only) in both the `ci` and `publish` jobs, so the net10.0 runtime pack still restores.

`Microsoft.Extensions.DependencyInjection` is `10.0.14` when `$(TargetFramework)` is `net10.0` and `11.0.0-rc.3.26522.148` when it is `net11.0`. A net10.0 host does not restore the 11 RC. `dotnet list package` on the named-pipelines sample shows `10.0.14` for `net10.0` and `11.0.0-rc.3.26522.148` for `net11.0`.

The dev CLI stays `net10.0` (`#:property TargetFramework=net10.0` on `tools/dev-cli/dev.cs`). `TimeWarp.Nuru` 4.0.0-beta.7 ships `lib/net10.0` only, so the CLI was not moved to net11.0. A net11.0 host can still restore that asset. `ganda repo audit --fix` moved `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` from `4.0.0-beta.6` to `4.0.0-beta.7`. `dotnet run --file tools/dev-cli/dev.cs -- benchmark --help` succeeds on that pin.

`dev pack` requires exactly `lib/net10.0` and `lib/net11.0` for `TimeWarp.Mediator`. The generators layout is unchanged.

BenchmarkDotNet `0.15.8` throws on a net11.0 host (`GetRuntimeVersion` for `RuntimeMoniker.NotRecognized`) before it measures. The comparison project pins `0.16.0-preview.2`, which runs the same out-of-process job. `documentation/benchmarks/comparison-baseline.md` is still the net10.0 gate record from 021-003 (commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`). The net11.0 record is `documentation/benchmarks/comparison-baseline-net11.md`, commit `d84a8549902bcd8504b4dc2068e6d50a34edd3a6`, 2026-10-28, SDK `11.0.100-rc.3.26522.148`, runtime `11.0.0-rc.3.26522.148`, warmup 3, iteration 8. The gate does not read that file. Means:

| Method | Mean | Allocated |
| --- | ---: | ---: |
| TimeWarpGeneratedMonomorphicSend | 9.919 ns | 24 B |
| TimeWarpGeneratedInterfaceSend | 12.218 ns | 24 B |
| TimeWarpGeneratedObjectSend | 21.290 ns | 96 B |
| TimeWarpGeneratedInterfaceSendUnit | 10.191 ns | 0 B |
| TimeWarpGeneratedMonomorphicSendUnit | 8.091 ns | 0 B |
| TimeWarpCallSiteInliningPrototype | 22.965 ns | 24 B |
| TimeWarpGeneratedScopedSend | 12.874 ns | 24 B |
| TimeWarpGeneratedInterfacePublish | 22.708 ns | 0 B |
| TimeWarpGeneratedScopedPublish | 22.102 ns | 0 B |
| TimeWarpGeneratedStatePipeline | 140.910 ns | 0 B |
| TimeWarpGeneratedArchitectureSend | 91.508 ns | 24 B |
| MartinothamarGenerated | 12.000 ns | 24 B |

GA follow-up: task 023-001, in `kanban/to-do/` on origin-home, depends on 023. When .NET 11 ships (expected November 2026), swap the `global.json` pin to `11.0.100`, the net11.0 DependencyInjection pin to `11.0.0`, and drop `dotnet-quality: preview`. That task also moves BenchmarkDotNet off `0.16.0-preview.2` when `0.16.0` stable is on NuGet, and it does not rewrite the net10.0 gate file.

`ganda repo audit --fix` also landed this kitchen onto origin-home. The advisory `kanban-landed` warning for claim 021 (no usable worktree) is another id; this task does not own that claim.

### How to validate

Smoke: `UseSharedCompilation=false dotnet run --file tools/dev-cli/dev.cs -- workflow` on SDK `11.0.100-rc.3.26522.148`.

Expect: the log ends with `Pipeline SUCCEEDED`. Release build reports 0 warnings and 0 errors. generators-tests 57 passed, analyzers-tests 42 passed, and compilation-tests 69 passed, each on `net10.0` and on `net11.0`, failed 0. The AOT sample publishes and runs on both frameworks. named-pipelines runs on both. Pack layout is verified for `TimeWarp.Mediator.15.0.0-beta.1.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.1.nupkg`. Package smoke prints `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on `net10.0` and on `net11.0`.

The same command was run again after the BenchmarkDotNet `0.16.0-preview.2` pin and the Nuru `4.0.0-beta.7` pin. It ended with `Pipeline SUCCEEDED`: build 0 warnings and 0 errors, the same test counts on both frameworks, both sample publishes and runs, both package layouts verified, and `package-pong` on both frameworks.
