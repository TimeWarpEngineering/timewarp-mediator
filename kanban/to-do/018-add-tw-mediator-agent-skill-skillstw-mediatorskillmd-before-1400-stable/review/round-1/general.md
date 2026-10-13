# Round 1 — general
**Date:** 2026-10-13
**Scope reviewed:** branch task/018 vs origin/master, commit b2897eb — skills/tw-mediator/SKILL.md, readme.md

## Summary

The change adds a 288-line `skills/tw-mediator/SKILL.md` and a readme "For AI agents" pointer. The six required sections, the generated-vs-reflection table, Host/Aot registration, and the 13.x→14.0 bullets match `generated-vs-legacy.md`, `migration.md`, and the emitter. Two claims will send an agent the wrong way: a library that forgets membership is not always TWM001, and TWM008's Aot escape applies only when MS.DI is the missing piece. Link is also described with an archive-only reservation the current emitter does not implement.

## Issues

### Issue 1 — Severity: bug
- File: skills/tw-mediator/SKILL.md:264
- Description: The gotcha says handlers in a library without `[assembly: MediatorAssembly]` are invisible and the symptom is TWM001 on the request, not a runtime error. TWM001 is only for a concrete request that itself sits in a member assembly and has no bound handler (`MessageGraphBuilder` walks `membership.MemberAssemblies` only, then reports `RequestHasNoHandler` when `handlerCount == 0`). A referenced assembly joins that set only with `[assembly: MediatorAssembly]` or a host `[assembly: MediatorAssemblies]` marker (`Membership.TryCreate`). When the request and its handler both live in the unmarked library, the host does not see the request, so there is no TWM001. `MediatorAssembliesMarker_LinksAnAttributeFreeLibrary` compiles that unlinked host with an empty problem list and leaves `Plain.Lookup` out of the manifest. `ISender.Send` of that request binds to the generic send, which calls `Send(object)` and throws `NoHandlerException` from the switch default (`mediator-emitter.cs`). TWM001 does fire when the request is already in a member assembly (typically the host) and only the handler was left outside the graph. An agent that colocates requests and handlers in a library, forgets the attribute, and trusts a green build will ship a host that throws at `Send`.
- Suggestion: Say that an unmarked library is omitted entirely. If its request types are also outside the graph, the host builds and `Send` throws `NoHandlerException`. TWM001 is the symptom only when the request type is in a member assembly and no handler in a member assembly binds it. Point at the membership section in `generated-vs-legacy.md`.
- Status: open

### Issue 2 — Severity: bug
- File: skills/tw-mediator/SKILL.md:226
- Description: The TWM008 fix is "Add the reference, or set the profile to `Aot`" for both "Contracts or MS.DI Abstractions not referenced." The Aot alternative applies only when MS.DI is the missing piece. `FindMissingRuntime` always requires `TimeWarp.Mediator.IMediator`, and the hint for that gap is only "Reference the TimeWarp.Mediator.Contracts package". `IServiceCollection` and the two `Service*Extensions` types are required only when the profile is not Aot or Link, and only that hint mentions setting `TimeWarpMediatorProfile` to Aot (`mediator-generator.cs`). `diagnostics.md` says the same: add the reference, or set the profile to Aot when you do not use MS.DI. Switching to Aot while Contracts is unresolved still emits nothing and reports TWM008 again.
- Suggestion: Split the fix. Missing Contracts: add `TimeWarp.Mediator.Contracts`. Missing MS.DI Abstractions under Host: add the reference, or set the profile to Aot (or Link) when the host does not use MS.DI.
- Status: open

### Issue 3 — Severity: suggestion
- File: skills/tw-mediator/SKILL.md:194
- Description: "Today emits the same as Aot" is right. "Reserved for call-site inlining" is not current product behavior. `MessageGraph.IsAotProfile` is true for both `Aot` and `Link`, and `MediatorEmitter.Emit` skips `AddGeneratedMediator` for that flag with no Link-only interceptor pass. `generated-vs-legacy.md` describes Link only as the same limits as Aot (no `IServiceProvider`, behaviors not woven, `new Mediator()`). Call-site inlining appears in `documentation/archive/`, not in the consumer docs. The emitter comment about a `Dispatch_*` prototype is on every profile, on methods of the internal `Mediator` class, and is not a Link feature.
- Suggestion: Stop at "today emits the same as Aot (same limits, same diagnostics)." Drop the reservation, or point at the Aot/Link limits section and do not name an unimplemented profile.
- Status: open
