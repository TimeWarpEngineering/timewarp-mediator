# Round 2 — general
**Date:** 2026-11-06
**Scope reviewed:** fix delta `b0c49a7..ae64cbd` (commit `ae64cbd`, "fix: read a cross-assembly handler name from handlerTypeName"), checked against round-1 `merged.md` M1–M7 and the `## Goal (implementer restatement)` acceptance bullets. Reviewer: Claude Opus 5.5 (`claude-opus-5-5`), effort 3, general axis.

## Summary

The fix pass carries out all seven round-1 findings. The getter-IL and private-reflection reader is
deleted, along with the `System.Reflection*` global usings in both projects. A cross-assembly name
now comes only from a constructor parameter named `handlerTypeName`, and its default value counts.
A metadata-only image covers that in tests, both the passing case and the getter-only TWM012 case.
TWM012 now has its own message for each of three cases: unreadable, not a type name, and not an
`IRequestHandler`. A generator host builds the compilation-end graph only when it finds a type
carrying a `GeneratedHandlerAttribute`-derived attribute. The docs, the skill, and `agent.md` match
the code. I found no new defects in the delta.

## Issues

None.

## Round-1 findings re-verified

| ID | Status | Evidence |
|----|--------|----------|
| M1 | fixed | `handler-type-name.cs`: `ReadMetadataBody` / `MethodBody` / `DecodeStringReturn` are removed. `TryReadConstructorArgument` matches the parameter name `handlerTypeName`. `DeclaredHandlerNameUnreadable` names the fix. Tests: `DeclaredHandler_FromMetadataOnlyConstructorArgument_IsDispatched` (an `EmitOptions(metadataOnly: true)` image passed to the compiler, and the full image loaded at runtime) and `DeclaredHandler_FromMetadataOnlyGetter_IsTwm012`. A grep for `Reflection.Metadata` / `GetMethodBodyOrThrow` in `source/` returns nothing. |
| M2 | fixed | `DeclaredHandler_FromRegisterSourceOutput_IsDispatched` uses a `SourceOutputHandler` generator that emits a nested `CounterState.Handler`. It asserts no problems, the declared name in `Mediator.g.cs`, and that Send returns `pong`. The post-init test is renamed `…FromPostInitializationOutput_DedupesTheVisibleHandler`. |
| M3 | fixed | `HandlerTypeNameReader.IsRequestHandler` checks `IRequestHandler<TReq,TResp>` against the request's `IRequest<T>`, or checks `IRequestHandler<TReq>` for a void request. `DeclaredHandlerNotAHandler` names the expected interface. There is one analyzer test and one compilation test. |
| M4 | fixed | `RequestHandlerAnalyzer`: in a generator host, the end action returns early unless `MessageGraphBuilder.HasGeneratedHandlerAttribute` finds an attribute. `Membership.EnumerateTypes` includes nested types, so a nested request still trips the gate. `GeneratorHost_MissingDeclaredHandler_IsTwm012` still passes. |
| M5 | fixed | Only the `handlerTypeName` parameter is read. The named-argument branch is gone. `DeclaredHandler_UnrelatedConstructorString_UsesTheGetter` covers an unrelated `description` string. |
| M6 | fixed | `DeclaredHandlerNameUnreadable`, `DeclaredHandlerNotATypeName`, and `DeclaredHandlerNotAHandler` each have their own `messageFormat` under TWM012. The generator maps `DeclaredHandlerFailure.Problem` to the matching descriptor. `NotATypeName_IsTwm012_WithoutTheMissingTypeSentence` covers it. |
| M7 | fixed | `skills/tw-mediator/SKILL.md` says "A `GeneratedHandlerAttribute`-derived attribute". The `agent.md` package rows list `GeneratedHandlerAttribute` and TWM012. |

## Verification run this round

- `dotnet test` Release, `net10.0` and `net11.0`: compilation 80/80, analyzers 50/50, generators 61/61, failed 0 on both frameworks.
- Probe `/tmp/rev029-r2/probe.cs` ran the built generator and analyzer DLLs in a generator host (`TimeWarpMediatorAssembly=true`, `TimeWarpMediatorGenerator=true`):
  - A void `IAction` nested in `CounterState`, with `CounterState.Handler : ActionHandler<Increment>` emitted from `RegisterSourceOutput` (the TimeWarp.State shape): 0 generator diagnostics, 0 compile errors, 0 analyzer diagnostics. Dispatch and registration name `global::App.CounterState.Handler`. This confirms the void branch of `IsRequestHandler` does not report a false TWM012.
  - A not-a-handler `Side`, both in source and emitted by another generator: TWM012 "Request 'App.Ping' names 'App.Side', which is not an IRequestHandler<App.Ping, string>". `Mediator.g.cs` still calls `Side`, so CS1061 follows. That pairing matches the documented missing-type case, where CS0234 or CS0246 appears beside TWM012.
  - No attribute: the gate returns early and reports nothing.

## Observations (not findings)

- The `<Description>` fields in `timewarp-mediator-analyzers.csproj` and `timewarp-mediator-generators.csproj` list TWM012 by its round-1 cases. They do not mention the not-a-type-name or not-a-handler cases. The text is incomplete, not wrong. The readmes and `diagnostics.md` carry the full list.
- The not-a-handler docs paragraph in `diagnostics.md` does not say that CS1061 also appears in `Mediator.g.cs`. The missing-type paragraph does mention CS0234/CS0246. This is optional polish.
- The fix pass did not re-run `./bin/dev workflow`. This round ran the three affected suites on both frameworks. It did not run samples or pack.
