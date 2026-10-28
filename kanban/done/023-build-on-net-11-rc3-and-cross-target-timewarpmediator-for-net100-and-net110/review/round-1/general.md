# Round 1 — general
**Date:** 2026-10-28
**Scope reviewed:** branch vs `master` (merge base `a9761ee`), all 30 changed files

## Summary

The change does what the task asks. `TimeWarp.Mediator` and every host multi-target
`net10.0;net11.0`. The generator and analyzer stay `netstandard2.0`. DI is pinned per TFM, and
CI installs both SDKs. The pack gate requires exactly the two lib folders, and the smoke consumer
runs once per framework. The net11 benchmark goes to its own file. I checked these on the tree:

- The packed `TimeWarp.Mediator.15.0.0-beta.1.nupkg` holds `lib/net10.0`, `lib/net11.0`, one
  `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`, `notice`, `unlicense` and `readme.md`.
  The Generators layout is unchanged.
- `dotnet test timewarp-mediator.slnx -c Release --no-build` passes 57, 42 and 69 tests on each
  of net10.0 and net11.0, with 0 failures.
- A host trace on the net10.0 named-pipelines build resolves `Microsoft.NETCore.App/10.0.14`, so
  today the net10.0 leg runs on the 10 runtime.
- `FindLibTargetFrameworks` sorts ordinally, so `SequenceEqual` against
  `[net10.0, net11.0]` is sound.
- Follow-up 023-001 exists in `kanban/to-do/` on `master` and covers the SDK, DI, CI-quality and
  BenchmarkDotNet swaps.

The goal restatement matches the task. I found no misread.

The main risk is latent. It does not show up today, but it will once .NET 11 goes GA (Issue 1).

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-mediator-generators-tests/timewarp-mediator-generators-tests.csproj:5 (also `tests/timewarp-mediator-generators-compilation-tests/…csproj:5`, `tests/timewarp-mediator-analyzers-tests/…csproj:5`, `tests/timewarp-mediator-benchmarks-comparison/…csproj:6`, `samples/timewarp-mediator-examples-named-pipelines/…csproj:6`, `samples/timewarp-mediator-examples-aot/…csproj:6`)
- Description: All six multi-targeted projects keep `<RollForward>LatestMajor</RollForward>`.
  Each `net10.0` output's `runtimeconfig.json` says `"rollForward": "LatestMajor"`. That setting
  rolls to the highest installed *release* major even when 10.0 is present. Today the only 11
  runtimes are prereleases, and the host does not roll a release request onto a prerelease, so
  the net10.0 leg runs on 10.0.14 (confirmed with `COREHOST_TRACE`). Once 11.0.0 GA is installed
  (CI installs `11.0.x` in every job, and 023-001 drops the preview quality), the net10.0 test
  leg, the `dotnet run -f net10.0` sample runs, and the net10.0 benchmark gate host all move to
  the 11 runtime without any error. `workflow` stays green, but nothing exercises the `net10.0`
  runtime any more. That breaks the "tests and samples on both frameworks" acceptance, and
  023-001 does not mention it. The NativeAOT publish is not affected, and neither is the smoke
  consumer, which has no `RollForward`.
- Suggestion: Remove `<RollForward>LatestMajor</RollForward>` from these six projects. Each TFM
  now has its own leg, and CI installs both runtimes, so nothing needs the roll-forward. If it
  must stay, condition it so it never applies to `net10.0`. Either way, say in Results that a
  net10.0 leg runs on the 10 runtime.
- Status: open

### Issue 2 — Severity: nit
- File: Directory.Packages.props:54
- Description: The comparison project moved from BenchmarkDotNet `0.15.8` to `0.16.0-preview.2`.
  `--gate` now measures net10.0 with the preview harness against `comparison-baseline.md`, which
  0.15.8 recorded. `agent.md` documents the mismatch. Results do not say whether
  `./bin/dev benchmark --gate` was run after the bump, so nobody has checked whether the gate
  still passes or still means the same thing.
- Suggestion: Run `./bin/dev benchmark --gate` once on the 0.16 preview and record pass or fail
  in Results. If it cannot run, say so in Results and leave the check to 023-001.
- Status: open
