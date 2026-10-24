# Round 1 — general
**Date:** 2026-10-24
**Scope reviewed:** branch vs `master`, all of `source/timewarp-mediator-generators`, the analyzer `MembershipOptions` change, the buildTransitive props, and the compilation tests. Two general passes ran in parallel: general-a covered extraction and general-b covered the graph and emitters. The orchestrator verified their findings.

## Summary

The pipeline change works as intended for incrementality. `HostCandidates`, `ReferencedAssemblies`, `Model`, and `Sources` are `Cached` on an unrelated edit (`IncrementalGeneratorTests` 3/3 pass, re-run by the orchestrator). The equatable records are sound, and the emitter and member-name diffs are mechanical. The weak spot is the "no change to emitted output" requirement. The new fact path diverges from the old `MessageGraphBuilder` symbol walk in several places that the in-repo goldens don't exercise. The worst: NuGet consumers get an empty host assembly name, so the host is never a member assembly. The second: `[MediatorScope]` on a message type with no `TypeFact` (every plain notification) is lost.

## Issues

### Issue 1 — Severity: bug (general-a #1, general-b #1)
- File: source/timewarp-mediator-generators/build-transitive/timewarp-mediator-generators.props:9; candidate-graph.cs:49; referenced-assembly-candidates.cs:31-34
- Description: `TimeWarpMediatorAssemblyName` defaults to `$(AssemblyName)` in a buildTransitive `.props`. NuGet imports that file from `Microsoft.Common.props`, before `Microsoft.NET.Sdk.props` sets `AssemblyName`, so a package consumer gets an empty string. The orchestrator confirmed this in the repo. `tests/timewarp-mediator-package-smoke/obj/Release/net10.0/timewarp-mediator-package-smoke.GeneratedMSBuildEditorConfig.editorconfig` has `build_property.TimeWarpMediatorAssemblyName = ` (empty), while the samples, which import the props in the project body, get the real name. With an empty name:
  - the host is not added to `members`;
  - the scratch compilation is named `TimeWarpMediatorMetadata`;
  - TWM007 prints an empty host.
- Failure scenario: in a NuGet consumer that leaves `AssemblyName` unset:
  - Host `[assembly: MediatorBehavior(typeof(Log<,>))]` is silently not woven (candidate-graph.cs:135).
  - A host request with no handler reports no TWM001 (lines 77 and 170).
  - `DerivedH : BaseH : IRequestHandler<Q,int>` binds twice because `BoundByBase` needs a member base (line 391). The result is no binding, no TWM002, and a runtime `NoHandlerException`.
  - The manifest has `"membership":[]`.
  - A referenced library's `InternalsVisibleTo("MyHost")` is ignored, which gives a false TWM007 and a dropped handler.
  - The compilation tests set the property by hand (host-compiler.cs:91), so they hide all of this.
- Suggestion: Do not depend on MSBuild evaluation order. `context.CompilationProvider.Select((c, _) => c.AssemblyName ?? "")` returns an equal string on unrelated edits, so the steps downstream stay `Cached`. Alternatively, set the default in a buildTransitive `.targets`. Add a package-smoke assertion that the host is a member and that one host behavior or TWM001 appears.
- Status: open

### Issue 2 — Severity: bug (general-a #2, general-b #2)
- File: source/timewarp-mediator-generators/candidate-extractor.cs:159-162; candidate-graph.cs:413-421
- Description: `ReadType` returns null unless a type has handler matches or is a concrete request. `ScopeOf` only looks in `byKey`, so the scope is lost for:
  - plain `INotification` types;
  - abstract or constructed-generic request types;
  - requests declared in non-member assemblies.
  
  The old code called `ResolveScope(symbol)` on the message itself (master message-graph-builder.cs:191, :237).
- Failure scenario: `[MediatorScope(typeof(Billing))] sealed class InvoicePaid : INotification {}` plus an unscoped `InvoicePaidHandler`.
  - Old: the handler goes on `Publisher_Billing`.
  - New: it goes on the default publisher.
  - With a handler scoped to `Shipping`, old reported a scope mismatch (TWM003/TWM005). New reports nothing, and the analyzer skips it in a generator host.
- Suggestion: Record the resolved message scope on `HandlerMatch` (message and response), or emit a fact for any discoverable type whose `ResolveScope` is not null. Add a generator test where only the notification carries `[MediatorScope]`.
- Status: open

### Issue 3 — Severity: bug (general-a #3)
- File: source/timewarp-mediator-generators/referenced-assembly-candidates.cs:31-34
- Description: The scratch compilation used for `IsSymbolAccessibleWithin` is named after the host but has no public key. An `InternalsVisibleTo` grant that names a public key fails for a strong-named host, while the real compilation (the old `AccessGuard`) matched.
- Failure scenario: a host with `SignAssembly=true` and a member library with `[assembly: InternalsVisibleTo("Host, PublicKey=…")]` plus an internal handler. Old binds it. New drops it with TWM007 (and TWM001).
- Suggestion: Carry the host public key from the same `CompilationProvider.Select` as Issue 1, and build the scratch compilation with `WithCryptoPublicKey`.
- Status: open

### Issue 4 — Severity: bug (general-a #4, general-b #3)
- File: source/timewarp-mediator-generators/type-model.cs:102-119
- Description: `BaseKeys` and `InterfaceKeys` are filled only for `INamedTypeSymbol`. An array response has no interface keys, so a behavior with an interface constraint on `TResponse` no longer closes. The old `ImplementsOrInherits` used `ITypeSymbol.AllInterfaces`.
- Failure scenario: `GetIds : IRequest<int[]>` plus a behavior `where TResponse : System.Collections.IEnumerable`. Old weaves it. New does not, and reports TWM011 if nothing else matches.
- Suggestion: Fill the interface keys from `type.AllInterfaces` for every `ITypeSymbol`, and keep the base-chain walk for named types only, as the old code did.
- Status: open

### Issue 5 — Severity: bug, low likelihood (general-a #5, general-b #4)
- File: source/timewarp-mediator-generators/candidate-extractor.cs:183-211 with candidate-graph.cs:84
- Description: The per-request `bound.Add` dedupe now runs before `BoundByBase`. The old code short-circuited `!IsBoundByBaseHandler(...) && bound.Add(...)`. An inherited interface therefore claims the request and blocks the derived class's own interface match.
- Failure scenario: `Base : IRequestHandler<R,int>` and `Derived : Base, IRequestHandler<R,string>` (or `IRequestHandler<R>`). Old reports TWM002. New binds `Base` silently.
- Suggestion: Keep all candidate matches in the fact. Apply `BoundByBase` first and the per-request dedupe after it, in `CandidateGraph`.
- Status: open

### Issue 6 — Severity: bug, low likelihood (general-a #6; general-b #5 as suggestion)
- File: source/timewarp-mediator-generators/candidate-extractor.cs:38
- Description: The predicate `attribute.Name.ToString().Contains("MediatorBehavior")` misses a using-alias spelling. The old walk read `Assembly.GetAttributes()` by symbol.
- Failure scenario: `using Weave = TimeWarp.Mediator.MediatorBehaviorAttribute; [assembly: Weave(typeof(Log<,>))]`. Old weaves it. New drops it silently.
- Suggestion: Match any attribute in an `assembly:`-targeted `AttributeListSyntax`, and keep the symbol filter in `ExtractBehavior`.
- Status: open

### Issue 7 — Severity: suggestion (general-a #7)
- File: source/timewarp-mediator-generators/candidate-extractor.cs:257-262, 295-303
- Description: Behavior description diverges from the old `CanConstruct` / `ImplementsPipeline` in two ways:
  - `ReadParameters` skips type-parameter constraints. Old treated them as unsatisfiable.
  - `Slot` encodes `Result<TRequest>`-style arguments as an open concrete key, which never matches a closed response.
- Failure scenario:
  - `where TRes : TReq`: old does not weave. New weaves, and the generated code can fail with CS0311.
  - `W<TReq> : IPipelineBehavior<TReq, Result<TReq>>`: old weaves. New does not.
- Suggestion: Encode type-parameter constraints so they are treated as unsatisfiable, and encode nested type-parameter substitution, or mark the slot as needing construction.
- Status: open

### Issue 8 — Severity: nit (general-a #8)
- File: source/timewarp-mediator-generators/referenced-assembly-candidates.cs:48-62
- Description: References are walked per `MetadataReference` without deduping by `IAssemblySymbol`. The old `Membership` used a set. Duplicate or aliased references double that assembly's `BehaviorFact`s. Type facts are deduped by key, so only behaviors are affected.
- Suggestion: Skip assemblies already seen, using a `HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default)`.
- Status: open

### Issue 9 — Severity: nit (general-b #6)
- File: source/timewarp-mediator-generators/candidate-graph.cs:93
- Description: `dropped.Add(match.Message.Key)` also runs for notification matches. The old code only dropped request matches, so a type that is both a request and a notification, with an inaccessible notification handler, loses its TWM001.
- Suggestion: Guard the add with `!match.IsNotification`.
- Status: open

### Issue 10 — Severity: nit (general-b #7)
- File: source/timewarp-mediator-generators/candidate-graph.cs:39-46, 69, 163
- Description: Facts iterate host-first, then references. The old code walked member assemblies in ordinal name order, so diagnostic order and same-display-name tie-breaks can differ. Emitted source is unaffected.
- Suggestion: Sort facts by `(AssemblyName ordinal, index)` before building `byKey`.
- Status: open

### Issue 11 — Severity: nit (general-a #9, general-b #8)
- File: source/timewarp-mediator-generators/candidate-graph.cs:328, 469; candidate-facts.cs Design region
- Description: Three small mismatches:
  - The TWM007 location falls back to the outer type's location (`inaccessible.Location ?? type.Location`), where the old code used `Location.None` for metadata types.
  - The `RequestModel` fallback returns the response type as the request. It is unreachable today.
  - The Design comment "Path and SpanStart preserve attribute order" is misleading for metadata behaviors, where `Path` is always `""` and `SpanStart` is a `GetAttributes()` index.
- Suggestion: Use `inaccessible.Location` only, make the fallback throw or use `requests[key]`, and fix the comment or rename the field.
- Status: open

### Issue 12 — Severity: nit (general-b #9)
- File: source/timewarp-mediator-generators/referenced-assembly-candidates.cs:36-43
- Description: TWM008 now resolves the required types against a reference-only scratch compilation. A host that declares one of those types in its own source gets a false TWM008.
- Suggestion: Document this, or record host declarations of those names in `HostInput`.
- Status: open
