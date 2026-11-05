# Option 3b: GeneratedHandlerAttribute so a request in source can name a handler another generator emits

## Goal (implementer restatement)

A request written in source can name the handler another generator emits. The public contract is an abstract `GeneratedHandlerAttribute`. The handler name is a constant string relative to the request's containing type, or to the request's namespace when the request is top-level. The extractor records that fact, the dispatcher emits that type name, and TWM012 is reported on the attribute when the name cannot be read or the handler is still absent after generation. Requests without the attribute stay on the previous path, including the unrelated-edit cache. Both packages stay at 15.0.0-beta.5. There is no runtime handler table.

Round-1 review is part of the same acceptance bar:

- An attribute declared in another assembly is read from a constructor parameter named `handlerTypeName`. A default value counts. A metadata-only library image is the test, because a ProjectReference hands the compiler a reference assembly whose getter body is not the constant. The IL and private-reflection reader goes away. TWM012 tells the user to add that parameter.
- The same constructor read ignores every other string parameter. The dead named-argument branch goes away.
- A handler emitted from `RegisterSourceOutput` (invisible to this generator), nested as `CounterState.Handler`, dispatches and returns the send result. The post-initialization test is named as the visible dedupe case.
- A declared name that resolves to a type which does not implement the request's `IRequestHandler` is TWM012, with that interface in the message.
- In a generator host the full message graph is built at compilation end only when some type carries a `GeneratedHandlerAttribute`-derived attribute.
- Unreadable and not-a-type-name TWM012 each use their own message format under the same id.
- The skill and `agent.md` say a derived attribute, and they name `GeneratedHandlerAttribute` and TWM012.

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
- [x] Round-1 findings M1–M7 on this branch

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06, from the timewarp-architecture cockpit)
- Implementation: 01a1ace5-a6e2-77c1-adcf-7d30cf063e6c (2026-11-06)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ace5-a6e2-77c1-adcf-7d30cf063e6c, max-turns 500, Done — 2026-11-05T19:03:46Z
- Review oracle round 1: general reviewer (Claude Opus 5.5, claude-opus-5-5), effort 3, turn cap 200. 2026-11-06. Outcome: 7 open (2 bug, 3 suggestion, 2 nit); fix pass requested
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 10a53bba-de66-4161-9376-a4bf0a764511, max-turns 200, artifacts general.md, merged.md — 2026-11-05T19:09:08Z
- Fix pass (implement-fix-2): session 01a1ad1d-6c44-7ce0-b4ab-64246cf7c12d (2026-11-06). Implemented M1–M7. Did not run a review.

## Results

`GeneratedHandlerAttribute` is a public abstract attribute. `HandlerTypeName` is a constant string relative to the request's containing type, or to its namespace when the request is top-level. Derived attributes are found by base type. The host candidate extractor records that fact, `MessageGraph` carries it, and the existing dispatcher emits the declared type name. A request with no attribute stays on the previous path.

The same type is one handler. The request fact can be visited before the handler fact, so `AddHandler` drops a second entry with the same key or the same fully qualified name. A different type is still TWM002.

An unreadable name is TWM012 from the generator in a host and from the analyzer in a library. A readable name that still does not exist after every generator has run is TWM012 from the analyzer on the attribute, including in a generator host. A library does not report that missing type. The generated mediator still names the handler. Under an existing namespace the compiler reports CS0234; otherwise it reports CS0246. TWM012 is the diagnostic on the request.

Both packages are `15.0.0-beta.5`. TWM012 is in `AnalyzerReleases.Shipped.md` under Release 15.0.0. This walk does not tag or publish. TimeWarp.State pins this version from its own 3b task. `./bin/dev release` stays on master after merge.

`./bin/dev workflow` (mode Pr), after the partial-class split, exited 0. Build: 0 warnings, 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 48, compilation 76, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.5.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.5.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0. The log ends with `Pipeline SUCCEEDED`.

`ganda repo audit --fix` added `bin/dev`, `bin/dev.dbg`, and `bin/dev.exe` to `.gitignore`. The four files over the 500-line cap (`message-graph-builder.cs`, `candidate-extractor.cs`, `candidate-graph.cs`, `mediator-emitter.cs`) are partials split by responsibility. The audit then exited 0 (`Repository passes`) with one advisory warning: `kanban-landed` says claim 021 has no usable worktree. That claim is not this task, and `--fix` did not land a kitchen for it. This fix pass ran `ganda repo audit` and `ganda repo audit --fix` again: exit 0, the same advisory, 35 passed, and `--fix` landed 0 kitchens.

Round 1 left seven findings. This pass implements them. The cross-assembly contract is a constructor parameter named `handlerTypeName` (a default counts). The getter-IL and private-reflection reader is gone. An unrelated string parameter is ignored. A name that is not a constant string, a name that is not a type name, and a type that is not the request's `IRequestHandler` each keep id TWM012 and use their own message. A generator host builds the compilation-end message graph only when some member-assembly type carries a `GeneratedHandlerAttribute`-derived attribute. The skill and `agent.md` name that attribute and TWM012. Version stays `15.0.0-beta.5`. This walk does not tag or publish.

### How to validate

Smoke: Release `net10.0` of the three suites that cover the attribute.

```bash
dotnet test tests/timewarp-mediator-analyzers-tests/timewarp-mediator-analyzers-tests.csproj -c Release -f net10.0
dotnet test tests/timewarp-mediator-generators-compilation-tests/timewarp-mediator-generators-compilation-tests.csproj -c Release -f net10.0
dotnet test tests/timewarp-mediator-generators-tests/timewarp-mediator-generators-tests.csproj -c Release -f net10.0
```

Expect: analyzers 50 passed, compilation 80 passed, generators 61 passed, failed 0. That includes a metadata-only library whose `handlerTypeName` default dispatches `Send` to `pong` and emits `global::App.PingHandler`; a metadata-only getter-only attribute is TWM012 containing `not a constant string` and `handlerTypeName`; `RegisterSourceOutput` nested `CounterState.Handler` returns `pong`; an unrelated constructor string still uses the getter; a resolved type that is not `IRequestHandler<App.Ping, string>` is TWM012 naming that interface. This pass did not re-run `./bin/dev workflow` or `net11.0`.

### Review

Round 1 ran with one general reviewer at effort 3. The fix pass implemented all seven findings. Disposition belongs to review round 2.

| Severity | Open | Fixed | Won't fix |
|---|---|---|---|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 2 | 0 |

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`. `review/disposition.md` does not exist yet.

## Review

- Round 1 findings M1–M7 are implemented on this branch. Round 2 is the host review node.

## Notes

Steve 2026-11-06: all of 0, 1, 2, 3b, 4, 5 on State; 3b is the Mediator decision as much as a
State one. State's half: timewarp-state task "Option 3b (State side)".
