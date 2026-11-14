# Round 2 — merged findings
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
- File: tools/dev-cli/dev.cs:2
- Description: Round 1: `tools/dev-cli/dev.cs` set `#:property NoWarn=$(NoWarn);TWA0038` and `.timewarp/suppression-baseline.json` recorded `TWA0038: 1` for that file, after `TimeWarp.Nuru.DevCli` `4.0.0-beta.13` put `release-command.cs` at 356 lines and `nuget-version-service.cs` at 411 lines. Round 2 re-check of `f68a938`: both the property and the baseline entry are gone. The baseline JSON is one `files` object. No `TWA0038` string remains under `tools/dev-cli/`. The recorded fix-pass gates (`self-install` with the cap active, `./bin/dev build`, `./bin/dev test` 382/382, `ganda repo audit` including `source-file-line-cap`) cover the edit. Those gates were not re-run in this round.
- Suggestion: none remaining.
- Source: general
- Disposition notes: fixed in `f68a938`. Re-verified in round 2 by reading the product delta and the recorded results. No new findings.

## Duplicates / conflicts

- One reviewer. M1 carried forward from round 1. Nothing new to collapse.
