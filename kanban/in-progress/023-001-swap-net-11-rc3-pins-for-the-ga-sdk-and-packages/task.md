# Task 023-001: Swap .NET 11 RC3 pins for the GA SDK and packages

## Description

Follow-up to 023. When .NET 11 ships (expected November 2026), replace the RC3 SDK pin, the net11.0 DependencyInjection pin, and the preview CI quality with the GA builds. 023 leaves those pins on RC3 so the net11.0 asset can be built before GA.

## Depends on

- 023

## Requirements

- `global.json`: pin SDK `11.0.100`. Drop the `rc.3` band. Keep `rollForward: latestFeature`. Drop `allowPrerelease` once the pin is a GA SDK.
- `Directory.Packages.props`: the `net11.0` `Microsoft.Extensions.DependencyInjection` pin becomes `11.0.0`. Leave the `net10.0` pin on the 10.0 line. A net10.0 host still must not restore an 11 package.
- `.github/workflows/workflow.yml`: remove `dotnet-quality: preview` from the `11.0.x` setup-dotnet step. Keep the `10.0.x` install so the net10.0 targeting pack and runtime still restore.
- BenchmarkDotNet is pinned per target framework: `0.15.8` on `net10.0` and `0.16.0-preview.2` on `net11.0`. `0.15.8` recorded `documentation/benchmarks/comparison-baseline.md`. `0.15.8` throws on a net11.0 host before it measures, so the net11.0 leg stays on the preview until `0.16.0` stable is on NuGet. Move only the `net11.0` pin off the preview. Leave the `net10.0` pin on `0.15.8`.
- Keep `./bin/dev benchmark --gate` green. That gate is the net10.0 check against `comparison-baseline.md`. On 2026-10-28 it passed on BenchmarkDotNet `0.15.8` and .NET 10.0.14, and it had failed on `0.16.0-preview.2` for the two monomorphic rows. Do not re-record `comparison-baseline.md` as part of the stable bump. A later move of the `net10.0` pin has to re-run the gate and leave it green: stay on `0.15.8`, or re-record that file only after Steven approves the new numbers.
- BenchmarkDotNet `0.16.0` stable is not on NuGet as of 2026-11-11 (latest `0.16.0-preview.2`). Leave the `net11.0` pin on the preview unless stable appears before this task lands.
- `source/Directory.Build.props`: `<Version>` becomes `15.0.0-beta.7` (Steven, 2026-11-11). Add `documentation/release-notes/15.0.0-beta.7.md` in the style of `15.0.0-beta.6.md`: .NET 11 GA SDK and packages. `./bin/dev check-version --strict` passes. The release is cut after merge with `./bin/dev release`, not in this task.
- Update `agent.md` where it names the RC3 SDK pin, the `11.0.0-rc.3.26522.148` DependencyInjection pin, and CI preview quality.
- `./bin/dev workflow` is green on the GA SDK after the swap.

## Goal (implementer restatement)

.NET 11 is GA, so the build stops depending on release-candidate bits. `global.json` pins the GA SDK `11.0.100` with `latestFeature` roll-forward and no prerelease allowance. The `net11.0` Microsoft.Extensions pins (DependencyInjection, and Logging alongside it) move to `11.0.0`. The `net10.0` pins stay on `10.0.14`. CI stops asking setup-dotnet for preview quality on `11.0.x` and keeps the `10.0.x` install. BenchmarkDotNet stays split: `0.15.8` on `net10.0`, and `0.16.0-preview.2` on `net11.0` until `0.16.0` stable reaches NuGet. It has not as of 2026-11-11. The product version becomes `15.0.0-beta.7` with release notes. `agent.md` and `documentation/overview.md` name the GA pins.

Acceptance:
- `global.json` is `11.0.100`, `rollForward: latestFeature`, no `allowPrerelease`.
- No `dotnet-quality: preview` in `.github/workflows/workflow.yml`. All three jobs still install `10.0.x` and `11.0.x`.
- `net11.0` DependencyInjection is `11.0.0` and `net10.0` stays `10.0.14`.
- `./bin/dev check-version --strict`, `./bin/dev workflow`, `./bin/dev benchmark --gate`, and `ganda repo audit` pass on the GA SDK.
- `comparison-baseline.md` is not re-recorded.

## Checklist

- [x] SDK pin is `11.0.100` and CI no longer asks for preview quality
- [x] net11.0 DependencyInjection pin is `11.0.0`; net10.0 pin stays on 10.0
- [x] net11.0 BenchmarkDotNet pin is off the preview when 0.16.0 stable exists; net10.0 stays on 0.15.8 (0.16.0 stable is not on NuGet as of 2026-11-11, so net11.0 stays on `0.16.0-preview.2`)
- [x] `./bin/dev benchmark --gate` is green. Re-record `comparison-baseline.md` only after Steven approves the new numbers
- [x] `<Version>` is `15.0.0-beta.7`, release notes added, `check-version --strict` passes
- [x] `agent.md` names the GA pins
- [x] `./bin/dev workflow` green

## Review

- Round 1: Blocked — open M1, M2

## Notes

- Parent: 023.
- 023 recorded the net11.0 comparison numbers in `documentation/benchmarks/comparison-baseline-net11.md`. That file is not the regression gate.
- Dependabot's NuGet updater cannot install the SDK that `global.json` pins (`11.0.100-rc.3`): run 41181970758 on 2026-10-31 failed with `dotnet_install: Error: Could not find .NET Core SDK with version = 11.0.100-rc.3` and then `Missing SDK, check global.json locations vs. job directories`. It had not run on NuGet since the pin landed in task 023 on 2026-10-28, so this is the first failure, not a regression from task 026. Weekly NuGet Dependabot PRs are off until this task moves `global.json` to the GA `11.0.100`. After the swap, confirm the next `Dependabot Updates` run for the `nuget` ecosystem succeeds and record its run id in Results.

## Session

- Created: 3691297 (2026-10-28)
- Spec: task 023 implementer (2026-10-28)
- Gate item: task 023 implement-fix-4 (2026-10-28). The net10.0 gate passed on BenchmarkDotNet 0.15.8. This task keeps that gate green when the net11.0 pin leaves the preview.
- Implementer (2026-11-11): swapped the RC3 pins for GA and committed b8432b5.
- Boy-scout audit fix (2026-11-11): `ganda repo audit` warned `vscode-window-icon` because `.vscode/settings.json` was missing. `ganda repo audit --fix` created it. That file is gitignored, so there is no tracked diff. The audit then passed every check.
- Hook note (2026-11-11): `git commit` failed in the `.githooks/pre-commit` runfile with `CS2012 ... pre-commit.dll ... Access ... denied`. The write goes through the shared `VBCSCompiler` server. `UseSharedCompilation=false git commit` compiled the hook in-process, and the commit went through. The repo was not changed for this.
- Implement oracle: implemented by implementer-cursor (cursor, vendor anthropic, model claude-opus-5-5-high), session not reported, max-turns uncapped (no --max-turns flag; bound 500), Done — 2026-11-11T11:15:13Z
- Review oracle round 1: reviewer-cursor (cursor, vendor google, model gemini-4-carbon-high), effort 1, roster general, max-turns uncapped (no --max-turns flag; bound 80), Blocked (fix pass needed for open findings M1, M2) — 2026-11-11
- Review oracle: review by reviewer-cursor (cursor, vendor google, model gemini-4-carbon-high), session not reported, max-turns uncapped (no --max-turns flag; bound 80), artifacts general.md, merged.md — 2026-11-11T11:25:55Z
- Implement-fix-2 (2026-11-11): fixed round 1 M1 and M2. `merged.md` marks both fixed with disposition notes. `ganda repo audit` failed only `kanban-landed`, because the kitchen in this worktree differs from origin-home until the host lands it. No product audit check failed.

## Results

The repo now builds on the .NET 11 GA SDK and packages. No RC3 pin is left in `global.json`, `Directory.Packages.props`, the workflow, or `agent.md`/`documentation/overview.md`. The recorded benchmark files are unchanged: `comparison-baseline.md` and `comparison-baseline-net11.md` still name the RC3 SDK they were measured on, and they were not re-recorded.

- `global.json`: `11.0.100`, `rollForward: latestFeature`. `allowPrerelease` is removed.
- `Directory.Packages.props`: on `net11.0`, `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` are both `11.0.0`. Logging moves with DI so the net11.0 split stays consistent. The `net10.0` pins stay at `10.0.14`. The restore of `tests/timewarp-mediator-package-smoke` resolved `Microsoft.Extensions.DependencyInjection/10.0.14` and `/11.0.0`.
- BenchmarkDotNet: `0.15.8` on `net10.0` and `0.16.0-preview.2` on `net11.0`. NuGet flat-container on 2026-11-11 lists `0.16.0-preview.2` as the newest version, with no `0.16.0` stable.
- `.github/workflows/workflow.yml`: `dotnet-quality: preview` is removed from all three `11.0.x` setup steps (ci, publish, benchmark). The `10.0.x` installs stay. The header comment names the GA pin.
- `source/Directory.Build.props`: `<Version>15.0.0-beta.7</Version>`. Release notes are in `documentation/release-notes/15.0.0-beta.7.md`. The notes say `TimeWarp.Mediator` has no NuGet dependencies, and that the Microsoft.Extensions pins in `Directory.Packages.props` serve the repo's test, benchmark, sample, and smoke hosts (review M1).
- Install pins name `15.0.0-beta.7` in `readme.md`, `source/timewarp-mediator/readme.md`, `source/timewarp-mediator-generators/readme.md`, `source/timewarp-mediator-analyzers/readme.md`, and `skills/tw-mediator/SKILL.md`. `readme.md` links the beta.7 release notes (review M2). After `./bin/dev pack`, the `readme.md` inside both `15.0.0-beta.7` `.nupkg` files names `--version 15.0.0-beta.7`, and package smoke printed `package-pong` on net10.0 and net11.0.
- Gates on the local SDK `11.0.100`:
  - `./bin/dev check-version --strict` passed: source is `15.0.0-beta.7` and NuGet has `15.0.0-beta.6`.
  - `./bin/dev workflow` printed `Pipeline SUCCEEDED`. That covers tests, AOT samples on net10.0 and net11.0, pack, and package smoke on both frameworks.
  - `ganda repo audit` passed all checks.
- `./bin/dev benchmark --gate`: the first run failed while other worktrees were compiling. Load average was about 13, and the error bars were ±10–12 ns. The failing rows were `TimeWarpGeneratedInterfaceSend` at 17.1 ns and `TimeWarpGeneratedObjectSend` at 52.3 ns. A re-run at load below 2 passed. `InterfaceSend` measured 8.58 ns against the 8.48 ns baseline, `ObjectSend` 16.85 ns against 16.55 ns, and `MonomorphicSend` 4.09 ns against 5.06 ns. Allocated bytes matched the baseline on every row. That run used BenchmarkDotNet 0.15.8 on .NET 10.0.15 and SDK 11.0.100. `comparison-baseline.md` was not re-recorded.
- Dependabot: pending. Dependabot runs on the default branch, so the first `nuget` run against `11.0.100` can only happen after merge. Record that run id here once it has run.

### How to validate

Smoke:

```bash
dotnet --version                       # 11.0.100
cat global.json                        # "11.0.100", latestFeature, no allowPrerelease
rg -n "rc\.3|dotnet-quality" global.json Directory.Packages.props .github/workflows/workflow.yml agent.md documentation/overview.md
rg -n "15\.0\.0-beta\.6" --glob '!kanban/**' --glob '!documentation/release-notes/**'
./bin/dev check-version --strict
./bin/dev workflow
unzip -p artifacts/packages/TimeWarp.Mediator.Generators.15.0.0-beta.7.nupkg readme.md | rg "version 15"
./bin/dev benchmark --gate             # run on a quiet host; timing gate is load-sensitive
```

Expect:

- The first `rg` command prints nothing.
- The `15.0.0-beta.6` `rg` prints only the `readme.md` release-notes link to `15.0.0-beta.6.md`.
- The packed generators `readme.md` names `--version 15.0.0-beta.7` for both packages.
- `check-version --strict` reports `15.0.0-beta.7` as new against NuGet `15.0.0-beta.6`.
- `workflow` ends with `Pipeline SUCCEEDED`, and package smoke prints `package-pong` for both net10.0 and net11.0.
- `benchmark --gate` prints `Benchmark gate passed.`
- On GitHub, the CI `Setup .NET 11` steps install a GA `11.0.x` without preview quality.

### Review (round 1)

Round 1 ran at effort 1 with one `general` reviewer. `## Goal (implementer restatement)` matches the original task (no misread). The `.NET 11` GA SDK pin (`11.0.100`), `Directory.Packages.props` `net11.0` pins (`11.0.0`) vs `net10.0` (`10.0.14`), CI `workflow.yml` steps, BenchmarkDotNet split, untouched `comparison-baseline.md`, and `agent.md`/`documentation/overview.md` updates all check out, and `dotnet test timewarp-mediator.slnx --no-build -c Release` passed on both `net10.0` and `net11.0` (61 + 50 + 80 tests).

Round 1 opened 2 findings (bug: 2 open / 0 fixed / 0 wontfix; suggestion: 0; nit: 0):
- M1 (bug): `documentation/release-notes/15.0.0-beta.7.md:7` falsely claims the shipped `net11.0` and `net10.0` package assets depend on `Microsoft.Extensions.DependencyInjection` (`11.0.0` / `10.0.14`); `TimeWarp.Mediator` has zero NuGet dependencies and those `Directory.Packages.props` pins are for this repo's test, benchmark, sample, and smoke hosts.
- M2 (bug): `source/timewarp-mediator/readme.md:8`, `source/timewarp-mediator-generators/readme.md:8-9` (both packed into the `.nupkg` files), `source/timewarp-mediator-analyzers/readme.md:20`, `readme.md:33-34`, and `skills/tw-mediator/SKILL.md:43-44,58` still pin `15.0.0-beta.6`, and `readme.md:133` omits the link to `documentation/release-notes/15.0.0-beta.7.md`.

Review disposition: M1 and M2 are fixed in the fix pass (see `merged.md` disposition notes). Round 2 is pending, and `review/disposition.md` is not yet written. Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.
