# Disposition — task 013-006

**Date:** 2026-10-12
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

Round 1 (general, effort 2) found no bugs. M1 is a suggestion to also annotate `Send`, `Publish`, `CreateStream`, and the exception-behavior `Handle` methods. That stays wontfix: those members implement `ISender`, `IPublisher`, and `IPipelineBehavior` in the trim-safe contracts assembly, and IL2046 requires the attributes to match. Annotating the contracts would mark the generated mediator unsafe. `AddMediator` and the constructors remain the call-site signal. M2 was a nit in the trim warning text; the messages now say the source generator emits `AddGeneratedMediator()` into the host. Round 2 confirmed the wording and raised nothing new.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M1 | suggestion | Annotating the reflection `Send` / `Publish` / `CreateStream` / `Handle` methods without the same attributes on the contracts interfaces is IL2046. Annotating those interfaces would warn the generated path. Interface dispatch cannot warn. `AddMediator` and `new Mediator(...)` stay the call-site signal. | review oracle |

## Escalations

- None.
