# Round 2 — general
**Date:** 2026-10-13
**Scope reviewed:** uncommitted fix in skills/tw-mediator/SKILL.md against round-1 M1–M3

## Summary

The uncommitted edit to `skills/tw-mediator/SKILL.md` (289 lines, inside the 250–350 limit) corrects the three round-1 claims and does not add a false statement. Membership now distinguishes a clean host build plus `NoHandlerException` from TWM001. The TWM008 row keeps Contracts mandatory and limits the Aot escape to a missing Host-profile MS.DI Abstractions reference. The Link bullet stops at the current emitter, which treats Link like Aot.

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: skills/tw-mediator/SKILL.md:263
- Description: The gotcha now says a library without `[assembly: MediatorAssembly]` is omitted from the host graph, that `Send` throws `NoHandlerException` when those request types are outside the graph and the build stays clean, and that TWM001 fires only when the request is already in a member assembly and no member handler binds it, matching `MessageGraphBuilder`'s `MemberAssemblies` walk, `RequestHasNoHandler` at `handlerCount == 0`, and the `Send(object)` switch default.
- Status: fixed

### M2 — Severity: bug — Status: fixed
- File: skills/tw-mediator/SKILL.md:225
- Description: The TWM008 row now says Contracts is always required and that Aot is only the escape for a missing Host-profile MS.DI Abstractions reference, matching `RequiredTypes` (`TimeWarp.Mediator.IMediator`) versus `HostRequiredTypes` in `FindMissingRuntime` and the TWM008 text in `documentation/diagnostics.md`.
- Status: fixed

### M3 — Severity: suggestion — Status: fixed
- File: skills/tw-mediator/SKILL.md:194
- Description: The Link bullet now says it emits the same as Aot, with the same limits and diagnostics, and no longer reserves call-site inlining, matching `IsAotProfile` (true for both `Aot` and `Link`) and `MediatorEmitter.Emit` skipping `AddGeneratedMediator` on that flag.
- Status: fixed

## Issues

No new issues.
