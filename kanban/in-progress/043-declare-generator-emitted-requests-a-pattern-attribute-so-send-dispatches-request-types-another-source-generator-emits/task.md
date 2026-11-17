# Declare generator-emitted requests: a pattern attribute so Send dispatches request types another source generator emits

## Description

Today Mediator discovers requests only from the host compilation's own syntax trees
(`mediator-generator.cs`, `CreateSyntaxProvider`) and from referenced assemblies marked
`[assembly: MediatorAssembly]`. A request type that **another source generator emits into the
same compilation** is never discovered, so the generated `Send` type switch has no arm for it
and the `default` arm throws `NoHandlerException` at run time. Task 029 (`GeneratedHandlerAttribute`,
"3b") covers the reverse direction only: a request written in source naming a handler another
generator emits.

timewarp-architecture task 483 needs the missing direction. Its generator emits, per contract,
a nested request `RoleState.CreateRoleActionSet.Action` and its `Handler` inside the host
project. Mediator must dispatch those. Roslyn generators cannot see each other's output, so
discovery by symbol is impossible; the facts must be **declared on a source type** that Mediator
can see, and `Send` arms must be emitted from those declared names as text.

Steve chose this path on 2026-11-17 over keeping Action shells in source (architecture 483
"Blocker", option 1).

### Shape to build (implementer refines; Steve reviews the public surface before merge)

A Mediator-owned attribute on a **source** type, `AllowMultiple = true`, inheritable so a
consuming repo can derive its own (`[ContractState]` in architecture). It declares request
types that another generator will emit, by **pattern plus contract types**, since the names
must be constants (Mediator cannot run the consumer's naming rule) and listing every request by
hand would duplicate the consumer's attribute:

```csharp
[GeneratedRequestPattern(
  requestTypeName: "{Host}.{Contract}ActionSet.Action",
  handlerTypeName: "{Host}.{Contract}ActionSet.Handler",
  responseTypeName: "{Contract.Response}",      // or a fixed type, or none for IRequest
  contracts: [typeof(CreateRole), typeof(UpdateRole)])]
public partial class RoleState { }
```

Placeholders are the minimum the architecture use needs: `{Host}` (the attributed type's
metadata name), `{Contract}` (each contract type's short name), and the response as either a
fixed type name, a `{Contract.<NestedOrProperty>}` reference, or absent. Exact placeholder set
and syntax are the implementer's call; keep it small and document it.

For each contract the generator computes the request, handler, and response names and emits a
`Send` arm exactly as it does for a discovered request with a `GeneratedHandlerAttribute`
(same handler invocation shape, same behaviors pipeline). No runtime handler table (029's
decision stands).

Analyzer: a diagnostic in the style of the existing "named handler must exist after every
generator has run" rule (`diagnostic-descriptors.cs`) reports a declared request or handler
type that does not exist in the final compilation, and a declared response that does not match
the emitted request's `IRequest<T>`. This is how a consumer learns its generator and its
declaration drifted.

Scope boundaries:
- Mediator does not know about State or contracts. The attribute is generic.
- Referenced-assembly discovery (`MediatorAssembly`) is unchanged.
- No change to `GeneratedHandlerAttribute` semantics.

## Checklist

- [ ] Public attribute (name, placeholders, response forms) written up in the Design region and readme; Steve approves the surface before the PR merges (comment on the PR)
- [ ] Generator emits `Send` arms for declared requests; dispatch goes through the same behaviors pipeline as discovered requests
- [ ] Analyzer diagnostic(s) for missing declared request/handler type and response mismatch; `AnalyzerReleases.Unshipped.md` updated
- [ ] Tests: a test generator (or hand-written "generated" source in a second syntax tree added by a test generator) emits the request and handler; `Send` dispatches; the analyzer fires on a bad declaration
- [ ] A derived attribute works (test derives `[MyPattern]` and the generator still reads it)
- [ ] Samples/docs: one sample showing a sibling generator plus the declaration
- [ ] `dev build` 0/0, `dev test`, `ganda repo audit` clean, `dev check-version` (bump to next beta; this is a library surface change)
- [ ] No cross-repo work: do not edit timewarp-state or timewarp-architecture; stop and report if something there must change

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-17)

## Notes

- Origin: timewarp-architecture task 483 "Blocker" section (2026-11-07) and Steve's choice of
  option 1 on 2026-11-17. The verified facts at master 678beec: `mediator-generator.cs:36-37`,
  `candidate-extractor.declared-handlers.cs:14-31`, `mediator-emitter.cs:270,:299`,
  `referenced-assembly-candidates.cs:11,:85`, `generated-handler-attribute.cs:32`,
  `message-graph-builder.handlers.cs:87`, `diagnostic-descriptors.cs:101`.
- Consumers after release: timewarp-state task 160 (manifest/catalog/dispatch/analyzers honor the
  same declaration), then architecture 483 resumes once both ship and the pins move.
- Release of the resulting beta is Steve's call; the task stops at the merged PR.
