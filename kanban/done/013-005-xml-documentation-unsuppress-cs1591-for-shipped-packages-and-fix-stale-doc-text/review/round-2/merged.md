# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/microsoft-extensions-di/service-collection-extensions.cs:7
- Description: The round-1 summary said scanned handlers were registered under `MediatorServiceConfiguration.Lifetime`. They are registered with `AddTransient` / `TryAddTransient`. The bullet now says `ServiceLifetime.Transient`. `IMediator`, `ISender`, and `IPublisher` remain documented under `Lifetime`.
- Suggestion: Restore the handler bullet to `ServiceLifetime.Transient`.
- Source: general
- Disposition notes: Fixed on this task in round 2. Re-checked against `service-registrar.cs` lines 129, 141, 189, and 393–398.

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-mediator/pipeline/i-request-pre-processor.cs:6
- Description: "Defined a" is now "Defines a", matching the sibling processor summaries and parent C-18 evidence.
- Suggestion: Change "Defined" to "Defines".
- Source: general
- Disposition notes: Fixed on this task in round 2.

## Duplicates / conflicts

- Single reviewer. Prior IDs M1 and M2 carried forward. No new findings.
