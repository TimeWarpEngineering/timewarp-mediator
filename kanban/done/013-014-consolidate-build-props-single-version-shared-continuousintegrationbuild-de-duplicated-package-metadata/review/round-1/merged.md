# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-mediator/timewarp-mediator.csproj:4
- Description: The runtime package description claims the source-generated mediator builds on this package's registration and pipeline infrastructure. Generated code uses Contracts plus DI and does not reference `TimeWarp.Mediator`. The packed nuspec carries the same sentence.
- Suggestion: Describe `AddMediator()` as the reflection dispatcher. Point `AddGeneratedMediator()` at Contracts + Generators and state that stack does not reference this package.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/Directory.Build.props:12
- Description: Shared `PackageTags` add `source-generator` and `aot` to all four packages. The reflection runtime is not AOT-safe; Contracts and Analyzers are not source generators. The packed runtime nuspec includes both tokens.
- Suggestion: Leave those tokens off the shared list. Append them only on the Generators csproj.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-generators/timewarp-mediator-generators.csproj:10
- Description: The generator description attributes TWM001–TWM006 to the bundled analyzer package. Analyzers report TWM001–TWM004. This package reports TWM005–TWM011.
- Suggestion: Name the analyzer range TWM001–TWM004 and the generator range TWM005–TWM011.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. No overlaps.
