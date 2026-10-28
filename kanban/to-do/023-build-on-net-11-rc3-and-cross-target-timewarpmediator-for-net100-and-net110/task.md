# Task 023: Build on .NET 11 RC3 and cross-target TimeWarp.Mediator for net10.0 and net11.0

## Description

Steven's direction (2026-10-28): move the build to the .NET 11 SDK (currently
`11.0.100-rc.3.26522.148`, installed locally; RC builds carry a go-live license) and ship
`TimeWarp.Mediator` for both `net10.0` and `net11.0`. This adds a newer target; it does not add
an older one, so it is consistent with the net10-or-newer library policy. The generator and
analyzer stay `netstandard2.0` and are unaffected; the Roslyn 5.0.0 floor is a minimum and the
newer compiler in the 11 SDK loads them.

Depends on: 022 (same `Directory.Packages.props`; land 022 first and build on it).

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
- [ ] `TimeWarp.Mediator` targets `net10.0;net11.0`; generator and analyzer untouched
- [ ] Tests, benchmarks, samples and smoke multi-target both TFMs
- [ ] DI pins conditioned per TFM; no RC bits in net10.0 hosts
- [ ] `global.json` and CI on the 11 RC3 SDK with 10.0.x retained
- [ ] Pack assertion and `agent.md` updated for two lib folders
- [ ] net11.0 benchmark numbers recorded without overwriting the net10.0 baseline
- [ ] `./bin/dev workflow` green on both TFMs
- [ ] GA follow-up task created and linked in Results
### Documentation
- [ ] `agent.md` Target Frameworks and Key Dependencies sections updated
- [ ] `documentation/overview.md` build/CI notes mention the SDK pin
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
