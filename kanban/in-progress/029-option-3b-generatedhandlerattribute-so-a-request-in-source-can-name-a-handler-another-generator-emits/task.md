# Option 3b: GeneratedHandlerAttribute so a request in source can name a handler another generator emits

## Goal (implementer restatement)

Ship an additive public abstract `GeneratedHandlerAttribute` whose `HandlerTypeName` is a constant string relative to the request's containing type (or its namespace when the request is top-level). The host candidate extractor accepts that attribute, and any attribute derived from it, as a new fact on the request type. `MessageGraph` and the emission graph carry the fact, and the existing dispatcher emits a call to the declared handler type name even when another generator emits the handler. When the name is unreadable, or when it still does not exist after every generator has run, report TWM012 on the attribute application so the failure is not only a bare CS0246 inside `Mediator.g.cs`. Requests without the attribute keep today's path, including the unrelated-edit incrementality cache. Bump both packages to 15.0.0-beta.5, ship the analyzer row, and write the release note so TimeWarp.State can pin this version. Do not add a runtime handler table, and do not fold Mediator's emitters into another generator.

## Description

Source: timewarp-state task 140 `report/analysis.md`, Option 3b (Mediator half). Steve decided
2026-11-06 to take it. Read that report's section 3b first.

Roslyn generators cannot see each other's output, so today a handler that TimeWarp.State (or the
TimeWarp.Architecture template) generates is never dispatched by Mediator. The fix is a declared
fact: Mediator ships a public abstract `GeneratedHandlerAttribute` with `HandlerTypeName`
(relative to the request's containing type). The host candidate extractor accepts it on a request
type in source and emits dispatch to that name. Generated code may reference generated code, since
all generator output compiles together.

## Requirements

- Public abstract `GeneratedHandlerAttribute : Attribute` on the request type; derived attributes
  found by base type through the semantic model.
- A new fact kind in `candidate-extractor.cs` and `candidate-graph.cs`; MessageGraph carries it;
  the dispatch emitter targets the declared handler name.
- A TWM diagnostic when the named handler does not exist after generation, so the failure is not
  a bare CS0246 in `Mediator.g.cs`.
- Additive: no change for requests without the attribute. Incrementality test extended.
- Rejected shapes, do not reopen: a runtime handler table (loses compile-time weaving); folding
  Mediator's emitters into State's generator (two generators racing for `Mediator.g.cs`).
- Version bump and release per the repo's rule. TimeWarp.State's 3b task pins the new version.

## Checklist

- [x] Attribute and fact kind
- [x] Dispatch emitter and TWM diagnostic with tests
- [x] Incrementality test; build 0/0; version bump

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06, from the timewarp-architecture cockpit)
- Implementation: 01a1ace5-a6e2-77c1-adcf-7d30cf063e6c (2026-11-06)

## Results

`GeneratedHandlerAttribute` is a public abstract attribute. `HandlerTypeName` is a constant string relative to the request's containing type, or to its namespace when the request is top-level. Derived attributes are found by base type. The host candidate extractor records that fact, `MessageGraph` carries it, and the existing dispatcher emits the declared type name. A request with no attribute stays on the previous path.

The same type is one handler. The request fact can be visited before the handler fact, so `AddHandler` drops a second entry with the same key or the same fully qualified name. A different type is still TWM002.

An unreadable name is TWM012 from the generator in a host and from the analyzer in a library. A readable name that still does not exist after every generator has run is TWM012 from the analyzer on the attribute, including in a generator host. A library does not report that missing type. The generated mediator still names the handler. Under an existing namespace the compiler reports CS0234; otherwise it reports CS0246. TWM012 is the diagnostic on the request.

Both packages are `15.0.0-beta.5`. TWM012 is in `AnalyzerReleases.Shipped.md` under Release 15.0.0. This walk does not tag or publish. TimeWarp.State pins this version from its own 3b task. `./bin/dev release` stays on master after merge.

`./bin/dev workflow` (mode Pr), after the partial-class split, exited 0. Build: 0 warnings, 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 48, compilation 76, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.5.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.5.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0. The log ends with `Pipeline SUCCEEDED`.

`ganda repo audit --fix` added `bin/dev`, `bin/dev.dbg`, and `bin/dev.exe` to `.gitignore`. The four files over the 500-line cap (`message-graph-builder.cs`, `candidate-extractor.cs`, `candidate-graph.cs`, `mediator-emitter.cs`) are partials split by responsibility. The audit then exited 0 (`Repository passes`) with one advisory warning: `kanban-landed` says claim 021 has no usable worktree. That claim is not this task, and `--fix` did not land a kitchen for it.

### How to validate

Smoke: `./bin/dev workflow` and `dotnet test tests/timewarp-mediator-generators-compilation-tests/timewarp-mediator-generators-compilation-tests.csproj -f net10.0 --filter "FullyQualifiedName~GeneratedHandler|FullyQualifiedName~DeclaredHandler|FullyQualifiedName~AddingGeneratedHandler"`.

Expect: workflow prints `0 Warning(s)`, tests 61 + 48 + 76 passed on net10.0 and net11.0, `Samples verified successfully!`, both `15.0.0-beta.5` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`. The filtered run passes the cross-generator Send (`pong`), the metadata getter read, the nested `global::App.CounterState.Handler` binding, TWM012 plus CS0234 for `App.MissingHandler`, the unreadable-name TWM012, and both incrementality cases (unrelated edit stays cached; adding the attribute modifies the model).

## Notes

Steve 2026-11-06: all of 0, 1, 2, 3b, 4, 5 on State; 3b is the Mediator decision as much as a
State one. State's half: timewarp-state task "Option 3b (State side)".
