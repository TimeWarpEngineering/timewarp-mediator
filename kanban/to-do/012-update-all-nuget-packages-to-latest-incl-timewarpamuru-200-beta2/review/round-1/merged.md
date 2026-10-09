# Round 1 — merged findings
**Date:** 2026-10-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 1 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: Directory.Packages.props:31; source/timewarp-mediator-{analyzers,generators}/*.csproj:24-27
- Description: The shipped analyzers and generators compiled against Microsoft.CodeAnalysis.CSharp 5.9.0. Older SDK/VS hosts would raise CS9057, and the generator would not run.
- Suggestion: Hold the shipped Roslyn components at a low compiler floor with VersionOverride.
- Source: general
- Disposition notes: Fixed. `VersionOverride="4.8.0"` on Microsoft.CodeAnalysis.CSharp in both shipped component csprojs. project.assets.json resolves 4.8.0. Test projects (Workspaces) stay on central 5.9.0. Build exits 0, 212 tests pass and 2 are skipped, `./bin/dev pack` layout is verified, and `ganda repo audit` is clean.

### M2 — Severity: suggestion — Status: wontfix
- File: Directory.Packages.props:29-32 (source/timewarp-mediator/timewarp-mediator.csproj)
- Description: The packable library's dependency floor rose to M.E.DI.Abstractions and Bcl.AsyncInterfaces 10.0.12. `net6.0` is outside 10.x support, and the agent.md Key Dependencies section was stale.
- Suggestion: Keep the library floor low via VersionOverride, or accept the raise and update the docs.
- Source: general
- Disposition notes: Accepted (orchestrator). The task directive is "latest" across packages. These are runtime-library pins, and their `net6.0` consumers still resolve the netstandard2.0 asset (a warning only, documented in task.md Notes). Unlike M1, this does not silently break consumers. agent.md Key Dependencies was updated to 10.0.12. Dropping `net6.0` and the release-note wording are left to the release (version-bump) work.

### M3 — Severity: nit — Status: fixed
- File: four csproj comments ("pinned to 5.9.0")
- Description: Hard-coded versions in comments would go stale.
- Suggestion: Drop the literal version.
- Source: general
- Disposition notes: Fixed. Versions were removed from the comments, and the shipped components now carry a comment explaining why the floor matters.

## Duplicates / conflicts

- None.
