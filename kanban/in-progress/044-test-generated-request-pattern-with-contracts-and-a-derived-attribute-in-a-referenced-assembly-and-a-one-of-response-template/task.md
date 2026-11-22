# Test GeneratedRequestPattern with contracts and a derived attribute in a referenced assembly and a OneOf response template

## Description

Task 043 shipped `GeneratedRequestPatternAttribute` (15.0.0-beta.10, released). A review of the
merged code against its first consumer (timewarp-architecture task 483, via timewarp-state task
160) found the code paths should handle the consumer's shape, but no test or sample proves it:

- Every test in `generated-request-pattern-tests.cs` / `.sources.cs` and the sample keep the
  contract types and the derived attribute **in the host compilation**. The architecture
  consumer has them in a **referenced project** (Web.Contracts), and the derived attribute
  (`[ContractState]`) in a referenced analyzer/attributes package.
- Only the simple `IRequest<{ContractType}.Response>` interface template is tested. The
  consumer's contracts respond with `OneOf<{ContractType}.Response, SharedProblemDetails>`,
  a nested generic argument.
- `DerivedAttribute_FromMetadataOnlyReference_IsReadFromConstFields` covers a metadata-only
  derived attribute, but with same-compilation contracts.

Resolution of `{ContractType}` is by dotted name through `request-pattern.type-names.cs:17-57`,
which should find referenced types; duplicate metadata names across assemblies and nested
generic contracts are the risks. This task adds the proof and fixes whatever it finds.

## Goal (implementer restatement)

Prove that `GeneratedRequestPatternAttribute` works in the shape timewarp-architecture uses, and
fix whatever the proof breaks. The contracts (each with a nested `Response`) live in one referenced
assembly, the derived pattern attribute with const templates lives in another referenced
(metadata-only) assembly, and the host only applies the attribute. Acceptance:

- A compilation test with referenced contracts + a referenced derived attribute whose interface
  template is a two-arity generic response (`IRequest<OneOf.OneOf<{ContractType}.Response,
  Ns.SharedProblemDetails>>`, with a OneOf stand-in in its own assembly): no errors, the Send arm
  is emitted, a woven behavior closes, and `Send` returns the handler's response.
- A void template naming an interface from a referenced assembly (stand-in for
  `TimeWarp.State.IAction`) plus a behavior from that assembly constrained on it: the behavior runs
  on the declared request.
- TWM013 when a referenced contract's `{ContractType}.Response` does not resolve, and the
  request-missing TWM013 names the referenced contract.
- The duplicate-metadata-name risk is probed (a same-named inaccessible type in another reference);
  any generator/analyzer defect found is fixed with a Design-region note and a beta.11 bump.
- Docs: one paragraph saying contracts and derived attributes may live in referenced assemblies,
  and that the declaration must name the interface a behavior is constrained on.
- Gates: `./bin/dev build` 0/0, the edited test project's request-pattern tests, `ganda repo audit`.

## Checklist

- [x] Test: contracts (with a nested `Response` record) compiled into a separate assembly referenced by the host; a derived attribute with const templates in another referenced assembly; host declares `[Derived(typeof(Ns.CreateThing))]`; a test sibling generator emits the nested request and handler; `Send` dispatches and the response round-trips
- [x] Test: interface template `TimeWarp.Mediator.IRequest<OneOf.OneOf<{ContractType}.Response, Ns.SharedProblemDetails>>` (or an equivalent two-arity generic defined in the test, if adding the OneOf package to tests is undesirable) resolves and dispatches
- [x] Test: a void interface template naming an interface from a referenced assembly (stand-in for `TimeWarp.State.IAction`) and a behavior constrained on that interface runs on the declared request
- [x] Test: TWM013 reports when the referenced contract type name does not resolve
- [x] Any generator or analyzer fix the tests expose, with a Design-region note
- [x] Readme/docs: one paragraph stating that contracts and derived attributes may live in referenced assemblies, and the "declaration must name the interface a behavior is constrained on" limitation
- [x] `dev build` 0/0, `dev test`, `ganda repo audit` clean; `dev check-version` (bump to beta.11 only if generator or analyzer code changes; tests-only needs no bump)
- [x] No cross-repo work; stop and report if the consumer shape needs a Mediator surface change

## Results

New tests in `tests/timewarp-mediator-generators-compilation-tests/generated-request-pattern-tests.referenced.cs`
(sources in `generated-request-pattern-tests.referenced-sources.cs`), all on `GeneratedRequestPatternTests`:

- `ReferencedContractsAndDerivedAttribute_OneOfResponse_Dispatch`: `Web.Contracts` (contracts with
  nested `Response` records and `SharedProblemDetails`), a `OneOf` stand-in assembly, and a
  metadata-only `State.Attributes` assembly with `[ContractState]` const templates using
  `TimeWarp.Mediator.IRequest<OneOf.OneOf<{ContractType}.Response, Ns.SharedProblemDetails>>`.
  Both Send arms emit, the woven `Tag<,>` closes over the OneOf response, and `Send` returns
  `0:Response { Name = widget }` and `1:conflict` (both OneOf branches round-trip).
- `VoidTemplate_NamingAReferencedInterface_ClosesTheReferencedBehavior`: `TimeWarp.State.IAction`
  stand-in and a behavior constrained on it, both in a referenced `StateRuntime` assembly; the
  behavior runs, and a host behavior constrained on `TimeWarp.Mediator.IAction` does not close
  (the documented limitation).
- `ReferencedContractWithoutResponse_IsTwm013_FromTheGenerator` and
  `ReferencedContract_MissingRequest_IsTwm013_NamingTheContract`.
- `ReferencedNames_WithInaccessibleDuplicatesInAnotherReference_StillResolve`: **found a defect.**
  An internal `Ns.SharedProblemDetails` / `Ns.CreateThing+Response` in another reference made
  `Compilation.GetTypeByMetadataName` return null, so the pattern reported a false TWM013 and
  emitted no Send arm. Fixed in `source/timewarp-mediator-analyzers/request-pattern.type-names.cs`
  (shared by generator and analyzer): on a miss, fall back to `GetTypesByMetadataName` and take the
  single type accessible within the host assembly (two accessible types stay unresolved, matching
  CS0433). Design-region note added.

Because analyzer/generator code changed: version bumped to `15.0.0-beta.11`
(`source/Directory.Build.props`, readme/skill install lines), release note
`documentation/release-notes/15.0.0-beta.11.md`. Docs paragraph added to
`source/timewarp-mediator/readme.md` and `documentation/architecture.md` (referenced assemblies +
"declaration must name the interface a behavior is constrained on").

No Mediator public surface change; no cross-repo work.

Test helper change: `Load(host, params byte[][] companions)` loads several referenced runtime images.

### Gates

- `./bin/dev build` — passed (0 errors; product projects build with TreatWarningsAsErrors).
- `dotnet test tests/timewarp-mediator-generators-compilation-tests/timewarp-mediator-generators-compilation-tests.csproj -c Release --filter "FullyQualifiedName~GeneratedRequestPatternTests"` — Passed 18/18 (5 new + 13 existing). This is an xunit csproj, not a Jaribu runfile (no `#if !JARIBU_MULTI` guard), and `./bin/dev test` takes no file arguments; CI runs the full suite. Before the resolver fix the duplicate-name test failed with the false TWM013.
- `./bin/dev check-version` — `15.0.0-beta.11` is new (latest on NuGet 15.0.0-beta.10).
- `ganda repo audit` — passed with 1 advisory (`vscode-window-icon`, missing ignored `.vscode/settings.json`); `ganda repo audit --fix` fixed it (local ignored file only), re-run: all checks pass.

### How to validate

Smoke:

```bash
./bin/dev build
dotnet test tests/timewarp-mediator-generators-compilation-tests/timewarp-mediator-generators-compilation-tests.csproj -c Release --filter "FullyQualifiedName~GeneratedRequestPatternTests"
```

Expect: build succeeds; 18 tests pass, including the five `Referenced*` / `VoidTemplate_NamingAReferencedInterface_*`
tests. Reverting `ByMetadataName` in `request-pattern.type-names.cs` to a plain
`GetTypeByMetadataName` makes `ReferencedNames_WithInaccessibleDuplicatesInAnotherReference_StillResolve` fail with TWM013.

### Review

Round 1, roster general, effort 2. Counts: bug 0 open / 0 fixed / 0 wontfix, suggestion 0, nit 0. Disposition: **clean**. `## Goal (implementer restatement)` matches the original checklist (referenced contracts, metadata-only derived attribute, two-arity OneOf response, referenced void interface and its behavior, TWM013 when `{ContractType}.Response` does not resolve, inaccessible duplicate-name fix with a Design-region note, docs paragraph, beta.11 bump). No misread. Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`. Gates were not re-run; the implementer's record above covers build, the request-pattern compilation tests, version check, and audit.

### Evidence boundaries

- **Proved:** none.
- **Trusted assumptions:** Roslyn `Compilation.GetTypeByMetadataName`, `GetTypesByMetadataName`, and `IsSymbolAccessibleWithin` match the accessibility the C# compiler uses when the host assembly names a type. The test `OneOf.OneOf<T0, T1>` (implicit conversions, `Index:Value` text) is the stand-in the task allowed instead of the OneOf package.
- **Empirical checks:** recorded by the implementer, not re-run in review. `./bin/dev build` passed with 0 errors under TreatWarningsAsErrors. `dotnet test tests/timewarp-mediator-generators-compilation-tests/timewarp-mediator-generators-compilation-tests.csproj -c Release --filter "FullyQualifiedName~GeneratedRequestPatternTests"` passed 18/18. `./bin/dev check-version` reported `15.0.0-beta.11` as new. `ganda repo audit` passed after `--fix` wrote a local ignored `.vscode/settings.json`.
- **Out of scope:** the other test suites (CI and the host audit node), two accessible duplicates of one metadata name (documented as unresolved; the consumer case is the inaccessible duplicate), and any edit in timewarp-state or timewarp-architecture.

## Session

- 2026-11-22 implement oracle: tests + resolver fix + beta.11 bump; audit advisory fixed via `--fix`.

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-17)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 185bce2a-93a7-4d7b-bb06-2b65ebaf3c79, max-turns 500, Done — 2026-11-22T13:46:03Z
- Review oracle: grok (xAI), session 01a20381-de97-7c62-a2fa-668d1dba30ac, effort 2, roster general, round 1, disposition clean — 2026-11-22
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a20381-de97-7c62-a2fa-668d1dba30ac, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-22T13:54:28Z

## Review

- Round 1: Done

## Notes

- Filed per Steve 2026-11-17 ("file the follow up") after the surface review of PR #137.
- Consumers: timewarp-state 160 (declares `TimeWarp.State.IAction` as the interface template),
  then timewarp-architecture 483. Neither is blocked on this task unless a test here fails.
