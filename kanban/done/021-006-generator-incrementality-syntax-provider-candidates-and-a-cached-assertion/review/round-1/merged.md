# Round 1 — merged findings
**Date:** 2026-10-24
**Sources:** general (passes general-a and general-b; see `general.md`)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 6 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 5 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/build-transitive/timewarp-mediator-generators.props:9; candidate-graph.cs:49
- Description: `TimeWarpMediatorAssemblyName=$(AssemblyName)` is set in a buildTransitive `.props`, which NuGet imports before the SDK sets `AssemblyName`. For package consumers it is empty, so the host is never a member. Confirmed in the package-smoke editorconfig, where the value is empty. The effects:
  - host behaviors are dropped;
  - TWM001 and TWM002 are lost for host requests;
  - derived handlers double-bind;
  - the manifest has empty membership;
  - `InternalsVisibleTo` grants to the host fail.
  
  The tests set the property by hand, so they mask this.
- Suggestion: Get the host name from `CompilationProvider.Select(c => c.AssemblyName)`, which is an equatable string, so the model stays `Cached`. Alternatively, set it in a `.targets`. Add a package-smoke assertion on host membership.
- Source: general-a #1, general-b #1
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/candidate-extractor.cs:159-162; candidate-graph.cs:413-421
- Description: Message scope is lost when the message type has no `TypeFact`: plain notifications, abstract or generic requests, and requests from non-member assemblies. Handlers then land on the wrong publisher or sender, and TWM003/TWM005 scope-mismatch reports are lost.
- Suggestion: Carry the resolved message scope on `HandlerMatch`, or emit scope facts. Add a test where only the notification is scoped.
- Source: general-a #2, general-b #2
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/referenced-assembly-candidates.cs:31-34
- Description: The scratch access compilation has no host public key, so a strong-named host loses `InternalsVisibleTo(…, PublicKey=…)` access. The result is a false TWM007 and a dropped handler.
- Suggestion: Pass the host public key and use `WithCryptoPublicKey`.
- Source: general-a #3
- Disposition notes:

### M4 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/type-model.cs:102-119
- Description: Array types get no interface keys, so behaviors with an interface constraint on `TResponse` stop closing for array responses. The old code used `AllInterfaces`.
- Suggestion: Fill the interface keys for every `ITypeSymbol`.
- Source: general-a #4, general-b #3
- Disposition notes:

### M5 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/candidate-extractor.cs:183-211; candidate-graph.cs:84
- Description: The per-request `bound` dedupe runs before `BoundByBase`, so an inherited interface blocks the derived class's own match, and TWM002 is lost. Likelihood is low.
- Suggestion: Dedupe after `BoundByBase` in `CandidateGraph`.
- Source: general-a #5, general-b #4
- Disposition notes:

### M6 — Severity: bug — Status: open
- File: source/timewarp-mediator-generators/candidate-extractor.cs:38
- Description: A using-alias spelling of `[assembly: MediatorBehavior]` fails the name predicate, so the behavior is silently dropped. Likelihood is low.
- Suggestion: Match every attribute in an `assembly:`-targeted list, then filter by symbol.
- Source: general-a #6 (bug), general-b #5 (suggestion); collapsed to the stronger severity
- Disposition notes:

### M7 — Severity: suggestion — Status: open
- File: source/timewarp-mediator-generators/candidate-extractor.cs:257-262, 295-303
- Description: Behavior closing diverges for type-parameter constraints (`where TRes : TReq`) and for nested type-parameter pipeline arguments (`Result<TReq>`).
- Suggestion: Encode both cases so they reproduce the old `CanConstruct` result.
- Source: general-a #7
- Disposition notes:

### M8 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/referenced-assembly-candidates.cs:48-62
- Description: Duplicate references to the same assembly double its behavior facts.
- Suggestion: Dedupe by `IAssemblySymbol`.
- Source: general-a #8
- Disposition notes:

### M9 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/candidate-graph.cs:93
- Description: `dropped` also collects notification matches. The old code collected request matches only.
- Suggestion: Guard the add with `!match.IsNotification`.
- Source: general-b #6
- Disposition notes:

### M10 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/candidate-graph.cs:39-46, 69, 163
- Description: Diagnostic order is host-first instead of ordinal by assembly name.
- Suggestion: Sort the facts by assembly name before building `byKey`.
- Source: general-b #7
- Disposition notes:

### M11 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/candidate-graph.cs:328, 469; candidate-facts.cs Design
- Description: The TWM007 location falls back to the outer type. The unreachable `RequestModel` fallback returns the response type. The Path/SpanStart comment is misleading.
- Suggestion: See general.md Issue 11.
- Source: general-a #9, general-b #8
- Disposition notes:

### M12 — Severity: nit — Status: open
- File: source/timewarp-mediator-generators/referenced-assembly-candidates.cs:36-43
- Description: TWM008 resolves against a reference-only compilation, so a required type declared in host source gives a false TWM008.
- Suggestion: Document this, or include host declarations.
- Source: general-b #9
- Disposition notes:

## Duplicates / conflicts

- Both passes independently found M1, M2, M4, M5, M6 and M11. Those were collapsed, and the stronger severity was kept for M6.
- The orchestrator verified the evidence for M1 (package-smoke `GeneratedMSBuildEditorConfig` shows an empty `TimeWarpMediatorAssemblyName`), M2 (`ReadType` returns null without matches or a concrete request, and master `ResolveScope(pair.Key)` is at :237), and M4 (`CreateCore` is gated on `INamedTypeSymbol`).
- The incrementality goal itself holds: `IncrementalGeneratorTests` pass 3/3 on this branch.

## Plan alignment

- Restatement vs task: no misread. The restatement covers every requirement. Using `CreateSyntaxProvider` instead of `ForAttributeWithMetadataName` is allowed ("syntax providers"), and the reason is documented. Deriving the host name from an MSBuild property is a design choice and is the root cause of M1. It is not a misread of the task.
- The requirement "No change to emitted output" is met for the in-repo goldens only. M1, M2 and M4–M6 are output changes the goldens don't cover.
