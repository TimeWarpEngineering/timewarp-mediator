# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-mediator

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-mediator.
Filter: (all)
Outdated:
  TimeWarp.SourceGenerators 1.0.0-beta.14 -> 1.0.0-beta.16 (patch)
  TimeWarp.Nuru 4.0.0-beta.12 -> 4.0.0-beta.13 (patch)
  TimeWarp.Nuru.DevCli 4.0.0-beta.12 -> 4.0.0-beta.13 (patch)
  Microsoft.CodeAnalysis.Analyzers 5.9.0 -> 5.12.0 (minor)
  Microsoft.CodeAnalysis.CSharp 5.9.0 -> 5.12.0 (minor)
  Microsoft.Extensions.DependencyInjection 10.0.15 -> 11.0.0 (major)
  Microsoft.CodeAnalysis.CSharp.Workspaces 5.9.0 -> 5.12.0 (minor)
  MediatR 14.3.0 -> 15.0.0 (major)
  Microsoft.Extensions.Logging 10.0.15 -> 11.0.0 (major)
Edit Directory.Packages.props in this repo only.

Version: set <Version> in source/Directory.Build.props from 15.0.0-beta.7 to 15.0.0-beta.8 in this pull request. The prerelease suffix is -beta.N, never +beta.N. The wave releases it after merge.

## Goal (implementer restatement)

Bring the central package pins in `Directory.Packages.props` up to date and bump the product
version so the wave can release it.

Acceptance criteria:

- `TimeWarp.SourceGenerators` 1.0.0-beta.16, `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.13.
- `Microsoft.CodeAnalysis.Analyzers`, `.CSharp`, and `.CSharp.Workspaces` 5.12.0. The SDK in
  `global.json` (11.0.100) ships Roslyn 5.12.0, so `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` still holds.
- `MediatR` 15.0.0 (benchmark comparison only); benchmarks still build.
- `Microsoft.Extensions.DependencyInjection` / `Logging`: the net11.0 rows are already 11.0.0. The
  net10.0 rows stay on the 10.0.x line on purpose (a net10.0 host must not restore an 11 package),
  unless the audit says otherwise.
- `<Version>` in `source/Directory.Build.props` goes 15.0.0-beta.7 -> 15.0.0-beta.8.
- `./bin/dev build`, `./bin/dev test`, and `ganda repo audit` pass (audit failures such as other
  stale pins are fixed here too).
- Fix pass (review round 1, M1): `TimeWarp.Nuru.DevCli` 4.0.0-beta.13 has no content file at or over
  the 500-line cap, so the dev CLI's TWA0038 silence (`#:property NoWarn` in `tools/dev-cli/dev.cs`)
  and its `.timewarp/suppression-baseline.json` entry go. `self-install` must build with TWA0038 active.
  This does task 037's work on this id; no sibling task.

## Session

- Minted by ganda task wave run 20261114T054552Z-9a0f39
- Implementer (Claude Opus 5.5, 2026-11-14): merged origin/master (kanban-only), refreshed pins, bumped
  version, rebuilt the dev CLI with `self-install`, ran the gates.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session b9fd91d6-173b-485b-9193-961f5ded1f3d, max-turns 500, Done — 2026-11-14T09:00:30Z
- Review oracle round 1: grok 4.7 (xAI, headless), effort 1, roster general, max-turns 80, session 01a1d949-8539-7c52-adfb-bda69301a4aa. Result: Blocked, fix pass needed for open finding M1 — 2026-11-14
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1d949-8539-7c52-adfb-bda69301a4aa, max-turns 80, artifacts general.md, merged.md — 2026-11-14T09:09:23Z
- Fix pass (implement-fix-2, Claude Opus 5.5, 2026-11-14): fixed M1. Removed the TWA0038 `NoWarn` line and
  its comment from `tools/dev-cli/dev.cs` and the matching baseline entry, cleared the runfile cache, and
  rebuilt the dev CLI with `self-install`. Reran build, test, and audit.

## Results

Pins changed in `Directory.Packages.props`:

| Package | From | To | Note |
|---|---|---|---|
| TimeWarp.SourceGenerators | 1.0.0-beta.14 | 1.0.0-beta.16 | |
| TimeWarp.Nuru / TimeWarp.Nuru.DevCli | 4.0.0-beta.12 | 4.0.0-beta.13 | dev CLI |
| TimeWarp.Amuru / TimeWarp.Amuru.Tools | 2.0.0 | 3.0.0-beta.2 | Nuru 4.0.0-beta.13 depends on Amuru 3.0.0-beta.2, and the audit's package-pin-currency check flagged it |
| TimeWarp.Terminal | 1.0.2 | 2.0.0-beta.1 | Nuru 4.0.0-beta.13 depends on it, and the same audit check flagged it |
| Microsoft.CodeAnalysis.Analyzers / .CSharp / .CSharp.Workspaces | 5.9.0 | 5.12.0 | The SDK 11.0.100 compiler is Roslyn 5.12.0 (`csc.deps.json`), so the referenced-compiler test still holds |
| MediatR | 14.3.0 | 15.0.0 | benchmark comparison only; compiles unchanged |
| TimeWarp.Build.Tasks | 2.0.0 | 2.0.1-beta.1 | published 2026-11-14 after the first audit; the post-commit attest flagged it (package-pin-currency) |

Left as is: `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` stay on
10.0.15 in the `net10.0` rows on purpose: a net10.0 host must not restore an 11 package. The
`net11.0` rows were already 11.0.0. The audit does not flag them.

Also changed:

- `source/Directory.Build.props` `<Version>` 15.0.0-beta.7 -> 15.0.0-beta.8.
- `agent.md` and the `tools/dev-cli/endpoints/workflow-command.cs` comment now name Nuru 4.0.0-beta.13.
- Audit boy-scout: `ganda repo audit --fix` refreshed the outdated memsearch hooks in `.githooks/*.cs`
  (memsearch-scaffold warning). It also set the VS Code `window.title`, but `.vscode/settings.json`
  is git-ignored, so that fix stays local.

- Review fix M1: `tools/dev-cli/dev.cs` no longer sets `#:property NoWarn=$(NoWarn);TWA0038`, and
  `.timewarp/suppression-baseline.json` no longer lists `tools/dev-cli/dev.cs`. In `TimeWarp.Nuru.DevCli`
  4.0.0-beta.13, `release-command.cs` is 356 lines and `nuget-version-service.cs` is 411 lines. The largest
  `.cs` file under `tools/dev-cli/` plus that package's `content/` is `attestation-verifier.cs` at 441 lines.
  The dev CLI now builds with TWA0038 active. Task 037 (to-do) asked for this same removal and is now
  satisfied by this task; close it on the board after merge.

### How to validate

Smoke: `ganda runfile cache --clear && dotnet run --file tools/dev-cli/dev.cs -- self-install && ./bin/dev --help`,
then `./bin/dev build && ./bin/dev test`, then `grep -n TWA0038 tools/dev-cli/dev.cs .timewarp/suppression-baseline.json`.
Expect: `self-install` publishes the dev CLI on Nuru beta.13 / Amuru 3 with TWA0038 active and prints its help.
The build has 0 warnings and 0 errors. Tests pass (generators 61, analyzers 50, compilation 80, each on
net10.0 and net11.0). The grep finds nothing.

### Gates

- `./bin/dev build`: passed, 0 warnings, 0 errors.
- `./bin/dev test`: passed, 382/382 (61+50+80 on net10.0 and on net11.0).
- `ganda repo audit`: passed, with 2 advisory warnings (memsearch-scaffold, vscode-window-icon).
  `ganda repo audit --fix` then fixed both: 43 passed, 0 failed, 3 skipped.
- The post-commit attest then failed on package-pin-currency (TimeWarp.Build.Tasks 2.0.1-beta.1 was
  published after that audit). After that pin was bumped, the gates were rerun:
  - `./bin/dev build`: passed, 0 warnings, 0 errors.
  - `./bin/dev test`: passed, 382/382.
  - `ganda repo audit`: passed all checks (43 passed, 0 failed, 3 skipped).
- Fix pass (M1), after the last product edit:
  - `dotnet run --file tools/dev-cli/dev.cs -- self-install` (runfile cache cleared first): succeeded with TWA0038 active.
  - `./bin/dev build`: passed, 0 warnings, 0 errors.
  - `./bin/dev test`: passed, 382/382 (61+50+80 on net10.0 and on net11.0).
  - `ganda repo audit`: "Repository passes all audit checks." (`source-file-line-cap`: no C# file exceeds the 500-line cap).

### Review

Round 1 (effort 1, roster general) is open. One bug, no suggestions, no nits. M1: `tools/dev-cli/dev.cs:7` still sets `#:property NoWarn=$(NoWarn);TWA0038`, and `.timewarp/suppression-baseline.json:60-61` still records it, after `TimeWarp.Nuru.DevCli` `4.0.0-beta.13` put `release-command.cs` at 356 lines and `nuget-version-service.cs` at 411 lines. Disposition is not written. Files: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Learnings

- Misread (Extensions pins): the wave task lists `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` as `10.0.15 -> 11.0.0`. The restatement keeps the `net10.0` rows at `10.0.15` and treats the `net11.0` rows, already `11.0.0`, as done. The implementation follows that restatement and the standing split in `Directory.Packages.props` (a net10.0 host stays on the 10.0.x line; task 036 recorded the same outdated pair as ignored by design). `10.0.15` is the newest 10.0.x in the local NuGet cache. `11.0.0` does ship a `net10.0` asset, so the split is policy, not a missing lib folder. No finding: applying the wave arrow would put the 11 package on the net10.0 target, which this repo has already refused.

## Review

- Round 1: Blocked — open M1
- Fix pass for round 2: M1 fixed (suppression and baseline entry removed; see Results).
