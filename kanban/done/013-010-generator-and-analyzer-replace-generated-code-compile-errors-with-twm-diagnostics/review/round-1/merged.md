# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-mediator-analyzers/message-graph-builder.cs:209
- Description: Pipeline scope markers are not passed through `AccessGuard`. An `internal` `[MediatorScope]` marker (including an assembly-level marker) in a referenced member assembly is still bound, and the emitter names it on `ISender<TScope>` / `IPublisher<TScope>`, which is CS0122. Handlers, requests, responses, notifications, and behavior definitions are already checked.
- Suggestion: If the emitted scope (`handlerScope ?? messageScope`) is not accessible, report TWM007 and skip that request or notification binding. Cover the internal marker and the `InternalsVisibleTo` case in `GeneratorDiagnosticsTests`.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. No overlaps.
