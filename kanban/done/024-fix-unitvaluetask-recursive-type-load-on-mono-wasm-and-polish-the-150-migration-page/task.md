# Fix Unit.ValueTask recursive type load on Mono WASM and polish the 15.0 migration page

## Goal (implementer restatement)

`Unit.ValueTask` is a static auto-property, so the `Unit` struct has a backing field of type `ValueTask<Unit>`. Mono's WebAssembly loader treats that field as a recursive type and throws `TypeLoadException` on the first `ISender.Send`. CoreCLR accepts the same type. Replace the auto-property with an expression-bodied getter that has no backing field, and add a reflection test that fails if `Unit` again declares a static field of type `ValueTask<Unit>`.

The same change fills the gaps timewarp-state task 128 recorded against `documentation/migration.md`: the generated-namespace using, the internal `MediatorManifest` handoff (keep the type internal; the host passes the JSON string), the absent `profile` key, which package takes `PrivateAssets`, the `IActionHandler<>` replacement signature, `ValueTask<T>` conversion limits, `TimeWarpMediatorAssembly=false` in the checklist at the top, and a `using static TimeWarp.Mediator.Unit` example that compiles.

`<Version>` becomes `15.0.0-beta.2`, with a release-notes entry. Publishing the package is after merge (`tw-release`). This walk does not cut the release. This repo has no Blazor WebAssembly lane, so the load proof stays timewarp-state task 128 after it re-pins beta.2.

Review round 1 stays in that acceptance. The `IncrementHandler` sample returns `ValueTask.CompletedTask`, and a generators-test `ActionHandler<T>` subclass compiles that shape. `Handle` is a non-generic `ValueTask`, so `Unit.ValueTask` (`ValueTask<Unit>`) does not belong there. The Nuru section lists the JSON kind strings `"Query"`, `"IdempotentCommand"`, `"Command"`, `"Action"`, and `"Request"`. `documentation/migration.md` and `documentation/release-notes/15.0.0-beta.2.md` cite CS0433 when a library references two hosts. CS0436 is the conflict between a source type and an imported type.

## Description

`TimeWarp.Mediator` 15.0.0-beta.1 cannot run under Blazor WebAssembly. The first
`ISender.Send` throws:

```
System.TypeLoadException: Recursive type definition detected System.Threading.Tasks.ValueTask`1
```

Cause (`source/timewarp-mediator/unit.cs`):

```csharp
public static System.Threading.Tasks.ValueTask<Unit> ValueTask { get; } = new(Instance);
```

A static auto-property emits a static backing field of type `ValueTask<Unit>` on the
`Unit` struct, and `ValueTask<T>` holds a `T`. CoreCLR accepts the cycle; the Mono
WebAssembly type loader rejects it. That is why every CoreCLR Fixie suite and
Interactive Server pass while the Blazor WASM e2e lane fails.

Found while moving timewarp-state onto beta.1 (timewarp-state task 128, branch
`task/128-update-to-timewarpmediator-1500-beta1-one-package`). State is blocked on this
until a beta.2 ships.

Second part: the same migration recorded a list of gaps on `documentation/migration.md`
that cost the implementer real time. Fold them in while the page is open.

## Checklist

### Blocker

- [x] Replace the auto-property with a getter that has no backing field on `Unit`:
      `public static ValueTask<Unit> ValueTask => new(Instance);` (a completed
      `ValueTask<T>` is a struct wrapping the value, so allocation is unchanged).
      Alternative if a cached instance is wanted: keep the field on a separate
      `static class` and expose it from `Unit`.
- [x] Add a test that `typeof(Unit)` declares no static field whose type is
      `ValueTask<Unit>` (reflection, runs on CoreCLR, guards the regression without a
      WASM lane).
- [x] Verify under Mono WASM if a lane exists (package smoke or a Blazor WASM sample);
      otherwise note in `## Results` that timewarp-state task 128 e2e is the proof.

### Migration page (`documentation/migration.md`)

- [x] `AddGeneratedMediator()` lives in `TimeWarp.Mediator.Generated` (or
      `TimeWarpMediatorNamespace`). State it needs an explicit `using` or a global using;
      it is not an implicit using. Show the one-line snippet.
- [x] `MediatorManifest` is `internal` to the generating assembly. A library (State's
      `LogTimeWarpStateMiddleware`, `PipelineBehaviorRegistry`) cannot read it directly;
      the host must hand it over (State takes the JSON string). Document the handoff
      pattern or expose a public accessor. Decide which and record the decision.
- [x] The page says the JSON has no profile field; also state there is no `profile` key
      at all and what callers that switched on Aot/Link should do instead.
- [x] Analyzers do not flow transitively. `PrivateAssets="all"` on `TimeWarp.Mediator`
      (not just Generators) drops the runtime DLL from publish. A host that needs
      DevTools or any runtime type must reference `TimeWarp.Mediator` directly without
      `PrivateAssets`. Add a "which package, which PrivateAssets" table.
- [x] `IActionHandler<>` is gone (table row exists); add the replacement signature with
      `ValueTask` return so a reader does not have to guess.
- [x] `ValueTask<T>` ergonomics section: no implicit conversion from `Task<T>`, no
      `ContinueWith`, `Task.WhenAll` needs `.AsTask()`. One short example each.
- [x] `TimeWarpMediatorAssembly=false` is analyzer membership only and does not stop
      emission (page already says this under Generators); surface it in the quick
      checklist near the top, not only at line ~192.
- [x] Note the dev-cli / Nuru pattern: `using static TimeWarp.Mediator.Unit` plus Nuru
      works; `Unit.ValueTask` must be qualified in expressions (CS0229). Already partly
      covered; make sure the example compiles.

### Release

- [x] Bump `source/Directory.Build.props` `<Version>` to `15.0.0-beta.2`.
- [x] Release-notes entry under `documentation/release-notes/` for beta.2 (WASM fix,
      migration-page updates).
- [ ] After merge: `tw-release` cut beta.2, then timewarp-state task 128 re-pins and
      re-runs `ganda task work 128`.

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-28)
- Implementation: 01a18213-753b-7270-8ac6-6e294b102e35 (2026-10-28)
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session 01a18213-753b-7270-8ac6-6e294b102e35, max-turns 500, Done — 2026-10-28T10:44:06Z
- Review oracle round 1: general reviewer (claude-opus-5-5, headless `ganda task work`), effort 2 by diff. Fix pass requested for M1–M3 — 2026-10-28
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session 4a0e8efe-3d0b-4f4e-8bad-61b9702ab892, max-turns 120 — 2026-10-28T10:46:56Z
- Fix pass: implement-fix-2 closed M1, M2, and M3 (2026-10-28)
- Review oracle round 2: general reviewer (claude-opus-5-5, headless `ganda task work`), effort 2. M1–M3 fixed, no new findings. Disposition clean — 2026-10-28

## Results

`Unit.ValueTask` is an expression-bodied getter, `public static ValueTask<Unit> ValueTask => new(Instance)`. `typeof(Unit)` declares no static field of type `ValueTask<Unit>`. `UnitValueTaskTests` asserts that, compiles the three `ValueTask<T>` ergonomics snippets from the migration page, runs `NuruStyleUnitHandler`, and runs `IncrementHandler`. `IncrementHandler` matches the migration-page `ActionHandler<T>` sample and returns `ValueTask.CompletedTask`.

`MediatorManifest` stays `internal`. The migration page records that decision and the handoff: the generating assembly passes `MediatorManifest.Json` to the library. A public type would collide (CS0433) when a library references two hosts. CS0436 is the conflict between a source type and an imported type. `HostVisibilityTests` already requires `Accessibility.Internal` and rejects CS0433, CS0436, and CS0121.

This repo has no Blazor WebAssembly project and no browser package-smoke lane. The load proof is timewarp-state task 128 after it re-pins `15.0.0-beta.2` and re-runs its WebAssembly e2e lane.

`<Version>` is `15.0.0-beta.2`. Install lines in the root readme, the three package readmes, and `skills/tw-mediator` pin that version. `./bin/dev check-version` reports source is one prerelease ahead of NuGet `15.0.0-beta.1`. Cutting the package is `tw-release` after merge, then timewarp-state task 128 re-pins. That checklist item stays open.

`ganda repo audit` exits 0. Advisory: `kanban-landed` warns that task 021 has no usable worktree. That claim is not this id.

### How to validate

Smoke: `./bin/dev test` and `dotnet test tests/timewarp-mediator-generators-tests/timewarp-mediator-generators-tests.csproj --filter FullyQualifiedName~UnitValueTaskTests`

Expect: exit 0. generators-tests 61 passed, analyzers-tests 42 passed, and compilation-tests 69 passed, each on `net10.0` and `net11.0`. `Unit_DeclaresNoStaticFieldOfTypeValueTaskOfUnit` and `ActionHandler_ReturnsCompletedValueTask` pass on both. `./bin/dev check-version` prints `Version in source: 15.0.0-beta.2` and `safe to release`. WebAssembly proof is timewarp-state task 128 e2e after the re-pin.

### Review

Disposition: **clean**. Two rounds ran, each with one general reviewer (claude-opus-5-5) at effort 2. Round 1 raised one bug (M1) and two nits (M2, M3). Fix pass `cd587ef` closed all three. Round 2 re-verified them and found nothing new. Final counts: bug 1 fixed, nit 2 fixed, 0 open, 0 wontfix. `./bin/dev test --quiet` passes.

- M1: `IncrementHandler` in `documentation/migration.md` returns `ValueTask.CompletedTask`. `tests/timewarp-mediator-generators-tests/increment-handler.cs` compiles that shape.
- M2: the Nuru bullet lists the JSON kind strings `"Query"`, `"IdempotentCommand"`, `"Command"`, `"Action"`, and `"Request"`.
- M3: `documentation/migration.md` and `documentation/release-notes/15.0.0-beta.2.md` cite CS0433 for a library that references two hosts, and name CS0436 as the source-versus-imported case.

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Review

- Round 1: Blocked — open M1, M2, M3
- Fix pass: M1, M2, and M3 addressed.
- Round 2: Done
## Notes

- Blocker verified at tag `v15.0.0-beta.1` and on `origin/master` (same line).
- State-side evidence: `TypeLoadException` at
  `TimeWarp.Features.Routing.RouteState.PushRouteInfo` and `Counter.ButtonClick` in the
  Blazor WASM e2e lane; all CoreCLR suites green (DevTools 101, client-integration 64).
- Full feedback list lives in timewarp-state
  `kanban/in-progress/128-update-to-timewarpmediator-1500-beta1-one-package/task.md`
  under `## Results`.
