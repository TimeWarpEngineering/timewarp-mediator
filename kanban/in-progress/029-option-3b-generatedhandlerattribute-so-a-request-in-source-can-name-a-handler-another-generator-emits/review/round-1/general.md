# Round 1 — general
**Date:** 2026-11-06
**Scope reviewed:** the same as the framework: `1931c40..b0c49a7`, product and docs. Kanban files are excluded.

## Summary

The change adds `GeneratedHandlerAttribute`. It reads `HandlerTypeName` from a constructor argument,
a source getter, or a getter body read out of a referenced assembly's IL. A request carries the
declared handler as a fact, and that handler goes into the same handler list the emitter already
renders. The core path works. A probe with a second generator that emits the handler from
`RegisterSourceOutput`, which the mediator generator cannot see, compiled clean. It emitted
`GetRequiredService<global::App.CounterState.Handler>` and registered `AddScoped<global::App.CounterState.Handler>`.
The split files are pure moves apart from the new logic. The filtered tests pass on net10.0:
compilation 7/7 and analyzer 6/6.

The main risk is the cross-assembly name read. The State scenario puts the derived attribute in
another assembly. That read needs the getter's IL body, which a reference assembly does not have,
so a ProjectReference consumer gets TWM012 and no dispatch. The headline test also does not test
the invisible-handler case its name claims.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-mediator-analyzers/handler-type-name.cs:91 (`ReadMetadataBody`, through `MethodBody` at :381)
- Description: A derived attribute declared in a referenced assembly with `public override string HandlerTypeName => "Handler";` (the shape the docs and tests use) is read from the getter's IL. In a ProjectReference build the SDK passes csc the **reference assembly** (`obj/.../refint/*.dll`, `ProduceReferenceAssembly` defaults to true). A reference assembly's getter body is `ldnull; throw`, so `DecodeStringReturn` returns null. The result is TWM012 "not a constant string", and the request gets **no** dispatch. Probe (`/tmp/rev029/refprobe.cs`): the library is emitted twice, full and with `EmitOptions(metadataOnly: true)`, and the host runs `MediatorGenerator`:
  ```
  refOnly=False: generator diags:            Mediator.g.cs mentions PingHandler: True
  refOnly=True:  TWM012: Request 'App.Ping' names generated handler 'State.ReducerAttribute.HandlerTypeName (not a constant string)', ...   Mediator.g.cs mentions PingHandler: False
  ```
  TimeWarp.State's own tests and samples, and any solution that ProjectReferences State, will hit this. NuGet `lib/` consumers work only because they get full bodies. The IL path also uses private reflection on Roslyn internals (`ModuleMetadata.Module`, `PEModule.GetMethodBodyOrThrow`), so a Roslyn update can silently turn every cross-assembly read into TWM012.
- Suggestion: Make a constructor argument the supported contract for an attribute declared in another assembly. A parameter with a default value works, for example `public ReducerAttribute(string handlerTypeName = "Handler")`, because `AttributeData.ConstructorArguments` carries the default. Say so in `generated-handler-attribute.cs` remarks, `documentation/diagnostics.md` TWM012, the SKILL, and the release note. Make the unreadable-name TWM012 detail name that fix. Add a compilation test whose library image is emitted with `metadataOnly: true`, covering the constructor-default shape (passes) and the getter-only shape (TWM012 with the new hint). Then either drop the IL and private-reflection path or keep it as an undocumented best effort.
- Status: open

### Issue 2 — Severity: bug
- File: tests/timewarp-mediator-generators-compilation-tests/generated-handler-tests.cs:20 (and :176–182 `EmittedHandler`)
- Description: `DeclaredHandler_FromAnotherGenerator_IsDispatched` and `DeclaredHandler_ReadFromReferencedAttributeBody_IsDispatched` emit the handler with `RegisterPostInitializationOutput`. Post-init output is visible to every generator, so the mediator generator binds `PingHandler` through `IRequestHandler`, and the declared fact only takes the dedupe path. The case the feature exists for has no successful end-to-end test: a handler emitted in `RegisterSourceOutput` that the mediator generator cannot see, where `TypeModels.Declare` builds the model. `MissingDeclaredHandler_IsTwm012_OnTheAttribute` reaches `Declare` but ends in CS0234. The probe in the summary shows that path works today, but nothing in the suite would catch a regression. This is missing required coverage for the task's main requirement.
- Suggestion: Add an `EmittedHandler` variant that uses `context.RegisterSourceOutput(context.CompilationProvider, …)`, ideally with a nested `CounterState.Handler` like State's. Assert no errors, `Mediator.g.cs` contains the declared name, and `Send` returns the handler's value. Keep the post-init test, renamed so its name says it covers the visible, same-type dedupe case.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-mediator-analyzers/message-graph-builder.handlers.cs:87 (the generator twin is candidate-graph.cs:158–182)
- Description: When the declared name resolves to a type that exists but does not implement `IRequestHandler<TRequest[, TResponse]>`, no TWM diagnostic is reported. The type is added as the handler, and the failure is CS1061 or CS0029 inside `Mediator.g.cs`, the same bare-generated-code error TWM012 was added to prevent. One way to hit it is a name that resolves to the wrong nested type.
- Suggestion: In the analyzer pass, which runs after every generator, check that a declared handler symbol implements the request's handler interface. Report TWM012 with a detail such as "'X' is not an IRequestHandler<Ping, string>". Add a test.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-mediator-analyzers/request-handler-analyzer.cs:58
- Description: Before this change, a generator host registered no compilation-end action. TWM001–TWM003 belong to the generator there, and the graph was built only when a TWM004 send needed it. Now every generator host forces `graph.Value`, the full symbol walk over the compilation and its member assemblies, at compilation end on every build, even when no type carries `GeneratedHandlerAttribute`. That cost falls on hosts that never use the feature, which goes against "Additive: no change for requests without the attribute."
- Suggestion: Gate the end action on the feature. For example, a `SymbolKind.NamedType` action records whether any type carries a `GeneratedHandlerAttribute`-derived attribute, and the end action builds the graph only then. Or, in a generator host, run a narrow walk that collects declared handlers and checks that they exist.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-mediator-analyzers/handler-type-name.cs:71
- Description: `TryRead` takes the **first** non-empty string constructor argument from any constructor of the derived attribute as the handler name. A derived attribute with an unrelated string parameter, such as `[Reducer("increments the counter")]`, gets that text read as the handler name, and the result is TWM012 "not a type name" or a dispatch to the wrong type. Separately, the `NamedArguments` branch at :59 cannot run: an override of a get-only abstract property cannot add a setter, so `HandlerTypeName = "…"` is never a valid named argument.
- Suggestion: Take the constructor argument only when `attribute.AttributeConstructor.Parameters[i].Name` is `handlerTypeName` (ordinal-ignore-case), and document that name as the contract along with Issue 1. Remove the unreachable named-argument branch, or document why it stays.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-mediator-analyzers/diagnostic-descriptors.cs:97
- Description: Unreadable names and names that are not type names reuse the missing-type message format, so the text reads "Request 'App.Ping' names generated handler 'State.ReducerAttribute.HandlerTypeName (not a constant string)', which does not exist after generation". That sentence is contradictory and does not tell the user what to change.
- Suggestion: Keep TWM012, but build the message from a format that fits each case. For example, use a second descriptor with the same id and a different `messageFormat`, or move "which does not exist after generation" into the argument. Make the unreadable text name the fix from Issue 1.
- Status: open

### Issue 7 — Severity: nit
- File: skills/tw-mediator/SKILL.md:28; agent.md:22 and :45
- Description: The SKILL row says "`[GeneratedHandler]` on the request". The attribute is abstract and cannot be applied; a derived attribute is applied instead. `agent.md` (`CLAUDE.md`) still says the package "Packs the TWM001–TWM004 analyzer", and its `TimeWarp.Mediator` contents list omits `GeneratedHandlerAttribute`. The other docs were updated.
- Suggestion: Say "a `GeneratedHandlerAttribute`-derived attribute on the request". Update the two `agent.md` rows to name TWM012 and `GeneratedHandlerAttribute`.
- Status: open
