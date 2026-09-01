# Round 1 — merged findings
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
- File: Directory.Build.props:25 (pack Version SSOT for `src/**`); source/Directory.Build.props:7 (check-version Version SSOT via TimeWarp.Nuru.DevCli `CheckVersionCommand.GetVersionFromSource`); tools/dev-cli/endpoints/workflow-command.cs:141-146 (release invokes that gate)
- Description: Release mode runs shared `check-version`, which reads `<Version>` only from `source/Directory.Build.props`. Product projects still live under `src/` and evaluate `<Version>` from root `Directory.Build.props`. The two files currently both say `13.0.0`, so smoke passes, but they can drift independently until 006-003. Old CI grepped root `Directory.Build.props`; this branch gates publish on a file that does not control the nupkg version today.
- Suggestion: Until kebab/`src`→`source` (006-003), either assert root and `source/` `<Version>` are identical before the release gate, or teach the local release path to validate the Version that pack actually emits (root / project evaluation). Do not rely on accidental equality.
- Source: general
- Disposition notes: Fixed in `ee2dc97`. `AssertVersionSsot` compares root vs `source/` `Directory.Build.props` Version before shared check-version (`RepoLayout.TryReadVersion`). Mismatch or missing → ExitCode 1.

### M2 — Severity: suggestion — Status: fixed
- File: tools/dev-cli/endpoints/pack-command.cs:71-73,127-143; tools/dev-cli/endpoints/workflow-command.cs:168-176
- Description: Former `Build.ps1` deleted `.\Artifacts` before packing. `PackAsync` only `CreateDirectory`s `artifacts/packages` and never clears it; `RepoCleanService` cleans bin/obj only, not artifacts. `FindNupkg` then `OrderBy(...).FirstOrDefault()`, so a stale lower version can be layout-checked while a newer nupkg was just written. `PushAsync` pushes the whole `*.nupkg` glob, so a dirty local artifacts dir can publish leftover packages (`--skip-duplicate` only masks already-published ids). Fresh CI checkouts are unaffected.
- Suggestion: Clear `artifacts/packages` at the start of `PackAsync` (or select the newest matching nupkg and push only the versions produced by this run).
- Source: general
- Disposition notes: Fixed in `ee2dc97`. `PackAsync` deletes `artifacts/packages` recursively when present, then recreates it.

## Duplicates / conflicts

- None (single reviewer).
