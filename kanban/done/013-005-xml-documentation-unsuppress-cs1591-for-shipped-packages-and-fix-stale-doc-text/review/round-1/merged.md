# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-mediator/microsoft-extensions-di/service-collection-extensions.cs:7
- Description: The new class summary says scanned handler implementations are registered under `MediatorServiceConfiguration.Lifetime`. `ServiceRegistrar` registers them with `AddTransient` / `TryAddTransient` (`service-registrar.cs` lines 129, 141, and 189). `Lifetime` applies to `IMediator`, `ISender`, `IPublisher`, and a typed `INotificationPublisher` (`service-registrar.cs` lines 393–398). Parent finding C-18 only said the `IMediator` "transient" sentence was wrong; the handler bullet was already correct. A caller who sets `Lifetime` to `Singleton` still gets transient handlers.
- Suggestion: Restore the handler bullet to `ServiceLifetime.Transient`. Keep the `IMediator` / `ISender` / `IPublisher` bullet on `Lifetime`.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: source/timewarp-mediator/pipeline/i-request-pre-processor.cs:6
- Description: The summary still says "Defined a request pre-processor for a handler". Parent C-18 evidence names this leftover. The sibling processor interfaces say "Defines".
- Suggestion: Change "Defined" to "Defines".
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer; nothing to collapse.
