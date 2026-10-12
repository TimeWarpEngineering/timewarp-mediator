# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** fix delta for M1 and M2, plus the round-1 claims that stayed in the tree

## Summary

M1 and M2 are fixed. Scanned handlers are documented as `ServiceLifetime.Transient` again, and `IMediator` / `ISender` / `IPublisher` stay on `MediatorServiceConfiguration.Lifetime`. The pre-processor summary now says "Defines". The two-line delta introduces no new doc defect. Compiler diagnostics for the shipped projects stayed clean (warnings 0); the unrelated generator errors are in samples and generator tests that need a built generator, outside this diff.

## Issues

### M1 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/microsoft-extensions-di/service-collection-extensions.cs:7
- Description: Handler bullet now says `ServiceLifetime.Transient`, matching `AddTransient` / `TryAddTransient` in `ServiceRegistrar`.
- Suggestion: none
- Status: fixed

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-mediator/pipeline/i-request-pre-processor.cs:6
- Description: Summary now says "Defines a request pre-processor for a handler".
- Suggestion: none
- Status: fixed
