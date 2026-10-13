# Round 2 — merged findings
**Date:** 2026-10-13
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: skills/tw-mediator/SKILL.md:263
- Description: The membership gotcha now says a library without `[assembly: MediatorAssembly]` is omitted from the host graph. When its request types are outside the graph, the host build stays clean and `Send` throws `NoHandlerException`. TWM001 fires only when the request is already in a member assembly and no member handler binds it. That matches `MessageGraphBuilder` (member assemblies only) and the `Send(object)` switch default.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Re-checked against `message-graph-builder.cs`, `Membership.TryCreate`, and `mediator-emitter.cs`.

### M2 — Severity: bug — Status: fixed
- File: skills/tw-mediator/SKILL.md:225
- Description: The TWM008 row now requires Contracts in every profile. The Aot escape is only for a missing Host-profile MS.DI Abstractions reference, matching `FindMissingRuntime` (`RequiredTypes` vs `HostRequiredTypes`) and `documentation/diagnostics.md`.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Re-checked against `mediator-generator.cs` and the TWM008 section.

### M3 — Severity: suggestion — Status: fixed
- File: skills/tw-mediator/SKILL.md:194
- Description: The Link bullet now says it emits the same as Aot (same limits, same diagnostics) and no longer reserves call-site inlining. `IsAotProfile` is true for both profiles, and that flag skips `AddGeneratedMediator`.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Re-checked against `mediator-generator.cs` and `generated-vs-legacy.md`. No new findings.

## Duplicates / conflicts

- No new findings. M1–M3 carried forward from round 1.
