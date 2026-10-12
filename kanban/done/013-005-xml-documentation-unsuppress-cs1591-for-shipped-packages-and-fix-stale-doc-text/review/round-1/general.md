# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `6be6bfe` vs `c295bb3` — XML docs and CS1591 `NoWarn` for `timewarp-mediator` and `timewarp-mediator-contracts`

## Summary

The change strips `CS1591` from the two shipped projects (`NoWarn` evaluates to `;CS1701;CS1702;` while analyzers and tests still suppress `CS1591`), documents the public members C-19 named, and fixes the enumerated C-18 / C-28 / C-29 text. The cancellation-token fallback comment matches both the reflection wrapper and the generator (`t == default ? cancellationToken : t`). The exception-action strategy comments match `ServiceRegistrar.AddRequiredServices` order. One rewritten summary states the wrong lifetime for scanned handlers.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-mediator/microsoft-extensions-di/service-collection-extensions.cs:7
- Description: The new class summary says scanned handler implementations are registered under `MediatorServiceConfiguration.Lifetime`. `ServiceRegistrar` registers them with `AddTransient` / `TryAddTransient` (`service-registrar.cs` lines 129, 141, and 189). `Lifetime` applies to `IMediator`, `ISender`, `IPublisher`, and a typed `INotificationPublisher` (`service-registrar.cs` lines 393–398). Parent finding C-18 only said the `IMediator` "transient" sentence was wrong; the handler bullet was already correct. A caller who sets `Lifetime` to `Singleton` still gets transient handlers.
- Suggestion: Restore the handler bullet to `ServiceLifetime.Transient`. Keep the `IMediator` / `ISender` / `IPublisher` bullet on `Lifetime`.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-mediator/pipeline/i-request-pre-processor.cs:6
- Description: The summary still says "Defined a request pre-processor for a handler". Parent C-18 evidence names this leftover. The sibling processor interfaces say "Defines".
- Suggestion: Change "Defined" to "Defines".
- Status: open
