# Round 1 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: Directory.Packages.props:27 (also `documentation/release-notes/15.0.0-beta.6.md:7` and task Results)
- Description: `TimeWarp.Nuru.DevCli` is pinned at `4.0.0-beta.11`, but nuget.org published `4.0.0-beta.12` at 2026-11-09T08:54:04Z. A fresh `ganda nuget outdated --no-cache` reports this pin as outdated. The task requires every prerelease pin to be on the latest prerelease. The release note and Results say this pin is current.
- Suggestion: Move the pin to `4.0.0-beta.12`. Run `dotnet run --file tools/dev-cli/dev.cs -- self-install`, then `./bin/dev workflow` and `./bin/dev check-version --strict`. Update the release note and Results. If DevCli beta.12 does not work with `TimeWarp.Nuru` `4.0.0-beta.11` (the latest Nuru), keep beta.11, write down that evidence, and mark this `wontfix`.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: documentation/release-notes/15.0.0-beta.6.md:5
- Description: In "remove that reference so the analyzer does not run", "that reference" reads as the `Microsoft.CodeAnalysis.CSharp` 4.11.0 dependency, but the projects remove the `TimeWarp.SourceGenerators` reference. "The analyzer" is also unclear.
- Suggestion: Name `TimeWarp.SourceGenerators` and "its generators" explicitly.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. There was a single reviewer.
