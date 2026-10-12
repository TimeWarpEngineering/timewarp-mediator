# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator-analyzers/message-graph-builder.cs:211
- Description: Inaccessible pipeline scope markers were still bound and named by generated `ISender<TScope>` / `IPublisher<TScope>` (CS0122).
- Suggestion: Skip that binding and report TWM007. Cover the internal marker and `InternalsVisibleTo`.
- Source: general
- Disposition notes: Fixed on this task. `PipelineScopeAllowed` guards both request and notification bindings. `GeneratorDiagnosticsTests` covers the internal assembly-level marker (one TWM007, no CS0122, handlers absent from the manifest) and the `InternalsVisibleTo` case (handlers and marker bound).

## Resolved prior

- M1 confirmed fixed. No new findings.

## Duplicates / conflicts

- Single reviewer. No overlaps.
