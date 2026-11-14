# Round 1 — merged findings
**Date:** 2026-11-14
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/dev.cs:7
- Description: `TimeWarp.Nuru.DevCli` moves to `4.0.0-beta.13` in this diff. The silence at `tools/dev-cli/dev.cs:3-7` (`#:property NoWarn=$(NoWarn);TWA0038`) and the baseline entry `.timewarp/suppression-baseline.json:60-61` (`TWA0038: 1`) still turn the 500-line cap off for the whole dev CLI runfile. The comment says the cap fires on `content/any/endpoints/release-command.cs` and `content/any/services/nuget-version-service.cs`, and that timewarp-mediator task 037 removes the line after timewarp-nuru task 576 ships. In the cached `4.0.0-beta.13` package those files are 356 and 411 lines. No file under `tools/dev-cli/` or under that package's `content/` is 450 lines or longer. The check stays silenced with no remaining over-cap file. Targeted command: `wc -l` on those two cache paths, then the same count for every `.cs` file under `tools/dev-cli/` and under `~/.nuget/packages/timewarp.nuru.devcli/4.0.0-beta.13/content`. Reason: the pin bump looked like it retired the suppression's premise. This was not a build, a test run, or an audit.
- Suggestion: Remove the `NoWarn` line and its comment from `tools/dev-cli/dev.cs`, and remove the `tools/dev-cli/dev.cs` / `TWA0038` entry from `.timewarp/suppression-baseline.json`. `dotnet run --file tools/dev-cli/dev.cs -- self-install` has to succeed with TWA0038 active. If some other injected file is still over the cap, name that file in the comment instead of a blanket silence. Do that on this task id. Task 037 already describes the same removal; do not open a sibling task.
- Source: general
- Disposition notes: fixed in the round-2 fix pass (2026-11-14). Removed `#:property NoWarn=$(NoWarn);TWA0038` and its comment from `tools/dev-cli/dev.cs` and the `tools/dev-cli/dev.cs` / `TWA0038` entry from `.timewarp/suppression-baseline.json`. `self-install` built with TWA0038 active after a runfile cache clear; build, test (382/382), and `ganda repo audit` pass.

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
