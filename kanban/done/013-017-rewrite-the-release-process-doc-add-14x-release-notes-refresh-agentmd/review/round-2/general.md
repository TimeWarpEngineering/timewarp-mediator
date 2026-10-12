# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** fix delta on `agent.md` and `documentation/release-notes/release-notes-v14.0.0.md` after round 1. Re-checked M1–M4 against `mediator-emitter.cs`, `message-graph.cs` (`IsAotProfile` includes Aot and Link), `service-collection-extensions.cs`, `mediator.cs`, the exception-processor behaviors, and the four package license expressions.

## Summary

The four round-1 findings are fixed. Streams are described as reflection-only, the beta.4 note limits `AddGeneratedMediator()` to the Host profile, the unreleased section includes the license expression and the trim annotations, and the opening sentence no longer says the library has no dependencies. The fix delta does not add a new defect.

## Issues

### M1 — Severity: bug — Status: fixed
- File: agent.md:57
- Description: Shared patterns now say `CreateStream` / `IStreamRequest<>` are reflection-only and cite TWM009. `IPipelineBehavior<,>` is limited to the reflection runtime and the generated Host profile.
- Suggestion: Done.
- Status: fixed

### M2 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release-notes-v14.0.0.md:84
- Description: Beta.4 notes now say Aot and Link do not emit `GeneratedMediatorServiceCollectionExtensions`. That matches `Emit` skipping the file when `IsAotProfile` is set, and `IsAotProfile` is true for both Aot and Link.
- Suggestion: Done.
- Status: fixed

### M3 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release-notes-v14.0.0.md:30
- Description: Unreleased notes now record `Unlicense AND Apache-2.0` on Mediator and Contracts (Generators and Analyzers stay `Unlicense`) and the `RequiresUnreferencedCode` / `RequiresDynamicCode` annotations on both `AddMediator` overloads, the `Mediator` constructors, and the two exception-processor behavior constructors.
- Suggestion: Done.
- Status: fixed

### M4 — Severity: nit — Status: fixed
- File: agent.md:7
- Description: "with no dependencies" is gone. The package table still names the DependencyInjection.Abstractions reference on the reflection package.
- Suggestion: Done.
- Status: fixed
