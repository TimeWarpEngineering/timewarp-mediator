# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: tests/timewarp-mediator-tests/variance-tests.cs:3
- Description: The summary now says notification base and interface handlers run when a derived handler is scanned with them, and that a derived notification is not delivered when only base handlers are scanned. It says the registrar does not bind a base request handler to a derived request. `BuildMediator` still registers `IRequestHandler<FormalGreeting, string>` explicitly, with a comment that the registrar will not map `Greeting` onto `FormalGreeting`. The test is `Send_explicitly_registered_derived_request_handler_should_see_runtime_type` and still expects `Hello Ada from FormalGreeting`. `DogBarkedHandler` remains in `ScannedTypes`.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Re-read the summary, the registration comment, and the renamed test. `dotnet test --filter FullyQualifiedName~VarianceTests` passed 5/5.

## Duplicates / conflicts

- No new findings. M1 carried forward from round 1.
