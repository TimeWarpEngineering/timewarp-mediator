# Generator and analyzer test matrix, Roslyn 4.8 floor leg, and release tracking

## Description

Generator and analyzer tests are happy-path only in one assembly under the Host profile, and release tracking says no rule has shipped although four betas are tagged. Parent review task 013, findings G-16, G-20, G-23, G-24, B-16.

- G-23 (debt): no tests for Aot output beyond visibility, `MediatorModule`, `MediatorAssemblies`, profile/namespace overrides, TWM001-003 via the generator, duplicate handlers, multi-interface or derived handlers, nested/generic/internal/keyword-named types, notification ordering or base-type delivery, referenced-assembly members, unknown-scope `ArgumentException`, `CreateStream` `NotSupportedException`, null guards, incrementality. Analyzer tests have one positive case per rule and no location assertions.
- G-24 (debt): manifest assertions use exact substrings; static `PipelineLog` couples test order; analyzer tests use `Any(...)`; compilation tests reference Roslyn 5.9.0 so the 4.8.0 floor the packages declare is never exercised.
- G-16, G-20, B-16: `AnalyzerReleases.Shipped.md` is empty while v14.0.0-beta.1 to beta.4 exist; docs state no minimum SDK although 4.8.0 means SDK 8.0.100 / VS 17.8.

## Requirements

- Table-driven `HostCompiler` tests asserting zero diagnostics for a set of source shapes, plus analyzer negatives with location checks.
- Parse manifest JSON in tests instead of substring matching.
- One test leg (or CI job) that runs the generator against Microsoft.CodeAnalysis.CSharp 4.8.0.
- Move TWM001-004 to a `## Release 14.0.0-beta.1` section in Shipped.md; add the step to the release checklist.
- Document the minimum SDK in readme and `documentation/generated-vs-legacy.md`.

## Checklist

- [x] Shape matrix tests
- [x] Analyzer negative and location tests
- [x] Manifest JSON parsing
- [x] 4.8.0 compiler leg
- [x] Release tracking files and minimum-SDK docs
- [x] Implementation review disposition clean
- [x] `./bin/dev workflow` green (PR with merge commit is the host's open-pr step)

## Notes

Parent: 013. Prefer landing before the incremental rewrite so it has a safety net.

Review: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 3, roster general. Disposition clean.

## Session

- Created: 686431 (2026-10-11)
- Implemented: 2026-10-12 (implement oracle)
- Review: Grok session 01a12fb7-6d0f-7b23-93ef-efc796fc4ca6 (2026-10-12)
- Review oracle: review by implementer-grok (grok, model grok-4.7), session not reported, max-turns 200 — 2026-10-12T10:53:59Z

## Results

**Tests (G-23, G-24)**

- `tests/timewarp-mediator-generators-compilation-tests/shape-matrix-tests.cs`: table of 10 host
  shapes (nested, internal, keyword-named, closed generic, multi-interface handler, derived handler,
  notification fan-out with base type, scoped pipeline, struct request, empty) x Host/Aot profiles.
  Each must have zero warnings/errors and exactly the request/notification bindings listed.
- `generated-mediator-tests.cs`: referenced members join by `[assembly: MediatorAssembly]` (not
  `[MediatorModule]` alone) and `[MediatorAssemblies]` markers, runtime dispatch to a referenced
  handler, profile + namespace overrides (Host/Aot/Link), TWM001–TWM003 from the generator with
  exact message and location, notification base-type delivery and order, struct dispatch, null
  guards, `NoHandlerException`, `CreateStream` `NotSupportedException`, and
  `AddGeneratedMediator<TScope>` unknown-scope `ArgumentException`.
- `analyzer-negative-tests.cs`: 10 near-miss shapes x library/host that must not be diagnosed, plus
  location checks (`TWM001` per orphan, `TWM004` per wrong send, `[MediatorModule]` membership).
  The `Any(...)` assertions in `request-handler-analyzer-tests.cs` are now single-diagnostic checks
  with exact message and location.
- `manifest-document.cs` parses `MediatorManifest.Json` (System.Text.Json); all manifest substring
  assertions in compilation and runtime generator tests now check parsed fields.
- `PipelineLog` is per-async-flow (`AsyncLocal`), so the `PipelineLog` xunit collection that
  serialized four test classes is removed.
- `tests/timewarp-mediator-generators-roslyn-floor-tests`: new project in the slnx that links every
  compilation test and compiles against Microsoft.CodeAnalysis.CSharp **4.8.0**
  (`VersionOverride`), with a guard test that the loaded compiler is 4.8. It runs in
  `dotnet test`/`./bin/dev workflow`, so CI covers it with no workflow change.

**Product fix found by the matrix**: a `struct` request made generated code fail with CS0037
(`request is null` on a value type). `mediator-emitter.cs` now emits the null guard only for
reference-type requests. Recorded in `documentation/release-notes/release-notes-v14.0.0.md`.

**Release tracking (G-16, G-20, B-16)**

- TWM001–TWM004 moved from `AnalyzerReleases.Unshipped.md` to `AnalyzerReleases.Shipped.md`.
  Deviation: the header is `## Release 14.0.0`, not `## Release 14.0.0-beta.1`, because RS2007
  rejects headers that are not a `System.Version` (the build failed with the beta label). Each row's
  Notes column says `shipped in 14.0.0-beta.1`, and a file comment explains the convention.
- `documentation/overview.md`: release checklist step to move Unshipped rows to Shipped, plus an
  "Analyzer release tracking" section that explains the prerelease convention.
- Minimum SDK (Roslyn 4.8 = .NET SDK 8.0.100 / VS 2022 17.8) documented in `readme.md` and
  `documentation/generated-vs-legacy.md`.

**Note:** a shared VBCSCompiler server that another worktree started can hold an older
`TimeWarp.Mediator.Analyzers` 14.0.0.0. The generator then fails with CS8784
(`MembershipOptions` TypeLoadException). Run with `UseSharedCompilation=false` or
`dotnet build-server shutdown`. This comes from the environment, not this change.

### How to validate

**Smoke**

```bash
UseSharedCompilation=false ./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-roslyn-floor-tests -c Release --filter "FullyQualifiedName~RoslynFloorTests|FullyQualifiedName~ShapeMatrixTests"
dotnet test tests/timewarp-mediator-analyzers-tests -c Release --filter "FullyQualifiedName~AnalyzerNegativeTests"
ganda repo audit
```

**Expect**

- Workflow ends `Pipeline SUCCEEDED`. Test counts: generators-tests 40, analyzers-tests 43,
  generators-compilation-tests 85, generators-roslyn-floor-tests 86, mediator-tests 201 passed / 2 skipped.
- The floor run shows `Microsoft.CodeAnalysis.CSharp/4.8.0` in
  `tests/timewarp-mediator-generators-roslyn-floor-tests/bin/Release/net8.0/*.deps.json`, and
  `Compiler_IsTheDeclaredFloor` passes.
- Building `source/timewarp-mediator-analyzers` has no RS2xxx release-tracking errors.
- `ganda repo audit` prints "Repository passes all audit checks."

### Review

- Rounds: 1. Roster: general. Effort: 3.
- Round 1: no findings.
- Final counts: bug 0 open / 0 fixed / 0 wontfix. Suggestion 0. Nit 0.
- Disposition: **clean**. No wontfix. No escalation.
- Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.
- Re-checked (Release): `timewarp-mediator-generators-roslyn-floor-tests` 86 passed, deps pin `Microsoft.CodeAnalysis.CSharp/4.8.0`; `timewarp-mediator-generators-compilation-tests` 85 passed; `timewarp-mediator-analyzers-tests` 43 passed; `timewarp-mediator-generators-tests` 40 passed.
