# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/timewarp-mediator.csproj:4
- Description: Runtime description claimed the source-generated mediator builds on this package. Packed nuspec now describes `AddMediator()` as the reflection dispatcher and states that `AddGeneratedMediator()` does not reference this package.
- Suggestion: Describe the reflection stack and the generated stack as separate.
- Source: general
- Disposition notes: Fixed on this task. Re-verified from the packed `TimeWarp.Mediator` nuspec.

### M2 — Severity: bug — Status: fixed
- File: source/Directory.Build.props:12
- Description: Shared tags added `source-generator` and `aot` to every package, including the reflection runtime.
- Suggestion: Append those tokens only on Generators.
- Source: general
- Disposition notes: Fixed. Packed tags: Generators only.

### M3 — Severity: suggestion — Status: fixed
- File: source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:10
- Description: Generator description attributed TWM001–TWM006 to the analyzer package.
- Suggestion: Analyzer range TWM001–TWM004; generator reports TWM005–TWM011.
- Source: general
- Disposition notes: Fixed. Packed description matches `documentation/diagnostics.md`.

## Resolved prior

- M1, M2, M3 carried from round 1. All fixed. No new IDs.

## Duplicates / conflicts

- Single reviewer. No overlaps.
