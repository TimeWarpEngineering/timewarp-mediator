# Findings: generators + analyzers (+ their tests)

Paths are relative to the task-013 worktree. "builder" = source/timewarp-mediator-analyzers/message-graph-builder.cs, "emitter" = source/timewarp-mediator-generators/mediator-emitter.cs.

### G-1: Generator is not incremental; whole graph rebuilt per compilation
- Severity: risk
- Area: source/timewarp-mediator-generators
- Evidence: mediator-generator.cs:12-27 combines CompilationProvider with AnalyzerConfigOptionsProvider and registers one RegisterSourceOutput that runs Membership.ForGenerator, MessageGraphBuilder.Build, and both emitters on the raw Compilation. The graph (builder.cs:36-74) walks every type of every member assembly; Membership.HasModuleAttribute (membership.cs:142-156) walks all types of the current assembly.
- Impact: Every keystroke in an IDE reruns full discovery and re-emits all sources; the pipeline captures Compilation and ISymbols, so no step can be cached and there are no equatable models.
- Proposed fix: Split into a syntax/semantic provider producing equatable models (strings, ImmutableArray of records) and emit from those; add WithTrackingName steps and a caching test.
- Child task: yes

### G-2: Duplicate diagnostics when generator package is referenced
- Severity: risk
- Area: source/timewarp-mediator-generators, source/timewarp-mediator-analyzers
- Evidence: Generators csproj:36-37 packs Analyzers.dll into the generator nupkg; the buildTransitive props (build-transitive/timewarp-mediator-generators.props:4) sets TimeWarpMediatorAssembly=true, so Membership.TryCreate succeeds in the analyzer (request-handler-analyzer.cs:23-30) and reports graph.Diagnostics at compilation end (:35-41). The generator reports the same graph.Diagnostics (mediator-generator.cs:20-23).
- Impact: TWM001/002/003 are very likely reported twice in generator hosts (not verified at build time; no test asserts the count). Consumers who add both the Analyzers and Generators packages also get the same analyzer assembly from two packages.
- Proposed fix: Have the generator skip reporting diagnostics when the analyzer is shipped alongside, or drop the analyzer from the generator package and make the generator the single reporter. Add a test that compiles with both and asserts a single TWM001.
- Child task: yes

### G-3: Handler implementing several request or notification interfaces is bound to only the first
- Severity: bug
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:244-270 (TryGetRequestHandler) and :286-295 (TryGetNotificationHandler) return on the first matching interface in AllInterfaces.
- Impact: A class implementing INotificationHandler<A> and INotificationHandler<B> (a common pattern) or IRequestHandler<X,..> and IRequestHandler<Y,..> silently loses all but one binding; the other request raises a false TWM001 and the other notification is silently never handled.
- Proposed fix: Loop over all matching interfaces and register each binding; add tests.
- Child task: yes

### G-4: Derived handler classes produce false duplicate handlers
- Severity: risk
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:244 iterates type.AllInterfaces, which includes interfaces inherited from a base class; each non-abstract class in the assembly that inherits a concrete handler is added to handlersByRequest (:47-53).
- Impact: `class B : A` where A is a concrete handler is reported as TWM002 for A's request; also affects notifications (handlers run twice).
- Proposed fix: Only count interfaces declared directly on the type (type.Interfaces plus abstract-base chain) or exclude types whose base already implements the handler interface.
- Child task: no (fold into G-3)

### G-5: Type-switch ordering can emit uncompilable or wrong dispatch for inheritance and interfaces
- Severity: bug
- Area: source/timewarp-mediator-generators
- Evidence: emitter.cs:267-288 and :317-332 emit `case <Type> local:` in alphabetical order (graph is sorted by display name, builder.cs:116 and :191). Notification type arguments may be interfaces or base classes (builder.cs:290 accepts any INamedTypeSymbol).
- Impact: When a base type or interface sorts before its derived type the C# compiler reports CS8120 (case subsumed) in generated code; when it compiles, a derived notification is not delivered to handlers of its base (MediatR contravariance is lost) with no diagnostic.
- Proposed fix: Order cases most-derived first (topological sort by assignability) or use exact-type checks; document or diagnose the no-variance behavior.
- Child task: yes

### G-6: Pattern variable names can collide with parameters or C# keywords
- Severity: bug
- Area: source/timewarp-mediator-generators
- Evidence: emitter.cs:631-646 LocalName lower-cases the first letter of the type name and uses it as the pattern variable (`case X request:` at :280, `case X notification:` at :329) with no keyword or parameter escape.
- Impact: A request named `Request`, a notification named `Notification`, or types named `Event`, `Object`, `Lock`, `Params`, `Base`, etc. generate code that fails with CS0136/CS1001; the user sees errors in a .g.cs file only.
- Proposed fix: Use fixed names (`r0`, `r1`...) or prefix with `@`/`__`; add a compilation test with these names.
- Child task: no (fold into G-5 or the test task)

### G-7: Aot/Link profile silently ignores behaviors but manifest reports them
- Severity: bug
- Area: source/timewarp-mediator-generators
- Evidence: emitter.cs:410-427 and :446 skip all ClosedBehaviors when aot; manifest-emitter.cs:53-62 writes request.ClosedBehaviors regardless of profile. No diagnostic is raised.
- Impact: The manifest claims a pipeline that never runs under Aot/Link; behaviors configured with [assembly: MediatorBehavior] are dropped silently, which can disable cross-cutting concerns such as validation or transactions.
- Proposed fix: Either weave behaviors in Aot (static instances) or emit a warning diagnostic and omit them from the manifest in that profile.
- Child task: yes

### G-8: Aot ServiceGen requires parameterless handlers without a diagnostic
- Severity: risk
- Area: source/timewarp-mediator-generators
- Evidence: emitter.cs:162-198 emits `new Handler()` for every handler; the design note (:7-8) states "parameterless handlers only" but nothing in the builder checks for it. Handlers become process-wide singletons (static readonly fields, :176-177) shared across threads.
- Impact: A handler with constructor parameters gives CS7036 in generated code; stateful handlers are shared unexpectedly.
- Proposed fix: Add a TWM diagnostic at the handler location for Aot profile when no accessible parameterless constructor exists; document singleton lifetime.
- Child task: no

### G-9: Internal types in referenced member assemblies are discovered but inaccessible
- Severity: risk
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:638-651 IsEffectivelyPrivate rejects private/protected only; membership.cs:102-108 adds referenced assemblies with [MediatorAssembly]; the emitter writes fully qualified references to every discovered handler, request and behavior (emitter.cs:170, :399, :434).
- Impact: An internal handler or request in a referenced member assembly (no InternalsVisibleTo) yields CS0122 in generated code instead of a clear diagnostic. Task 010 only made the generated types internal.
- Proposed fix: In the builder use compilation.IsSymbolAccessibleWithin(type, compilation.Assembly) and report a diagnostic (or skip) when not accessible; add a compilation test.
- Child task: yes

### G-10: Notification handler order is unspecified and not stable across assemblies
- Severity: risk
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:34-36 iterates membership.MemberAssemblies, an ImmutableHashSet keyed by SymbolEqualityComparer (membership.cs:118); handlers are appended in discovery order (:71) and emitted in that order (emitter.cs:354). Requests and notification groups are sorted by name (:116, :191-193) but handlers are not. DuplicateHandler also uses handlerList[1] and the unsorted name list (:109-111).
- Impact: Handler execution order for a notification, and the TWM002 message and location, can differ between builds when several assemblies are members; ordering within one assembly follows source file/namespace order and is undocumented.
- Proposed fix: Sort assemblies by name and handlers by display name (or give an explicit order attribute) and document the rule.
- Child task: no

### G-11: Unsupported MediatR features are silently ignored
- Severity: debt
- Area: source/timewarp-mediator-generators
- Evidence: emitter.cs:379-391 stub CreateStream methods throw NotSupportedException (documented in documentation/m1-generated-mediator.md:63); grep finds no handling of IRequestPreProcessor, IRequestPostProcessor or exception handlers anywhere in the generator or analyzers, and the deferred table (m1 doc:56-66) lists only the exception cascade.
- Impact: Pre/post processors and stream handlers registered by a migrating user are ignored without a warning, and stream requests compile but throw at runtime. Stream handlers also produce no TWM001/TWM002 coverage.
- Proposed fix: Add a diagnostic when an IStreamRequestHandler, pre/post processor or exception handler exists in a member assembly; list processors in the deferred table.
- Child task: yes

### G-12: Behavior cancellation token fallback replaces an explicit CancellationToken.None
- Severity: nit
- Area: source/timewarp-mediator-generators
- Evidence: emitter.cs:477-489 passes `t == default ? cancellationToken : t` into the next link.
- Impact: A behavior that deliberately passes CancellationToken.None (to shield cleanup) gets the outer token anyway.
- Proposed fix: Pass `t` through unchanged, or document the deviation from the reflection mediator.
- Child task: no

### G-13: Invalid or unusable [MediatorBehavior] entries are dropped silently
- Severity: debt
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:340-344 `continue`s when the type is not an IPipelineBehavior; builder.cs:386-396 and TryCloseBehavior (:436-474) skip behaviors that cannot be closed, including when the Scope names a marker no request uses. Arity-1 behaviors are constructed with only the request type (:458-461), which is undocumented.
- Impact: Typos in behavior registration produce no feedback and a pipeline that is silently missing a behavior.
- Proposed fix: Report a diagnostic when a registered behavior is not an IPipelineBehavior, and an info diagnostic when it closes over zero requests.
- Child task: no

### G-14: Scope mismatch check ignores handler-scoped vs unscoped request
- Severity: nit
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:128-144 reports TWM003 only when both scopes are non-null, then uses `handlerScope ?? requestScope`; notification grouping (:169) does the same with no mismatch check. The descriptor text (diagnostic-descriptors.cs:36) says both must match or both be unscoped.
- Impact: A scoped handler for an unscoped request is silently moved into the named pipeline; the description over-promises.
- Proposed fix: Align description with behavior, or report TWM003 when exactly one side is scoped; also check notifications.
- Child task: no

### G-15: Type-name sanitization can collide
- Severity: nit
- Area: source/timewarp-mediator-analyzers
- Evidence: builder.cs:666-683 maps every non-alphanumeric char to `_`, so `A.B_C` and `A_B.C` (or generic variants) share Dispatch_, Publish_ and ServiceGen field names (emitter.cs:621-629). ScopeSuffix (:683-695) can also collide for scopes with the same Name in different namespaces only when sanitized names collide.
- Impact: Rare duplicate-member compile errors in generated code.
- Proposed fix: Append a short stable hash or per-name index on collision.
- Child task: no

### G-16: Release tracking files inconsistent with shipped versions
- Severity: debt
- Area: source/timewarp-mediator-analyzers
- Evidence: AnalyzerReleases.Shipped.md has only the header; AnalyzerReleases.Unshipped.md:5-10 lists TWM001-TWM004, but tags v14.0.0-beta.1 to beta.4 already exist (`git tag`) and v14.0.0-beta.4:source/timewarp-mediator-analyzers/AnalyzerReleases.Shipped.md is also empty.
- Impact: Roslyn's release-tracking analyzers (RS2000 series) treat the rules as never shipped; no history of when each rule was added.
- Proposed fix: Move the rules into a `## Release 14.0.0-beta.1` section of Shipped.md (or the beta that first included them) and leave Unshipped empty; add this to the release checklist.
- Child task: no

### G-17: Descriptors lack help links and the CompilationEnd tag
- Severity: nit
- Area: source/timewarp-mediator-analyzers
- Evidence: diagnostic-descriptors.cs:11-45 omit helpLinkUri and customTags, yet TWM001-003 are reported from a RegisterCompilationEndAction (request-handler-analyzer.cs:35-41).
- Impact: No documentation link in the IDE; compilation-end diagnostics without WellKnownDiagnosticTags.CompilationEnd are mishandled by some hosts and trip RS1037.
- Proposed fix: Add helpLinkUri pointing at the docs and customTags: WellKnownDiagnosticTags.CompilationEnd for TWM001-003. Ids and the shared "TimeWarp.Mediator" category are otherwise consistent.
- Child task: no

### G-18: TWM004 analyzer false negatives and fragile receiver detection
- Severity: debt
- Area: source/timewarp-mediator-analyzers
- Evidence: request-handler-analyzer.cs:123-127 handles only MemberAccessExpression (misses `sender?.Send(...)` and static-extension call shapes); :82 requires the argument's static type to be a concrete named type (variables typed as IRequest<T>/base/object are ignored); calls through unscoped ISender/IMediator with a scoped request are never reported; :66-69 checks only the method name "Send", not its containing type.
- Impact: Wrong-pipeline sends compile cleanly and fail at runtime with NoHandlerException; a user-defined Send on an ISender<T> implementer may be analyzed incorrectly.
- Proposed fix: Handle conditional access, verify method.ContainingType is ISender<>/IMediator/the generated types, and add unscoped-sender coverage if intended.
- Child task: no

### G-19: Analyzer rebuilds the full graph on every compilation start and scans all syntax trees for options
- Severity: debt
- Area: source/timewarp-mediator-analyzers
- Evidence: request-handler-analyzer.cs:32 builds the graph synchronously at CompilationStart; membership.cs:245-273 ReadProperty loops over every syntax tree calling GetOptions(tree) whenever a global option is unset (three keys, always unset for the analyzer-only package which has no props file). No cancellation tokens are used anywhere.
- Impact: Extra O(trees) work and IDE latency; the per-tree fallback is meaningless for build_property.* keys, which are global.
- Proposed fix: Read only GlobalOptions (or the first tree); defer graph building to the compilation-end action; pass a CancellationToken.
- Child task: no

### G-20: Packaging: Roslyn 4.8 floor excludes .NET 6/7 SDKs; unverified in tests
- Severity: risk
- Area: source/timewarp-mediator-generators, source/timewarp-mediator-analyzers
- Evidence: both csprojs pin Microsoft.CodeAnalysis.CSharp to 4.8.0 (generators csproj:26, analyzers csproj:26) and use IIncrementalGenerator. Docs (documentation/generated-vs-legacy.md "Packages") state no minimum SDK. Other layout items check out: IncludeBuildOutput=false, analyzers/dotnet/cs pack path, PrivateAssets=all, buildTransitive props packed under the PackageId-cased name (generators csproj:38).
- Impact: Consumers on SDKs older than 8.0.100 / VS 17.8 get CS9057 or the generator not loading, with no documented requirement. The package packs Analyzers.dll via `$(OutputPath)` (csproj:36-37), which only works after a built ProjectReference copy-local; `pack --no-build` from a clean tree would fail or ship a stale file.
- Proposed fix: Document the minimum SDK in README and docs; consider a CI job that restores the packed nupkg into a sample on the oldest supported SDK.
- Child task: no

### G-21: Generator emits against types that may not be referenced
- Severity: risk
- Area: source/timewarp-mediator-generators
- Evidence: Build() tolerates missing IRequestHandler/IMediator types (builder.cs:36-43 nullable lookups) but EmitMediator always emits `global::TimeWarp.Mediator.IMediator` and the DI registration unconditionally (emitter.cs:50, :37-40). The props file marks every referencing project as a host (props:4) and the package is DevelopmentDependency without a Contracts dependency (generators csproj:14-15, doc "Contracts is not transitive").
- Impact: A project that references only the Generators package gets a wall of CS0234/CS0246 errors instead of one actionable diagnostic. Hosts without Microsoft.Extensions.DependencyInjection.Abstractions break the same way under the Host profile.
- Proposed fix: Emit nothing and report one diagnostic when TimeWarp.Mediator.IMediator (or DI abstractions in Host profile) cannot be resolved.
- Child task: no

### G-22: Dead code and leftovers
- Severity: nit
- Area: both
- Evidence: builder.cs:29 and :40 declare/resolve Notification0/notification0 and never use them; emitter.cs:345-348 `if (!aot) { builder.Append(""); }` is a no-op; builder.cs:11 UnitTypeName fallback `unitType ?? request1` (:267) makes the void response type the request type when Unit is missing; the request-handler `TimeWarp.Mediator.IRequestHandler` metadata names are duplicated as strings in several places instead of constants; Membership types, MessageGraph and builders are public in the analyzer assembly (membership.cs:26, message-graph.cs:3) only because the generator references them; Properties/launchSettings.json uses Windows `\\` separators (:5); descriptor text and comments name IAction/ICommand/IQuery (diagnostic-descriptors.cs:18) though discovery keys only on IRequest/IRequest<T> (builder.cs:220). The only MediatR mention is a comment (builder.cs:8) describing GetServices().Reverse().Aggregate semantics; no leftover MediatR identifiers in scope.
- Impact: Noise only.
- Proposed fix: Remove dead lines, make the shared types internal with InternalsVisibleTo, fix the separator, reword the descriptor.
- Child task: no

### G-23: No test coverage for large parts of the generator and analyzer
- Severity: debt
- Area: tests/*
- Evidence: The generators tests (state/, pipelines/, responses/, idempotency/) cover only happy paths in one assembly with Host profile. Grep of tests finds no use of: Aot/Link profile behavior beyond the three visibility theories (host-visibility-tests.cs:12-57), MediatorModule, MediatorAssemblies, TimeWarpMediatorProfile/Namespace overrides, TWM001-003 through the generator, duplicate handlers, scope mismatch via generator, multiple handler interfaces, derived handlers, nested/generic/internal types, keyword-named types, notification ordering, notification base-type delivery, referenced-assembly members, multiple scopes with same type name, AddGeneratedMediator<TScope> with unknown scope (ArgumentException path), CreateStream NotSupportedException, null-argument guards, and incrementality/caching. Analyzer tests (request-handler-analyzer-tests.cs) have one positive case per rule: no TWM004 negative (matching or unscoped sender), no location assertions, no cross-assembly or MSBuild-property membership tests.
- Impact: Most items above (G-3 to G-9) could regress or ship unnoticed; there is no compile-the-output check for odd shapes.
- Proposed fix: Extend HostCompiler-based tests (they already compile in memory) with a table of source shapes asserting zero warnings/errors, plus analyzer negatives and location checks.
- Child task: yes

### G-24: Tests are behavioral, not golden-file, and some assertions are brittle
- Severity: debt
- Area: tests/timewarp-mediator-generators-tests
- Evidence: There are no committed snapshots of generated source; "golden file" in the docs means behavioral State/pipeline tests. Brittle assertions: increment-action-set-tests.cs:21-24 and :119-123 and response-shape-tests.cs:121-125 use `ShouldContain` on exact manifest substrings with fully qualified names and field ordering (e.g. `"request":"...","response":"string[]"`); static PipelineLog with a serial collection (pipeline-log-collection.cs) makes tests order-coupled; AnalyzerTestHelper (analyzer-test-helper.cs:26-39) depends on TRUSTED_PLATFORM_ASSEMBLIES; analyzer tests use `Any(...)` rather than asserting exact diagnostics and locations (request-handler-analyzer-tests.cs:42, :89, :123). Compilation tests reference Microsoft.CodeAnalysis.CSharp without VersionOverride (separate from the shipped 4.8.0 floor), so the generator is never exercised against the 4.8 compiler it claims to support.
- Impact: Cosmetic manifest changes break tests; real generated-code regressions (formatting aside) are not pinned; floor compatibility is untested.
- Proposed fix: Parse the manifest JSON in tests, add Verify-style snapshots for a small number of representative outputs, and run the generator tests once against the 4.8.0 compiler.
- Child task: no

### G-25: Test conventions
- Severity: nit
- Area: tests/*
- Evidence: All three test projects use xunit + Shouldly with ImplicitUsings disabled; consistent with each other and with tests/timewarp-mediator-tests. File names are kebab-case and each file has a Purpose region, matching agent.md. No TODO/FIXME found in scope.
- Impact: None; recorded as a no-finding check.
- Proposed fix: None.
- Child task: no

## Summary
- Counts by severity: bug 4 (G-3, G-5, G-6, G-7); risk 8 (G-1, G-2, G-4, G-8, G-9, G-10, G-20, G-21); debt 7 (G-11, G-13, G-16, G-18, G-19, G-23, G-24); nit 6 (G-12, G-14, G-15, G-17, G-22, G-25). Total 25. Suggested child tasks: G-1, G-2, G-3, G-5, G-7, G-9, G-11, G-23.
- Overall health: the shipped feature set (host profile, named pipelines, Unit/array responses, internal emission) works and packaging layout matches the documented design, but the generator is a first-cut implementation rather than a robust incremental one. The biggest problems are the non-incremental pipeline, handler discovery that binds only one interface per class, a type switch that can emit uncompilable or non-variant dispatch, and an Aot profile that silently drops behaviors while the manifest claims them. Diagnostics coverage is thin: invalid shapes produce compile errors in generated files rather than TWM diagnostics. Test coverage is happy-path only, and there are no tests for the Aot output, multi-assembly membership, or the declared 4.8 compiler floor. Release tracking is stale relative to the tags already published.
