# Round 1 — general
**Date:** 2026-11-14
**Scope reviewed:** branch `task/040-refresh-outdated-nu-get-packages-in-time-warp` vs `master`. Pins in `Directory.Packages.props`, `<Version>` in `source/Directory.Build.props`, the Nuru version comments in `agent.md` and `tools/dev-cli/endpoints/workflow-command.cs`, the audit `--fix` Purpose regions in `.githooks/*.cs`, and the dev CLI TWA0038 silence those Nuru pins still sit under.

## Summary

The listed pin moves that the restatement keeps are in place: SourceGenerators `1.0.0-beta.16`, Nuru and Nuru.DevCli `4.0.0-beta.13`, the three Roslyn packages `5.12.0`, MediatR `15.0.0`, and `<Version>` `15.0.0-beta.8` (hyphen, not plus). Amuru `3.0.0-beta.2`, Terminal `2.0.0-beta.1`, and Build.Tasks `2.0.1-beta.1` match the Nuru `4.0.0-beta.13` nuspec and the recorded package-pin-currency follow-up. The recorded build, 382 tests, and clean audit cover that diff. The Nuru.DevCli bump also retires the reason for the dev CLI TWA0038 silence, and that silence is still on.

## Issues

### Issue 1 — Severity: bug
- File: tools/dev-cli/dev.cs:7
- Description: `TimeWarp.Nuru.DevCli` moves to `4.0.0-beta.13` in this diff. The silence at `tools/dev-cli/dev.cs:3-7` (`#:property NoWarn=$(NoWarn);TWA0038`) and the baseline entry `.timewarp/suppression-baseline.json:60-61` (`TWA0038: 1`) still turn the 500-line cap off for the whole dev CLI runfile. The comment says the cap fires on `content/any/endpoints/release-command.cs` and `content/any/services/nuget-version-service.cs`, and that timewarp-mediator task 037 removes the line after timewarp-nuru task 576 ships. In the cached `4.0.0-beta.13` package those files are 356 and 411 lines. No file under `tools/dev-cli/` or under that package's `content/` is 450 lines or longer. The check stays silenced with no remaining over-cap file. Targeted command: `wc -l` on those two cache paths, then the same count for every `.cs` file under `tools/dev-cli/` and under `~/.nuget/packages/timewarp.nuru.devcli/4.0.0-beta.13/content`. Reason: the pin bump looked like it retired the suppression's premise. This was not a build, a test run, or an audit.
- Suggestion: Remove the `NoWarn` line and its comment from `tools/dev-cli/dev.cs`, and remove the `tools/dev-cli/dev.cs` / `TWA0038` entry from `.timewarp/suppression-baseline.json`. `dotnet run --file tools/dev-cli/dev.cs -- self-install` has to succeed with TWA0038 active. If some other injected file is still over the cap, name that file in the comment instead of a blanket silence. Do that on this task id. Task 037 already describes the same removal; do not open a sibling task.
- Status: open
