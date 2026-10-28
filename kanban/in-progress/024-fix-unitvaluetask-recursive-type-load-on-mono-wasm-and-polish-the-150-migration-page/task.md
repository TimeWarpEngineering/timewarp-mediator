# Fix Unit.ValueTask recursive type load on Mono WASM and polish the 15.0 migration page

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

- [ ] Replace the auto-property with a getter that has no backing field on `Unit`:
      `public static ValueTask<Unit> ValueTask => new(Instance);` (a completed
      `ValueTask<T>` is a struct wrapping the value, so allocation is unchanged).
      Alternative if a cached instance is wanted: keep the field on a separate
      `static class` and expose it from `Unit`.
- [ ] Add a test that `typeof(Unit)` declares no static field whose type is
      `ValueTask<Unit>` (reflection, runs on CoreCLR, guards the regression without a
      WASM lane).
- [ ] Verify under Mono WASM if a lane exists (package smoke or a Blazor WASM sample);
      otherwise note in `## Results` that timewarp-state task 128 e2e is the proof.

### Migration page (`documentation/migration.md`)

- [ ] `AddGeneratedMediator()` lives in `TimeWarp.Mediator.Generated` (or
      `TimeWarpMediatorNamespace`). State it needs an explicit `using` or a global using;
      it is not an implicit using. Show the one-line snippet.
- [ ] `MediatorManifest` is `internal` to the generating assembly. A library (State's
      `LogTimeWarpStateMiddleware`, `PipelineBehaviorRegistry`) cannot read it directly;
      the host must hand it over (State takes the JSON string). Document the handoff
      pattern or expose a public accessor. Decide which and record the decision.
- [ ] The page says the JSON has no profile field; also state there is no `profile` key
      at all and what callers that switched on Aot/Link should do instead.
- [ ] Analyzers do not flow transitively. `PrivateAssets="all"` on `TimeWarp.Mediator`
      (not just Generators) drops the runtime DLL from publish. A host that needs
      DevTools or any runtime type must reference `TimeWarp.Mediator` directly without
      `PrivateAssets`. Add a "which package, which PrivateAssets" table.
- [ ] `IActionHandler<>` is gone (table row exists); add the replacement signature with
      `ValueTask` return so a reader does not have to guess.
- [ ] `ValueTask<T>` ergonomics section: no implicit conversion from `Task<T>`, no
      `ContinueWith`, `Task.WhenAll` needs `.AsTask()`. One short example each.
- [ ] `TimeWarpMediatorAssembly=false` is analyzer membership only and does not stop
      emission (page already says this under Generators); surface it in the quick
      checklist near the top, not only at line ~192.
- [ ] Note the dev-cli / Nuru pattern: `using static TimeWarp.Mediator.Unit` plus Nuru
      works; `Unit.ValueTask` must be qualified in expressions (CS0229). Already partly
      covered; make sure the example compiles.

### Release

- [ ] Bump `source/Directory.Build.props` `<Version>` to `15.0.0-beta.2`.
- [ ] Release-notes entry under `documentation/release-notes/` for beta.2 (WASM fix,
      migration-page updates).
- [ ] After merge: `tw-release` cut beta.2, then timewarp-state task 128 re-pins and
      re-runs `ganda task work 128`.

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-28)

## Notes

- Blocker verified at tag `v15.0.0-beta.1` and on `origin/master` (same line).
- State-side evidence: `TypeLoadException` at
  `TimeWarp.Features.Routing.RouteState.PushRouteInfo` and `Counter.ButtonClick` in the
  Blazor WASM e2e lane; all CoreCLR suites green (DevTools 101, client-integration 64).
- Full feedback list lives in timewarp-state
  `kanban/in-progress/128-update-to-timewarpmediator-1500-beta1-one-package/task.md`
  under `## Results`.
