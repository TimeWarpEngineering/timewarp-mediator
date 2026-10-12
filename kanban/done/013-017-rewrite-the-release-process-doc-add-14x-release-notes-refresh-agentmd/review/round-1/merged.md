# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 2 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: agent.md:57
- Description: Shared-patterns text says both dispatchers support streams via `IStreamRequest<>`. The generated mediator throws `NotSupportedException` from `CreateStream` and reports TWM009 for stream handlers. Only the reflection `AddMediator()` path dispatches them.
- Suggestion: Describe streams as reflection-only and point at `documentation/diagnostics.md`.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: documentation/release-notes/release-notes-v14.0.0.md:70
- Description: Beta.4 notes list `GeneratedMediatorServiceCollectionExtensions` as emitted internal in every profile. Aot and Link do not emit that type (`mediator-emitter.cs`, including at tag `v14.0.0-beta.4`).
- Suggestion: Say the DI extension is Host-only.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: documentation/release-notes/release-notes-v14.0.0.md:16
- Description: Unreleased notes omit the `Unlicense AND Apache-2.0` license expression on Mediator and Contracts, and the trim/AOT annotations on the reflection entry points. Both landed after `v14.0.0-beta.4`.
- Suggestion: Add those two consumer-visible items. Do not expand the note into the full generator-fix catalog.
- Source: general
- Disposition notes:

### M4 — Severity: nit — Status: open
- File: agent.md:7
- Description: Opening sentence says "with no dependencies" while `TimeWarp.Mediator` references `Microsoft.Extensions.DependencyInjection.Abstractions`.
- Suggestion: Drop the phrase.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
