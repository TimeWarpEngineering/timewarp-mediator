# Option 3b: GeneratedHandlerAttribute so a request in source can name a handler another generator emits

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

- [ ] Attribute and fact kind
- [ ] Dispatch emitter and TWM diagnostic with tests
- [ ] Incrementality test; build 0/0; version bump

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06, from the timewarp-architecture cockpit)

## Notes

Steve 2026-11-06: all of 0, 1, 2, 3b, 4, 5 on State; 3b is the Mediator decision as much as a
State one. State's half: timewarp-state task "Option 3b (State side)".
