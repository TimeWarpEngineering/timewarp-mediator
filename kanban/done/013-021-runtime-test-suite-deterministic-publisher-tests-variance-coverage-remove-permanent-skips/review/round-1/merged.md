# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: tests/timewarp-mediator-tests/variance-tests.cs:3
- Description: The variance file says a base handler handles a derived message. On the reflection registrar that is true for notifications only when a handler for the derived type is scanned with the base handlers, and it is not true for requests unless the test registers `IRequestHandler<FormalGreeting, string>` itself. Both directions were checked by editing the new tests: base-only `Publish(DogBarked)` logged nothing, and `Send(FormalGreeting)` without the extra `AddTransient` threw `No service for type 'IRequestHandler<FormalGreeting, string>'`.
- Suggestion: Describe those rules in the file summary. Rename the request test so the explicit registration is the scenario under test.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. Nothing to collapse.
