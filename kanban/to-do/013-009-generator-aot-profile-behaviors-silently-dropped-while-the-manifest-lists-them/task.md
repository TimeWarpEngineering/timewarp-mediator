# Generator Aot profile: behaviors silently dropped while the manifest lists them

## Description

Under the Aot/Link profile the emitter skips every closed behavior, but the manifest still lists them and no diagnostic fires. Parent review task 013, findings G-7, G-8.

- G-7 (bug): `mediator-emitter.cs` skips `ClosedBehaviors` when `aot`; `manifest-emitter.cs` writes them regardless. Validation or transaction behaviors configured via `[assembly: MediatorBehavior]` vanish silently.
- G-8 (risk): Aot `ServiceGen` emits `new Handler()` for every handler. A handler with constructor parameters yields CS7036 in generated code; handlers become process-wide singletons in `static readonly` fields with no documentation.

## Requirements

- Either weave behaviors in Aot (static instances with the same parameterless rule) or emit a warning diagnostic and omit them from the manifest in that profile.
- Add a TWM diagnostic at the handler location when the Aot profile is active and the handler has no accessible parameterless constructor.
- Document singleton lifetime for Aot handlers.
- Tests: Aot profile with a behavior; Aot handler with constructor parameters.

## Checklist

- [x] Decide weave vs diagnose (record in Notes)
- [x] Implement with tests
- [x] Manifest matches emitted pipeline
- [x] Parameterless-constructor diagnostic with test
- [x] Docs; `./bin/dev workflow` green
- [ ] PR with merge commit (host open-pr)

## Notes

Parent: 013.

**Decision: diagnose, not weave.** Behaviors usually take DI dependencies (loggers, validators,
DbContext) and per-request state; the Aot profile has no `IServiceProvider`, so static singleton
behaviors would be wrong more often than useful. Aot/Link reports **TWM005** (warning) once per
registered behavior that closes over a request, at the `[assembly: MediatorBehavior]` attribute,
and the manifest lists `"behaviors":[]` so it matches the emitted `Dispatch_*`.

**TWM006** (error) at the handler when Aot/Link is active and the handler has no constructor
accessible from the host assembly whose parameters are all optional. ServiceGen emits `default!`
for such a handler so TWM006 is the only error (no CS7036 in generated code).

Descriptors live in the shared `DiagnosticDescriptors` (analyzers assembly, now
`InternalsVisibleTo` the generator) and are tracked in `AnalyzerReleases.Unshipped.md`; only the
generator reports them (`AotProfile` in `aot-profile.cs`).

## Session

- Created: 686431 (2026-10-11)
- Implementer: Claude commit `d86e920` (2026-10-12)
- Review: Grok session `01a12f38-b61a-7b50-9b71-466403888c2c` (2026-10-12)

## Results

- `source/timewarp-mediator-generators/aot-profile.cs` — new `AotProfile.Analyze`: TWM005 per woven-in-Host behavior, TWM006 per unconstructible handler (request + notification handlers; private/required-param ctors; optional params OK; `required` members unless `[SetsRequiredMembers]`).
- `mediator-generator.cs` reports AotProfile diagnostics; `mediator-emitter.cs` ServiceGen uses `default!` for TWM006 handlers; design comment documents singleton lifetime.
- `manifest-emitter.cs` — empty `behaviors` in Aot/Link.
- `diagnostic-descriptors.cs` + `AnalyzerReleases.Unshipped.md` — TWM005 (Warning) / TWM006 (Error); analyzers csproj `InternalsVisibleTo` generator.
- Docs: `documentation/generated-vs-legacy.md` new "Aot / Link profile limits" section (singleton handlers, TWM005, TWM006); `documentation/m1-generated-mediator.md` updated.
- Tests: `tests/timewarp-mediator-generators-compilation-tests/aot-profile-tests.cs` (Aot+Link behavior → TWM005 at attribute; Aot manifest omits behaviors, Host lists them; behavior throws if it runs, and the Aot runtime test still expects `"pong"`; ctor-param / private-ctor / notification handler → TWM006 at handler, optional-param ctor accepted; required property and inherited required member → TWM006 with no CS9035; `[SetsRequiredMembers]` compiles; Host unaffected).
- `./bin/dev workflow` green on the implement commit (compilation tests 28/28, generators 40/40, analyzers 6/6, mediator 201 passed / 2 skipped; pack layout verified). Review re-ran `AotProfileTests`: 13 passed, 0 failed. `ganda repo audit` passed (30 passed, 0 failed).

### Review

- Rounds: 2. Roster: general. Effort: 2 (537-line implement diff).
- Round 1: 2 suggestions open (M1 required members → CS9035, M2 runtime behavior test not discriminating). 0 bugs, 0 nits.
- Round 2: both fixed, no new issues. Final counts: bug 0/0/0, suggestion 0 open / 2 fixed / 0 wontfix, nit 0/0/0.
- Disposition: **clean**. No wontfix, no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

**Smoke:**

```bash
dotnet build-server shutdown
./bin/dev workflow
dotnet test tests/timewarp-mediator-generators-compilation-tests -c Release --no-build --filter "FullyQualifiedName~AotProfileTests"
```

**Expect:** workflow ends `Pipeline SUCCEEDED`; the filtered run reports `Passed: 13, Failed: 0`
(TWM005 at `MediatorBehavior(typeof(Pipes.LoggingBehavior<,>))` for Aot and Link, Aot manifest
`"behaviors":[]`, exactly three TWM006 errors on `DependentHandler`, `DependentNoteHandler`,
`PrivateCtorHandler`, no CS7036, TWM006 on `RequiredMemberHandler` and `DerivedRequiredHandler`
with no CS9035, and no TWM006 when the constructor has `[SetsRequiredMembers]`).
`dotnet build-server shutdown` avoids a stale
`TimeWarp.Mediator.Analyzers.dll` in VBCSCompiler (`MissingFieldException` / CS8785) after
switching from an older build.

