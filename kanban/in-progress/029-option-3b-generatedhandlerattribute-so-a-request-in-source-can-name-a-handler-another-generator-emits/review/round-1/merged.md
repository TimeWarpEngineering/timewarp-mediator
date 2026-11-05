# Round 1 — merged findings
**Date:** 2026-11-06
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-mediator-analyzers/handler-type-name.cs:91 (`ReadMetadataBody` / `MethodBody` :381)
- Description: A derived attribute in a referenced assembly is read from its getter's IL. A ProjectReference build hands csc the reference assembly, where that body is `ldnull; throw`. The result is TWM012 "not a constant string" and no dispatch. A probe with an `EmitOptions(metadataOnly: true)` library reproduced it. The read also relies on private reflection into Roslyn internals.
- Suggestion: Make a constructor argument (a default value is fine) the documented contract for an attribute declared in another assembly. Make the TWM012 detail name that fix. Add a test against a metadata-only library image. Drop the IL and private-reflection path, or demote it to an undocumented best effort.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: tests/timewarp-mediator-generators-compilation-tests/generated-handler-tests.cs:20, :176–182
- Description: The "from another generator" tests use `RegisterPostInitializationOutput`, which the mediator generator can see. No test covers a successful Send to a handler emitted from `RegisterSourceOutput`, the invisible `TypeModels.Declare` path. That path works today (probe) but has no test.
- Suggestion: Add an `RegisterSourceOutput` emitter test, ideally with a nested `CounterState.Handler`, that asserts no errors, the declared name, and the Send result. Rename the post-init test to say it covers the visible, same-type dedupe case.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-analyzers/message-graph-builder.handlers.cs:87
- Description: A declared name that resolves to an existing type that does not implement the request's `IRequestHandler` gets no TWM. The failure is a bare CS1061 or CS0029 in `Mediator.g.cs`.
- Suggestion: In the analyzer's post-generation pass, check the interface and report TWM012 with a "not an IRequestHandler<…>" detail. Add a test.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-analyzers/request-handler-analyzer.cs:58
- Description: A generator host now builds the full MessageGraph at compilation end on every build, even when it does not use the feature. Previously the graph was built only when TWM004 needed it.
- Suggestion: Build the graph only when some type carries a `GeneratedHandlerAttribute`-derived attribute, or run a narrow declared-handler walk in a generator host.
- Source: general
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-analyzers/handler-type-name.cs:71 (named-argument branch :59)
- Description: The first string constructor argument of any constructor is taken as the handler name, so an unrelated string parameter is misread. The named-argument branch cannot run, because an override of a get-only abstract property cannot add a setter.
- Suggestion: Take the constructor argument only when the parameter is named `handlerTypeName`, and document that. Remove the dead branch or explain why it stays.
- Source: general
- Disposition notes:

### M6 — Severity: nit — Status: open
- File: source/timewarp-mediator-analyzers/diagnostic-descriptors.cs:97
- Description: Unreadable and not-a-type-name TWM012 use the missing-type message, so the text reads "names generated handler '….HandlerTypeName (not a constant string)', which does not exist after generation".
- Suggestion: Use a message format for each case under the same id, and name the fix from M1.
- Source: general
- Disposition notes:

### M7 — Severity: nit — Status: open
- File: skills/tw-mediator/SKILL.md:28; agent.md:22, :45
- Description: The SKILL says "`[GeneratedHandler]` on the request", but the attribute is abstract. `agent.md` still lists only the TWM001–TWM004 analyzer and omits `GeneratedHandlerAttribute`.
- Suggestion: Say "a `GeneratedHandlerAttribute`-derived attribute on the request", and update the `agent.md` rows.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- There was one reviewer and nothing to collapse. M1 and M5 touch the same method, `TryRead`. One fix can settle both by making a `handlerTypeName` constructor parameter the contract. M6's message text should name the same fix.

## Verification run this round

- Sorted line-by-line comparison of the split files against the base: `mediator-emitter.*` is a pure move. `candidate-graph.*` and `message-graph-builder.*` differ only by the declared-handler logic.
- `dotnet test …generators-compilation-tests -c Release -f net10.0 --filter GeneratedHandler|DeclaredHandler|AddingGeneratedHandler`: 7 passed, 0 failed.
- `dotnet test …analyzers-tests -c Release -f net10.0 --filter GeneratedHandler`: 6 passed, 0 failed.
- `/tmp/rev029/refprobe.cs`, full vs metadata-only library: reproduces M1.
- `/tmp/rev029/invisible.cs`, handler emitted from `RegisterSourceOutput`: compiles with 0 errors, emits and registers `global::App.CounterState.Handler`. The basis for M2.
- The full `./bin/dev workflow` was not re-run. The implementer reports it green (task.md Results).
