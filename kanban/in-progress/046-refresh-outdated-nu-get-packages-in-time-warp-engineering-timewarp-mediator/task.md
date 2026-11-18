# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-mediator

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-mediator.
Filter: (all)
Outdated:
  TimeWarp.Nuru 4.0.0-beta.15 -> 4.0.0-beta.16 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Nuru.DevCli 4.0.0-beta.15 -> 4.0.0-beta.16 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Amuru 3.0.0-beta.5 -> 3.0.0-beta.10 (dev-only: tools/dev-cli/Directory.Build.props)
  TimeWarp.Amuru.Tools 3.0.0-beta.5 -> 3.0.0-beta.10 (dev-only: tools/dev-cli/Directory.Build.props)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the four dev-only Dev CLI pins in `Directory.Packages.props`: TimeWarp.Nuru and
TimeWarp.Nuru.DevCli 4.0.0-beta.15 -> 4.0.0-beta.16, TimeWarp.Amuru and TimeWarp.Amuru.Tools
3.0.0-beta.5 -> 3.0.0-beta.10. Leave `<Version>` in `source/Directory.Build.props` alone.

Acceptance:
- `Directory.Packages.props` carries the four new versions; no product package changes.
- The Dev CLI and the solution build (`./bin/dev build`), and the dev CLI still self-installs.
- `ganda repo audit` passes.
- Doc/comment references that name the old Nuru version as a fact are kept accurate.

## Session

- Minted by ganda task wave run 20261118T170809Z-b6e473
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-18T18:27:06Z
- Review oracle: general (grok, session 01a1efe9-bc05-7693-8961-97389251b757), effort 1, max-turns 80, round 1 — 2026-11-19
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1efe9-bc05-7693-8961-97389251b757, max-turns 80, artifacts general.md, merged.md — 2026-11-18T18:31:53Z
- Review oracle round 2: general (grok, session 01a1efef-b34d-7442-9b9f-dc23bb05eb16), effort 1, max-turns 80, disposition clean — 2026-11-19

## Results

- `Directory.Packages.props` Dev CLI pins: TimeWarp.Nuru and TimeWarp.Nuru.DevCli
  4.0.0-beta.15 -> 4.0.0-beta.16; TimeWarp.Amuru and TimeWarp.Amuru.Tools 3.0.0-beta.5 ->
  3.0.0-beta.10. `<Version>` in `source/Directory.Build.props` unchanged.
- Amuru 3.0.0-beta.10 adds analyzer AMURU0001, which failed the dev CLI build at
  `tools/dev-cli/endpoints/benchmark-command.cs` (`Shell.Builder("dotnet")` with `publish`).
  `RunAotAsync` now uses `DotNet.Publish()` (`WithProject`, `WithConfiguration`,
  `WithFramework`, `WithRuntime`, `WithProperty("BenchmarkAot", "true")`, `WithOutput`). The
  builder has no `--nologo` option. That flag only trimmed captured output.
- `agent.md`: the Nuru fact line now says 4.0.0-beta.16, which still ships `lib/net11.0` only.

### Review

Round 1, effort 1, roster general, raised M1 (nit). Fix pass 2 updated the Design comment. Round 2, effort 1, roster general, re-read the delta since `a533117` and confirmed M1 fixed, with no new findings. Final counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0; nit 0 open / 1 fixed / 0 wontfix. Disposition: clean. No wontfix and no escalation. Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

### Gates

- `dotnet run --file tools/dev-cli/dev.cs -- self-install`: pass (the first try failed on AMURU0001; passes after the fix)
- `./bin/dev build`: pass
- Test files: none added or edited. The changed code is the dev CLI, which has no test files
  (no Jaribu runfiles under `tests/`). Smoke instead: `./bin/dev benchmark --aot`: pass (AOT
  publish with `BenchmarkAot=true` succeeded, and the benchmark table printed). The regenerated
  `comparison-baseline-aot.md` was reverted (not in scope).
- Fix pass 2 (M1, comment-only edit in `tools/dev-cli/endpoints/workflow-command.cs`): `./bin/dev build`: pass. Test files: none (dev CLI has no Jaribu runfiles). `ganda repo audit`: pass (all checks).
- `ganda repo audit`: pass with 1 advisory warning (`vscode-window-icon`: the gitignored `.vscode/settings.json` was missing). `ganda repo audit --fix` created it locally (untracked, ignored). The rerun of `ganda repo audit` passes all checks.

### How to validate

Smoke:
```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev build
./bin/dev benchmark --aot   # then: git checkout -- documentation/benchmarks/comparison-baseline-aot.md
```
Expect: self-install prints "Successfully installed dev CLI", the build reports "Build completed
successfully!", and `benchmark --aot` publishes, then prints the TimeWarp* timing table with exit 0.

## Review

- Round 1: Blocked — open M1
- Round 2: Done
