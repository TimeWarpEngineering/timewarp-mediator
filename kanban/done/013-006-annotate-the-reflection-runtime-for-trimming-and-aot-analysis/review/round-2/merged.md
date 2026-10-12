# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 1 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: wontfix
- File: source/timewarp-mediator/mediator.cs:46
- Description: Entry-point attributes do not cover `Send`, `Publish`, `CreateStream`, or behavior `Handle`. See round 1.
- Suggestion: Annotate those members without annotating the contracts interfaces.
- Source: general
- Disposition notes: Wontfix confirmed in round 2. `ISender`, `IPublisher`, and `IPipelineBehavior` are still unannotated, and the reflection implementations were not given the attributes. IL2046 requires a match. Annotating the contracts would mark the generated mediator unsafe. No alternative surfaces a call-site warning on interface dispatch without that.

### M2 — Severity: nit — Status: fixed
- File: source/timewarp-mediator/internal/trimming-messages.cs:11
- Description: Trim messages attributed `AddGeneratedMediator()` to the Generators assembly.
- Suggestion: Name the host-emitted extension and the Contracts reference.
- Source: general
- Disposition notes: Fixed. Round 2 confirmed both sentences now say to reference `TimeWarp.Mediator.Contracts` and the `TimeWarp.Mediator.Generators` source generator, which emits `AddGeneratedMediator()` into the host. That matches `mediator-emitter.cs`.

## Resolved prior

- M1 — suggestion — wontfix (confirmed)
- M2 — nit — fixed (confirmed)

## Duplicates / conflicts

- No new findings. Prior IDs carried forward.
