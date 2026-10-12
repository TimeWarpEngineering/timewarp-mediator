# Generator and analyzer: replace generated-code compile errors with TWM diagnostics

## Description

Invalid or unsupported shapes surface as compiler errors inside `.g.cs` files instead of actionable diagnostics. Parent review task 013, findings G-9, G-11, G-13, G-14, G-17, G-18, G-21.

- G-9 (risk): internal handlers or requests in a referenced member assembly without `InternalsVisibleTo` produce CS0122. `IsEffectivelyPrivate` rejects only private/protected.
- G-21 (risk): a project referencing only the Generators package gets a wall of CS0234/CS0246 because `IMediator` and DI abstractions are emitted unconditionally.
- G-11 (debt): `IStreamRequestHandler`, pre/post processors, and exception handlers in member assemblies are ignored with no warning; stream requests compile and throw `NotSupportedException` at runtime (message embeds internal ids "004-001"/"004-002").
- G-13 (debt): `[MediatorBehavior]` entries that are not `IPipelineBehavior`, or close over zero requests, are dropped silently.
- G-14 (nit): TWM003 fires only when both sides are scoped; the descriptor says both must match or both be unscoped; notifications are not checked.
- G-17 (nit): descriptors lack `helpLinkUri` and `WellKnownDiagnosticTags.CompilationEnd` for TWM001-003 reported at compilation end.
- G-18 (debt): TWM004 misses `sender?.Send(...)`, variables typed as `IRequest<T>`, unscoped `ISender`/`IMediator` with a scoped request, and matches on method name only.

## Requirements

- Use `compilation.IsSymbolAccessibleWithin` in the builder; report a diagnostic for inaccessible members.
- Emit nothing and report one diagnostic when `TimeWarp.Mediator.IMediator` (or DI abstractions under Host) cannot be resolved.
- New diagnostics for unsupported handler kinds and invalid behavior registrations; update the deferred table in `documentation/m1-generated-mediator.md`.
- Align TWM003 description or behavior; add help links and the CompilationEnd tag.
- Harden TWM004 receiver detection.
- Reword the `NotSupportedException` messages without internal ids.
- Add new ids to `AnalyzerReleases.Unshipped.md`.

## Checklist

- [x] G-9 accessibility diagnostic with test
- [x] G-21 missing-reference guard with test
- [x] G-11 unsupported-shape diagnostics with tests; doc table updated
- [x] G-13 behavior-registration diagnostics with tests
- [x] G-14, G-17 descriptor fixes
- [x] G-18 TWM004 hardening with negative tests
- [ ] `./bin/dev workflow` green (done locally); PR with merge commit (host open-pr)

## Notes

Parent: 013.

## Session

- Created: 686431 (2026-10-11)
- Implemented: 2026-10-12 (implement oracle)
- Review: Grok session 01a12f68-d196-7263-b637-322de95fe26c (2026-10-12)
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 200 — 2026-10-12T10:01:43Z

## Results

New diagnostics TWM007–TWM011, all generator-only, reported through `MessageGraph.GeneratorDiagnostics`.
The analyzer-only package still reports only TWM001–TWM004.

- **G-9, TWM007 (Error).** `AccessGuard` in `MessageGraphBuilder` uses `compilation.IsSymbolAccessibleWithin`.
  It checks each handler, request, response, notification, behavior definition, and the pipeline scope
  marker that generated `ISender<TScope>` / `IPublisher<TScope>` will name, including generic arguments
  and array element types. An inaccessible type is not bound. TWM007 fires once per type and suggests
  `InternalsVisibleTo("<host>")`. The orphaned request does not also get TWM001. Inaccessible request
  types are no longer collected for TWM001. An inaccessible scope skips that request or notification binding.
- **G-21, TWM008 (Error).** `MediatorGenerator.FindMissingRuntime` runs before the graph is built. It
  needs `TimeWarp.Mediator.IMediator`, and under Host it also needs `IServiceCollection`,
  `ServiceCollectionServiceExtensions`, and `ServiceProviderServiceExtensions`. If any is missing, the
  generator reports one TWM008 and emits nothing.
- **G-11, TWM009 (Warning).** Fires for each class and ignored kind: `IStreamRequestHandler`,
  `IStreamPipelineBehavior`, `IRequestPreProcessor`, `IRequestPostProcessor`, `IRequestExceptionHandler`,
  `IRequestExceptionAction`. The `CreateStream` `NotSupportedException` text no longer has 004-001/004-002.
  It now points at the reflection `AddMediator` runtime. The deferred table in
  `documentation/m1-generated-mediator.md` now lists these kinds.
- **G-13, TWM010 and TWM011 (Warning).** TWM010: a `[MediatorBehavior]` type that is not an
  `IPipelineBehavior`. TWM011: a valid behavior that closes over no request in its pipeline (constraints
  or `Scope`). Both point at the attribute when it is in this compilation. `BehaviorRegistration` now has
  a `Location`.
- **G-14, TWM003.** Behavior is unchanged: when only one side is scoped, the pair joins that pipeline.
  The description now says so. Notifications are now checked too (`ReportScopeMismatch`). The message
  says "message" instead of "request".
- **G-17.** Every descriptor has a `helpLinkUri` to `documentation/diagnostics.md#twm0NN`, which is new.
  TWM001–TWM003 carry `WellKnownDiagnosticTags.CompilationEnd`.
- **G-18, TWM004.** Rewritten on `IInvocationOperation`:
  - `sender?.Send(x)` is covered.
  - Only calls that bind to `ISender.Send` count (the interface member or the receiver's implementation
    of it). Extension methods and unrelated `Send` methods are ignored.
  - `Send(object)` is never reported.
  - Two cases need the whole program and run only in a generator host (`Membership.IsHost`, from
    `TimeWarpMediatorAssembly=true` or the generator): an unscoped `ISender`/`IMediator` sending a
    scoped request, and an `IRequest<T>`/abstract-typed argument when every bound request it could be
    is in another pipeline.
  - The message now names the sender as `'ISender<X>'` or `'ISender (unscoped)'`.
- Generator and analyzer diagnostic locations are restricted to this compilation's syntax trees.
- New ids are in `AnalyzerReleases.Unshipped.md`.

Tests: `tests/timewarp-mediator-analyzers-tests/send-scope-analyzer-tests.cs` (12) and
`tests/timewarp-mediator-generators-compilation-tests/generator-diagnostics-tests.cs` (12 cases).
`HostCompiler` gained `CompileWithReferences`, `CompileLibrary`, and `GetSystemReferences`. The analyzer
test helper gained `isHost`.

Local note: other sessions share the compiler server (VBCSCompiler), and it can serve a stale
`TimeWarp.Mediator.Analyzers.dll` with the same identity. That shows up as CS8785 MissingMethodException
on `GeneratorDiagnostics`. Run the workflow with `UseSharedCompilation=false` locally. CI is unaffected.

### How to validate

Smoke:

```bash
UseSharedCompilation=false ./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-compilation-tests -c Release --filter "FullyQualifiedName~GeneratorDiagnosticsTests"
dotnet test tests/timewarp-mediator-analyzers-tests -c Release --filter "FullyQualifiedName~SendScopeAnalyzerTests"
```

Expect:

- The workflow ends with `Pipeline SUCCEEDED`. Analyzer tests: 18 passed. Generators tests: 40 passed.
  Compilation tests: 47 passed. Core tests: 201 passed, 2 skipped. The Generators nupkg layout is verified.
- `GeneratorDiagnosticsTests`:
  - An internal member handler gives only TWM007, with no CS0122 and no TWM001. With `InternalsVisibleTo` it binds.
  - An internal assembly-level scope marker gives only TWM007, with no CS0122, and the handlers stay out of the manifest. With `InternalsVisibleTo` the request, notification, and marker bind.
  - No Contracts gives one TWM008 and zero generated trees. Host without MS.DI gives one TWM008. Aot without MS.DI generates.
  - A stream handler and a pre-processor give two TWM009 warnings. The `CreateStream` message has no `004-`.
  - A non-behavior gives TWM010 at the attribute. A constrained or empty-scope behavior gives TWM011.
- `SendScopeAnalyzerTests`:
  - Notification TWM003 is reported. One-side-scoped pairs are clean.
  - `sender?.Send` gives TWM004.
  - `Send(object)`, extension or unrelated `Send` methods, library-mode unscoped and interface sends, and
    an interface send that has a match are all clean.
  - In a host, an unscoped `IMediator` with a scoped request and an `IRequest<int>` with only
    other-pipeline candidates give TWM004.
  - Every descriptor has a help link. TWM001–TWM003 are CompilationEnd.

### Review

- Rounds: 2. Roster: general. Effort: 3.
- Round 1: 1 open bug (M1). 0 suggestions. 0 nits.
- M1: an inaccessible pipeline scope marker in a referenced member assembly was still named by generated `ISender<TScope>` / `IPublisher<TScope>` (CS0122). Fixed on this task. Round 2 confirmed it.
- Final counts: bug 0 open / 1 fixed / 0 wontfix. Suggestion 0. Nit 0.
- Disposition: **clean**. No wontfix. No escalation.
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.
