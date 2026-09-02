# Round 2 — merged findings
**Date:** 2026-09-02
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/endpoints/workflow-command.cs:144-188; tools/dev-cli/services/repo-layout.cs:41-54
- Description: Release mode ran shared `check-version`, which reads `<Version>` only from `source/Directory.Build.props`, while pack evaluates root `Directory.Build.props`.
- Suggestion: Assert root and `source/` `<Version>` are identical before the release gate.
- Source: general (round 1)
- Disposition notes: Re-verified. `AssertVersionSsot` runs after pack and before shared check-version/push. `RepoLayout.TryReadVersion` matches `CheckVersionCommand.GetVersionFromSource`. Both props files read `13.0.0`. Mismatch or missing sets ExitCode 1 and aborts.

### M2 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/endpoints/pack-command.cs:70-78
- Description: `PackAsync` did not clear `artifacts/packages`, so stale nupkgs could be layout-checked or pushed.
- Suggestion: Clear `artifacts/packages` at the start of `PackAsync`.
- Source: general (round 1)
- Disposition notes: Re-verified. `PackAsync` recursively deletes `artifacts/packages` when present, then recreates it.

## Duplicates / conflicts

- None. No new findings. Prior M# IDs carried forward as fixed.
