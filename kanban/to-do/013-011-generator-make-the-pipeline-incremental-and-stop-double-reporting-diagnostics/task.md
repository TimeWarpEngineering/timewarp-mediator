# Generator: make the pipeline incremental and stop double-reporting diagnostics

## Description

The generator rebuilds the whole message graph on every compilation and reports the same diagnostics as the bundled analyzer. Parent review task 013, findings G-1, G-2, G-19.

- G-1 (risk): `mediator-generator.cs` combines `CompilationProvider` with options and does all discovery and emission inside one `RegisterSourceOutput`. It captures `Compilation` and `ISymbol`s, so nothing is cacheable and every IDE keystroke re-emits everything.
- G-2 (risk): the Generators nupkg packs Analyzers.dll and its props set `TimeWarpMediatorAssembly=true`, so the analyzer also reports `graph.Diagnostics`. TWM001-003 are likely reported twice in generator hosts. No test asserts the count.
- G-19 (debt): the analyzer rebuilds the graph at `CompilationStart`; `Membership.ReadProperty` loops over every syntax tree for global `build_property.*` keys; no cancellation tokens.

## Requirements

- Split into syntax/semantic providers producing equatable models (strings, `ImmutableArray` of records); emit from those; add `WithTrackingName` steps and a caching test.
- Single reporter for TWM001-003: either drop the analyzer from the generator package or have the generator skip reporting when the analyzer is present. Test that compiles with both and asserts one TWM001.
- Read only `GlobalOptions`; defer graph build to compilation end; pass cancellation tokens.

## Checklist

- [x] Equatable model layer with tracking names
- [x] Incrementality test (edit unrelated file, assert cached steps)
- [x] Single-reporter decision implemented with test
- [x] Analyzer perf fixes (G-19)
- [x] Implementation review disposition clean
- [ ] `./bin/dev workflow` green; PR with merge commit (workflow green; PR is the host open-pr node)

## Notes

Parent: 013. Largest generator task; consider landing after the correctness tasks so tests exist first.

Review: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 2, roster general. Disposition clean.

## Session

- Created: 686431 (2026-10-11)
- Implemented: 2026-10-12 (implement oracle, ganda task work)
- Review: Grok session 01a12f98-c1ac-7f41-be3c-dfcbe39395a6 (2026-10-12)

## Results

### What changed

- **G-1 incremental pipeline** (`source/timewarp-mediator-generators/mediator-generator.cs`, new
  `mediator-model.cs`, `equatable-array.cs`, `tracking-names.cs`, `is-external-init.cs`):
  `AnalyzerConfigOptionsProvider.Select` -> `MembershipOptions` (equatable, step
  `TimeWarpMediator.Options`); `CompilationProvider.Combine(options).Select` -> `MediatorModel`
  (rendered `GeneratedSource(HintName, Text)` + `DiagnosticInfo(descriptor, message, severity,
  LocationInfo(path, span, lineSpan))`, no `Compilation`/`ISymbol`; step `TimeWarpMediator.Model`);
  `.Select` -> `Sources` / `Diagnostics`. `AddSource` runs only when the rendered text changes.
  The diagnostics output re-joins the compilation only to put spans back on their syntax tree so
  diagnostics stay in-source (squiggles, `Location.SourceTree`). Emitters now append to a
  `List<GeneratedSource>` instead of a `SourceProductionContext`.
  - Scope decision: the graph itself still builds from the `Compilation` (whole-program symbol
    info: referenced member assemblies, generic-constraint closing of behaviors). Splitting
    discovery into per-declaration syntax providers would mean rewriting `MessageGraphBuilder` on
    strings; the cost win that matters (no re-emit / re-parse of generated trees per keystroke) is
    delivered by the equatable model.
- **G-2 single reporter**: generator package props now set `TimeWarpMediatorGenerator=true`
  (compiler-visible). The bundled `RequestHandlerAnalyzer` reads it: in a generator host it treats
  the compilation as a host (IsHost) and does **not** report TWM001-TWM003; the generator does
  (live IDE feedback, not compilation-end only). Libraries without the generator still get
  TWM001-TWM003 from the analyzer. `documentation/diagnostics.md` updated.
- **G-19 analyzer perf**: `Membership.ReadProperty` per-syntax-tree loop removed; build properties
  come only from `GlobalOptions` via `MembershipOptions.Read`. Membership + graph are a
  `Lazy<MessageGraph?>` built on the first `ISender.Send` invocation or at compilation end, not in
  `CompilationStart`. `MessageGraphBuilder.Build` and `Membership.TryCreate`/`ForGenerator` take a
  `CancellationToken` and check it per type.
- Tests: `HostCompiler` now runs generator **and** analyzer (as the package does) with
  `TimeWarpMediatorGenerator=true`. New `incremental-generator-tests.cs` (unrelated edit -> Options
  Cached, Model Unchanged, Sources + AddSource output Cached, identical text; new handler -> Model
  Modified; generator + analyzer -> exactly one TWM001 at the request's source span). Analyzer
  tests: generator host leaves TWM001 to the generator; TWM004 still reported in a generator host.
  Verified the one-TWM001 test fails (2x TWM001) when the analyzer guard is removed.

### How to validate

Smoke:

```bash
dotnet build-server shutdown   # stale analyzer cache in a long-lived compiler server -> TypeLoadException
./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-compilation-tests -c Release --filter "FullyQualifiedName~IncrementalGeneratorTests"
dotnet test tests/timewarp-mediator-analyzers-tests -c Release --filter "FullyQualifiedName~GeneratorHost"
```

Expect:

- `./bin/dev workflow` ends `Pipeline SUCCEEDED`; analyzers 20/20, generators 40/40,
  generators-compilation 50/50, mediator 201 passed (2 skipped); package layout verified.
- `IncrementalGeneratorTests`: 3 passed (unrelated edit cached, new handler modified, one TWM001).
- Analyzer filter: `GeneratorHost_LeavesTwm001ToTheGenerator` and
  `Twm004_UnscopedSenderWithScopedRequest_IsErrorInGeneratorHost` pass.
- `ganda repo audit` passes all checks.

### Review

- Rounds: 1. Roster: general. Effort: 2.
- Round 1: no findings.
- Final counts: bug 0 open / 0 fixed / 0 wontfix. Suggestion 0. Nit 0.
- Disposition: **clean**. No wontfix. No escalation.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.
- Re-checked: `IncrementalGeneratorTests` 3 passed; `timewarp-mediator-generators-compilation-tests` 50 passed; `timewarp-mediator-analyzers-tests` 20 passed (Release).
