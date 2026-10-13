# Round 1 — merged findings
**Date:** 2026-10-13
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: skills/tw-mediator/SKILL.md:264
- Description: The membership gotcha says a library without `[assembly: MediatorAssembly]` is invisible and the symptom is TWM001, not a runtime error. TWM001 is reported only for a concrete request that itself sits in a member assembly and has no bound handler. `MessageGraphBuilder` walks `membership.MemberAssemblies` only. A referenced assembly joins that set only with `[assembly: MediatorAssembly]` or a host `[assembly: MediatorAssemblies]` marker. When the request and its handler both live in the unmarked library, the host does not see the request, the build stays clean, and `ISender.Send` throws `NoHandlerException` from the `Send(object)` switch default. TWM001 fires when the request is already in a member assembly and only the handler was left outside the graph.
- Suggestion: Say an unmarked library is omitted entirely. If its request types are also outside the graph, the host builds and `Send` throws `NoHandlerException`. TWM001 is the symptom only when the request type is in a member assembly and no member handler binds it.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: skills/tw-mediator/SKILL.md:226
- Description: The TWM008 fix says "Add the reference, or set the profile to `Aot`" for both a missing Contracts reference and a missing MS.DI Abstractions reference. `FindMissingRuntime` always requires `TimeWarp.Mediator.IMediator`; the Aot/Link profile drops only the MS.DI types. Setting the profile to Aot while Contracts is unresolved still emits nothing and reports TWM008. `diagnostics.md` limits the Aot alternative to hosts that do not use MS.DI.
- Suggestion: Missing Contracts: add `TimeWarp.Mediator.Contracts`. Missing MS.DI under Host: add Abstractions, or set the profile to Aot when the host does not use MS.DI.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: skills/tw-mediator/SKILL.md:194
- Description: "Today emits the same as Aot" matches the emitter (`IsAotProfile` is true for both `Aot` and `Link`, and that flag skips `AddGeneratedMediator` with no Link-only interceptor). "Reserved for call-site inlining" is archive design under `documentation/archive/`, not `generated-vs-legacy.md` or the emitter.
- Suggestion: Stop at "today emits the same as Aot (same limits, same diagnostics)."
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
