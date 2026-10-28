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

Fix pass for review round 1:

The six hosts that multi-target `net10.0;net11.0` (generator tests, compilation tests, analyzer tests, the comparison benchmark, the named-pipelines sample, and the AOT sample) do not set `RollForward` to `LatestMajor`. A `net10.0` leg stays on the 10 runtime after 11.0.0 GA is installed. Results state that a net10.0 leg runs on the 10 runtime.

`./bin/dev benchmark --gate` runs once with BenchmarkDotNet `0.16.0-preview.2` against `documentation/benchmarks/comparison-baseline.md`. Results record whether that gate passed or failed.

Fix pass for review round 4 (M3):

BenchmarkDotNet is pinned per target framework: `0.15.8` when `$(TargetFramework)` is `net10.0`, and `0.16.0-preview.2` when it is `net11.0`. The net10.0 leg is the tool that recorded `documentation/benchmarks/comparison-baseline.md`. `./bin/dev benchmark --gate` runs again on that pin. Results record pass or fail, and name the BenchmarkDotNet version the gate host printed. The `agent.md` benchmark row and the `Directory.Packages.props` comment match the pin. If the gate still fails on `0.15.8`, Results call that a regression and a task owns getting the gate green. If the per-framework pin does not restore or run, Results say why. Either way, a task has an explicit item to get `./bin/dev benchmark --gate` green again.

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
- [x] Six multi-targeted hosts omit `RollForward=LatestMajor`
- [x] net10.0 `--gate` result recorded on BenchmarkDotNet `0.16.0-preview.2`
- [x] BenchmarkDotNet is `0.15.8` on `net10.0` and `0.16.0-preview.2` on `net11.0`
- [x] net10.0 `--gate` passed on BenchmarkDotNet `0.15.8`
- [x] Task 023-001 owns keeping `./bin/dev benchmark --gate` green
### Documentation
- [x] `agent.md` Target Frameworks and Key Dependencies sections updated
- [x] `documentation/overview.md` build/CI notes mention the SDK pin
### Review
- [ ] Code Review
- [ ] PR with merge commit

## Review

- Round 1: M1 and M2 were open. This fix pass removed `RollForward=LatestMajor` and recorded the gate result.
- Round 2 (2026-10-28, effort 1, general): M1 and M2 fixed. M3 (suggestion) is open: the net10.0 `--gate` is red on the BenchmarkDotNet preview, and no task owns getting it green again. Fix pass requested.
- Round 2: Blocked — open M3
- Round 3 (2026-10-28, effort 1, general): the delta `0448d20..4563d15` is kitchen-only. No fix pass for M3 has landed. M1 and M2 stay fixed and M3 stays open. The `## Goal (implementer restatement)` has no fix-pass item for M3 yet, so the next implementer pass should add one. Fix pass requested for M3.
- Round 3: Blocked — open M3
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
- Review oracle round 1: claude-opus-5-5 (ganda task work, headless), effort 2, roster general, 2026-10-28. 2 open (M1 suggestion, M2 nit); fix pass requested
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-28T08:20:33Z
- Review oracle round 2: claude-opus-5-5 (ganda task work, headless), effort 1, roster general, max-turns 80, 2026-10-28. M1 and M2 fixed. 1 open (M3 suggestion). Fix pass requested
- Review oracle round 3: claude-opus-5-5 (ganda task work, headless), effort 1, roster general, max-turns 80, 2026-10-28. The delta is kitchen-only. M3 is still open. Fix pass requested
- Fix pass: implement-fix-2 (2026-10-28), SDK `11.0.100-rc.3.26522.148`. Dropped `RollForward=LatestMajor` from the six multi-targeted hosts. `./bin/dev benchmark --gate` failed two monomorphic rows against the 0.15.8 baseline. The baseline file was left unchanged.
- Fix pass: implement-fix-4 (2026-10-28), SDK `11.0.100-rc.3.26522.148`. Pinned BenchmarkDotNet per framework. `./bin/dev benchmark --gate` passed on `0.15.8` (exit 0). Published the gate item on task 023-001 (`origin/master` `d544794`).

## Results

`TimeWarp.Mediator` and the hosts (generator tests, compilation tests, analyzer tests, the comparison benchmark, both samples, and the package-smoke consumer) multi-target `net10.0;net11.0`. The generator and analyzer projects stay `netstandard2.0`. `<Version>` stays `15.0.0-beta.1`.

`global.json` pins SDK `11.0.100-rc.3` with `rollForward: latestFeature` and `allowPrerelease: true`. `dotnet --version` in this tree is `11.0.100-rc.3.26522.148`. CI installs `10.0.x` and `11.0.x` (`dotnet-quality: preview` on the 11 step only) in both the `ci` and `publish` jobs, so the net10.0 runtime pack still restores.

`Microsoft.Extensions.DependencyInjection` is `10.0.14` when `$(TargetFramework)` is `net10.0` and `11.0.0-rc.3.26522.148` when it is `net11.0`. A net10.0 host does not restore the 11 RC. `dotnet list package` on the named-pipelines sample shows `10.0.14` for `net10.0` and `11.0.0-rc.3.26522.148` for `net11.0`.

The dev CLI stays `net10.0` (`#:property TargetFramework=net10.0` on `tools/dev-cli/dev.cs`). `TimeWarp.Nuru` 4.0.0-beta.7 ships `lib/net10.0` only, so the CLI was not moved to net11.0. A net11.0 host can still restore that asset. `ganda repo audit --fix` moved `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` from `4.0.0-beta.6` to `4.0.0-beta.7`. `dotnet run --file tools/dev-cli/dev.cs -- benchmark --help` succeeds on that pin.

`dev pack` requires exactly `lib/net10.0` and `lib/net11.0` for `TimeWarp.Mediator`. The generators layout is unchanged.

BenchmarkDotNet `0.15.8` throws on a net11.0 host (`GetRuntimeVersion` for `RuntimeMoniker.NotRecognized`) before it measures. The comparison project pins BenchmarkDotNet per framework: `0.15.8` on `net10.0` and `0.16.0-preview.2` on `net11.0`. The preview runs the net11.0 job. The net10.0 gate uses `0.15.8`. `documentation/benchmarks/comparison-baseline.md` is still the net10.0 gate record from 021-003 (commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`). The net11.0 record is `documentation/benchmarks/comparison-baseline-net11.md`, commit `d84a8549902bcd8504b4dc2068e6d50a34edd3a6`, 2026-10-28, SDK `11.0.100-rc.3.26522.148`, runtime `11.0.0-rc.3.26522.148`, warmup 3, iteration 8. The gate does not read that file. Means:

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

GA follow-up: task 023-001, in `kanban/to-do/` on origin-home, depends on 023. When .NET 11 ships (expected November 2026), swap the `global.json` pin to `11.0.100`, the net11.0 DependencyInjection pin to `11.0.0`, and drop `dotnet-quality: preview`. That task moves the net11.0 BenchmarkDotNet pin off `0.16.0-preview.2` when `0.16.0` stable is on NuGet. It leaves the net10.0 pin on `0.15.8` and keeps `./bin/dev benchmark --gate` green. It does not rewrite the net10.0 gate file unless Steven approves a re-record. The gate item is on origin-home as `d544794`.

`ganda repo audit --fix` also landed this kitchen onto origin-home. The advisory `kanban-landed` warning for claim 021 (no usable worktree) is another id; this task does not own that claim.

### Fix pass (review round 1)

M1. The six multi-targeted hosts no longer set `<RollForward>LatestMajor</RollForward>`: generator tests, compilation tests, analyzer tests, the comparison benchmark, named-pipelines, and the AOT sample. Their Release `runtimeconfig.json` files omit `rollForward`, so the host keeps the minor compatibility range. A net10.0 leg runs on the 10 runtime. `COREHOST_TRACE` for named-pipelines `dotnet run -c Release -f net10.0 --no-build` chose `Microsoft.NETCore.App/10.0.14`. The same sample on `net11.0` chose `Microsoft.NETCore.App/11.0.0-rc.3.26522.148` and printed `client:hello` / `server:hello` on both. `UseSharedCompilation=false dotnet build timewarp-mediator.slnx -c Release` reported 0 warnings and 0 errors. `dotnet test timewarp-mediator.slnx -c Release --no-build` passed generators-tests 57, analyzers-tests 42, and compilation-tests 69 on `net10.0` and on `net11.0`, failed 0.

M2. `./bin/dev benchmark --gate` ran on BenchmarkDotNet `0.16.0-preview.2` against `documentation/benchmarks/comparison-baseline.md` (BenchmarkDotNet `0.15.8`, commit `c5e4a3a6a6155b6f5ed322379a59559be58fc69a`). The local `./bin/dev` was refreshed with `dotnet run --file tools/dev-cli/dev.cs -- self-install` first, because the previous publish omitted `--framework` and could not select `net10.0`. The gate host and the job both reported `.NET 10.0.14`. The gate failed (exit 1). Allocated bytes matched the baseline. Two means exceeded the 50% limit:

| Method | Baseline | Limit | Measured |
| --- | ---: | ---: | ---: |
| TimeWarpGeneratedMonomorphicSend | 5.016 ns | 7.524 ns | 10.509 ns |
| TimeWarpGeneratedMonomorphicSendUnit | 3.885 ns | 5.828 ns | 8.063 ns |

The other nine `TimeWarp*` rows stayed inside the limit. `comparison-baseline.md` was not rewritten. Task 023-001 still owns the move off `0.16.0-preview.2`.

### Fix pass (review round 4)

M3. `Directory.Packages.props` pins BenchmarkDotNet `0.15.8` when `$(TargetFramework)` is `net10.0` and `0.16.0-preview.2` when it is `net11.0`. `dotnet list package` on `tests/timewarp-mediator-benchmarks-comparison` shows those resolved versions. `UseSharedCompilation=false dotnet build tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj -c Release` reported 0 warnings and 0 errors on both frameworks. The per-framework pin restores and builds.

`./bin/dev benchmark --gate` ran on BenchmarkDotNet `0.15.8` against `documentation/benchmarks/comparison-baseline.md`. The summary names `BenchmarkDotNet v0.15.8`, SDK `11.0.100-rc.3.26522.148`, `[Host]` `.NET 10.0.14`, and `Job-LGHQEI` `.NET 10.0.14`, warmup 3, iteration 8. The gate printed `Benchmark gate passed` and exited 0. `comparison-baseline.md` was not rewritten. The two rows that failed on `0.16.0-preview.2` are inside the 50% limit on `0.15.8`:

| Method | Baseline | Limit | 0.16.0-preview.2 | 0.15.8 |
| --- | ---: | ---: | ---: | ---: |
| TimeWarpGeneratedMonomorphicSend | 5.016 ns | 7.524 ns | 10.509 ns | 5.239 ns |
| TimeWarpGeneratedMonomorphicSendUnit | 3.885 ns | 5.828 ns | 8.063 ns | 4.042 ns |

The preview tool caused that failure. The `agent.md` benchmark row and the props comment name both pins.

Task 023-001 owns keeping the gate green. Its checklist item is: `./bin/dev benchmark --gate` is green, and `comparison-baseline.md` is re-recorded only after Steven approves the new numbers. The net11.0 pin may move to `0.16.0` stable. The net10.0 pin stays `0.15.8` unless that re-record is approved. That kitchen is on origin-home as `d544794`.

### How to validate

Smoke: `UseSharedCompilation=false dotnet run --file tools/dev-cli/dev.cs -- workflow` on SDK `11.0.100-rc.3.26522.148`.

Expect: the log ends with `Pipeline SUCCEEDED`. Release build reports 0 warnings and 0 errors. generators-tests 57 passed, analyzers-tests 42 passed, and compilation-tests 69 passed, each on `net10.0` and on `net11.0`, failed 0. The AOT sample publishes and runs on both frameworks. named-pipelines runs on both. Pack layout is verified for `TimeWarp.Mediator.15.0.0-beta.1.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.1.nupkg`. Package smoke prints `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on `net10.0` and on `net11.0`.

The same command was run again after the BenchmarkDotNet `0.16.0-preview.2` pin and the Nuru `4.0.0-beta.7` pin. It ended with `Pipeline SUCCEEDED`: build 0 warnings and 0 errors, the same test counts on both frameworks, both sample publishes and runs, both package layouts verified, and `package-pong` on both frameworks.

Smoke: `COREHOST_TRACE=1 dotnet run --project samples/timewarp-mediator-examples-named-pipelines/timewarp-mediator-examples-named-pipelines.csproj -c Release -f net10.0 --no-build`, then `./bin/dev benchmark --gate`.

Expect: named-pipelines prints `client:hello` and `server:hello`, and the apphost trace chooses `Microsoft.NETCore.App/10.0.14`. The net10.0 `runtimeconfig.json` has no `rollForward` property. The gate log names `.NET 10.0.14` for `[Host]` and `Job-LGHQEI`, then prints `Benchmark gate failed:` for `TimeWarpGeneratedMonomorphicSend` (10.509 ns, limit 7.524 ns) and `TimeWarpGeneratedMonomorphicSendUnit` (8.063 ns, limit 5.828 ns). Exit code 1. `documentation/benchmarks/comparison-baseline.md` is unchanged.

That gate failure is the BenchmarkDotNet `0.16.0-preview.2` run from the round-1 fix pass. The round-4 smoke below is the current check.

Smoke: `dotnet list tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj package`, then `./bin/dev benchmark --gate`.

Expect: the net10.0 row requests BenchmarkDotNet `0.15.8` and resolves `0.15.8`. The net11.0 row requests `0.16.0-preview.2` and resolves `0.16.0-preview.2`. The gate summary names `BenchmarkDotNet v0.15.8`, SDK `11.0.100-rc.3.26522.148`, and `.NET 10.0.14` for `[Host]` and `Job-LGHQEI`. It prints `Benchmark gate passed` and exits 0. `TimeWarpGeneratedMonomorphicSend` mean is 5.239 ns. `TimeWarpGeneratedMonomorphicSendUnit` mean is 4.042 ns. `documentation/benchmarks/comparison-baseline.md` is unchanged.

### Review

Round 1 ran at effort 2 with one general reviewer. It found 0 bugs, 1 suggestion (M1: `RollForward=LatestMajor` on the six multi-targeted hosts), and 1 nit (M2: the `--gate` run on BenchmarkDotNet 0.16 preview was not recorded). This fix pass removed that property and recorded the gate failure above.

Round 2 ran at effort 1 with one general reviewer, scoped to `e1cdd00..0448d20`. M1 and M2 are fixed. One new finding is open: M3, a suggestion. The net10.0 `--gate` is red on BenchmarkDotNet `0.16.0-preview.2` against the `0.15.8` baseline, and no task owns getting it green again. The fix is to pin `0.15.8` on net10.0, re-run the gate, and record the result. Counts so far: bug 0; suggestion 1 open and 1 fixed; nit 1 fixed. Review disposition: pending, because M3 is open, so `review/disposition.md` is not written yet. Files: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`.

Round 3 ran at effort 1 with one general reviewer, scoped to `0448d20..4563d15`. That delta is kitchen-only. `Directory.Packages.props` still pins BenchmarkDotNet `0.16.0-preview.2` for every TFM, and task 023-001 still has no item to get the gate green. M3 stays open. Counts: bug 0; suggestion 1 open and 1 fixed; nit 1 fixed. Review disposition: pending (M3 open). `review/disposition.md` is not written yet. Files: `review/round-3/general.md`, `review/round-3/merged.md`.

Implement-fix-4 addressed that suggestion. The net10.0 pin is BenchmarkDotNet `0.15.8`, the gate passed, and task 023-001 on origin-home (`d544794`) has the checklist item that keeps the gate green. Round 4 has not run.
