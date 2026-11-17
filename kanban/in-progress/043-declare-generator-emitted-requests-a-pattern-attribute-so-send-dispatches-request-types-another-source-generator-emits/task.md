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

## Goal (implementer restatement)

Another source generator can emit a request and its handler into the host compilation. Mediator's
generator cannot see that output, so today `Send` has no arm for the request and throws
`NoHandlerException`. Add a public, inheritable, `AllowMultiple` attribute in `TimeWarp.Mediator`
that a source type carries to declare those requests by name pattern plus a list of contract types.
For each contract the generator computes the request, handler, and response from constant
templates and emits a `Send` arm with the same handler call and the same woven behaviors as any
discovered request. Behaviors constrained on the request's interface (for example State's
`where TRequest : IAction`) must still close over the declared request.

Acceptance:

- The attribute, its placeholders, and its response forms are documented in the attribute's Design
  region and in the package readme. Steve approves the surface on the PR before merge.
- `Send` dispatches a request a sibling test generator emits through `RegisterSourceOutput`, and
  woven behaviors run on it.
- A diagnostic reports, after every generator has run, a declared request or handler that does not
  exist, a request that does not implement the declared interface (response mismatch), and a handler
  that does not implement the request's `IRequestHandler`. Bad templates are reported by the
  generator. `AnalyzerReleases.Unshipped.md` lists the new rule.
- A derived attribute (also one declared in a metadata-only reference) is read the same way.
- One sample shows a sibling generator plus the declaration.
- `dev build` 0/0, `dev test` green, `ganda repo audit` clean, version bumped to the next beta so
  `dev check-version` passes.
- No edits to timewarp-state or timewarp-architecture.

## Checklist

- [x] Public attribute (name, placeholders, response forms) written up in the Design region and readme; Steve approves the surface before the PR merges (comment on the PR) — written up; approval is pending on the PR
- [x] Generator emits `Send` arms for declared requests; dispatch goes through the same behaviors pipeline as discovered requests
- [x] Analyzer diagnostic(s) for missing declared request/handler type and response mismatch; `AnalyzerReleases.Unshipped.md` updated
- [x] Tests: a test generator (or hand-written "generated" source in a second syntax tree added by a test generator) emits the request and handler; `Send` dispatches; the analyzer fires on a bad declaration
- [x] A derived attribute works (test derives `[MyPattern]` and the generator still reads it)
- [x] Samples/docs: one sample showing a sibling generator plus the declaration
- [x] `dev build` 0/0, `dev test`, `ganda repo audit` clean, `dev check-version` (bump to next beta; this is a library surface change)
- [x] No cross-repo work: do not edit timewarp-state or timewarp-architecture; stop and report if something there must change

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-17)
- 2026-11-17 implementer (ganda task work, implement oracle): built `GeneratedRequestPatternAttribute`,
  generator emission, analyzer TWM013, tests, sample, docs, version 15.0.0-beta.10. Gates under Results.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 790bd376-89e2-476d-b96c-840dc5e86d97, max-turns 500, Done — 2026-11-17T06:19:46Z

## Results

### Public surface (for Steve's review on the PR)

`TimeWarp.Mediator.GeneratedRequestPatternAttribute` (`source/timewarp-mediator/generated-request-pattern-attribute.cs`):

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public class GeneratedRequestPatternAttribute : Attribute
{
  public GeneratedRequestPatternAttribute(string requestTypeName, string handlerTypeName, string requestInterfaceName, params Type[] contracts);
  protected GeneratedRequestPatternAttribute(params Type[] contracts); // derived attribute with const templates
  public IReadOnlyList<Type> Contracts { get; }
}
```

- Placeholders: `{Host}` (attributed type's dotted full name), `{Contract}` (contract simple name),
  `{ContractType}` (contract dotted full name). Anything else is TWM013.
- Response forms: one template, `requestInterfaceName`, instead of the spec's `responseTypeName`.
  `TimeWarp.Mediator.IAction` / `IRequest` is void; `TimeWarp.Mediator.IRequest<{ContractType}.Response>`
  takes the response from a contract's nested type; `IRequest<string>` is a fixed response.
  Reason: State's behaviors are `where TRequest : IAction`. A response-only declaration would leave
  the declared request with no `IAction`, so those behaviors would not close over it. Naming the
  interface covers all three response forms and makes the behaviors close.
- Derived attributes: each template comes from a constructor parameter with the same name (a default
  value is enough) or a `const string` field `RequestTypeName` / `HandlerTypeName` /
  `RequestInterfaceName`. Contracts come from a constructor parameter named `contracts` (may be
  `params`). Consts survive in a reference assembly; getter bodies do not (the 029 lesson). This is
  what lets architecture write `[ContractState(typeof(GetRoles), typeof(CreateRole))]`.
- Scope: the nearest existing containing type's `[MediatorScope]` (the host), else the assembly's.
  A scope attribute on the emitted request itself is invisible.

### Diagnostics

TWM013 (new, `AnalyzerReleases.Unshipped.md`). The generator reports a pattern it cannot read or
expand (missing constant, unknown placeholder, generic host or contract, non-dotted name, unresolved
or non-`IRequest` interface). The analyzer reports, in a generator host after every generator has
run, a missing request, a missing handler, a request that does not implement the declared interface
(response mismatch), and a handler that is not `IRequestHandler<request[, response]>`.

### Changes

- Shared reader `source/timewarp-mediator-analyzers/request-pattern.cs` (linked into the generator).
- Analyzer check `request-pattern-analysis.cs`, wired into `RequestHandlerAnalyzer`'s compilation end.
- Generator: new syntax provider `candidate-extractor.request-patterns.cs` (attributed types, so a host
  with no base list is found), `RequestPatternFact` in `HostInput`, and
  `candidate-graph.request-patterns.cs`. The patterns reuse the declared-handler path, so the Send arm,
  handler cache / chain, and behavior weaving are unchanged. The declared request's model carries
  the interface keys so constraints close.
- `MessageGraphBuilder.GetScopeFromAttributes` is now `internal` (was `private`) for the reader.
- Tests: `tests/timewarp-mediator-generators-compilation-tests/generated-request-pattern-tests.cs`
  (13 cases: sibling `RegisterSourceOutput` generator dispatch with a woven behavior, void `IAction`
  in a scoped pipeline with an `IAction`-constrained behavior, derived attribute in a metadata-only
  reference via consts, derived attribute in source via default parameters, the four analyzer
  TWM013 cases, generator TWM013 for bad templates and an unreadable derived attribute, and the
  unrelated-edit incrementality cache).
- Sample: `samples/timewarp-mediator-examples-generated-requests` plus its sibling generator
  `samples/timewarp-mediator-examples-generated-requests-generator` (added to the slnx). The sample
  generator targets `netstandard2.0`: RS1041 fires on net11.0, and fixing it beats a `NoWarn`. It is
  not a package, so the net11.0-only package decision does not cover it.
- Docs: `documentation/diagnostics.md` (TWM013 section), `documentation/architecture.md` (new
  "Requests another generator emits" section, table rows), package readmes, `samples/readme.md`,
  `skills/tw-mediator/SKILL.md`, `documentation/release-notes/15.0.0-beta.10.md`.
- Version `15.0.0-beta.10` (`source/Directory.Build.props`, install snippets).

### Gates

- `./bin/dev build` (and `--clean`): Build succeeded, 0 Warning(s), 0 Error(s).
- `./bin/dev test`: passed. generators-tests 61/61, analyzers-tests 50/50, generators-compilation-tests 93/93.
- `./bin/dev verify-samples`: Samples verified successfully (the new sample prints `greet:Hello, Ada`, `add:5`).
- `./bin/dev check-version`: source 15.0.0-beta.10 is one prerelease ahead of the published 15.0.0-beta.9.
- `ganda repo audit`: the first run failed `source-file-line-cap` on two new files (`request-pattern.cs`
  698 lines, `generated-request-pattern-tests.cs` 601). Split them into `request-pattern.cs`,
  `request-pattern.templates.cs`, `request-pattern.type-names.cs`, and a
  `generated-request-pattern-tests.sources.cs` partial. Then reran build (0/0) and test (all pass),
  ran `ganda repo audit --fix`, and the audit now passes: "Repository passes all audit checks", exit 0.

### How to validate

- Smoke: `./bin/dev build && dotnet run --project samples/timewarp-mediator-examples-generated-requests --no-build -c Release`
  and `dotnet test tests/timewarp-mediator-generators-compilation-tests --filter "FullyQualifiedName~GeneratedRequestPattern"`.
- Expect: the sample prints `greet:Hello, Ada` and `add:5` and exits 0. Its Actions come from the sibling
  generator, which the Mediator generator cannot see. All 13 `GeneratedRequestPatternTests` pass. In
  `Pattern_RequestsFromSiblingGenerator_DispatchThroughWovenBehavior`, `Mediator.g.cs` has
  `case global::App.RoleState.CreateRoleActionSet.Action` and the woven `Tag<...>` behavior runs.

### Follow-ups / risks

- Steve reviews the public surface on the PR (the `requestInterfaceName` choice replaces the spec's
  `responseTypeName`).
- timewarp-state task 160 and architecture 483 consume this after release; no cross-repo edits were made.
- A behavior constrained on an interface the emitted request has but the declaration does not name
  will not close. Only the declared interface and the interfaces it extends are known.

## Notes

- Origin: timewarp-architecture task 483 "Blocker" section (2026-11-07) and Steve's choice of
  option 1 on 2026-11-17. The verified facts at master 678beec: `mediator-generator.cs:36-37`,
  `candidate-extractor.declared-handlers.cs:14-31`, `mediator-emitter.cs:270,:299`,
  `referenced-assembly-candidates.cs:11,:85`, `generated-handler-attribute.cs:32`,
  `message-graph-builder.handlers.cs:87`, `diagnostic-descriptors.cs:101`.
- Consumers after release: timewarp-state task 160 (manifest/catalog/dispatch/analyzers honor the
  same declaration), then architecture 483 resumes once both ship and the pins move.
- Release of the resulting beta is Steve's call; the task stops at the merged PR.
