# Version pack 14.0.0-beta.1 four packages

## Description

Parent: **005**. Change `<Version>` from `13.0.0` to **`14.0.0-beta.1`**. Pack Contracts, Mediator, Analyzers, Generators. CI `build-and-publish` must produce those four nupkgs and must **not** emit 13.0.0 artifacts.

## Requirements

- `Directory.Build.props` (or equivalent) Version = `14.0.0-beta.1`
- Package ids unchanged: `TimeWarp.Mediator`, `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Analyzers`, `TimeWarp.Mediator.Generators`
- Analyzers/Generators: `IncludeSymbols=false` (keep 004-001 NU5017 fix)
- `Build.ps1` / CI pack assertions still require analyzer DLLs in nupkgs
- Do **not** `dev release` / NuGet push here — that is **005-003**

## Checklist

- [x] Version bump
- [x] Local `Build.ps1` (or equivalent) packs 14.0.0-beta.1 for all four
- [ ] CI green on the bump PR
- [x] No 13.0.0 nupkgs from this branch
- [x] Implementation review (effort 1, general) — disposition clean

## Out of scope

- NuGet publish (005-003)
- Docs (005-002)
- Deleting legacy `AddMediator()`

## Session

- Created: 150754 (2026-09-01)
- Implementer: grok session (2026-09-02)
- Review: grok, effort 1, roster general (2026-09-02)

## Results

Bumped unified `<Version>` from `13.0.0` to `14.0.0-beta.1` in both `Directory.Build.props` and `source/Directory.Build.props` so pack and release `AssertVersionSsot` stay aligned. Package ids are unchanged. Analyzers/Generators still set `IncludeSymbols=false` (004-001 NU5017). No `dev release` / NuGet push.

`dotnet run --file tools/dev-cli/dev.cs -- pack` wrote only this run's artifacts under `artifacts/packages`:

| File | Nuspec id | Version |
|------|-----------|---------|
| `TimeWarp.Mediator.14.0.0-beta.1.nupkg` (+ snupkg) | `TimeWarp.Mediator` | `14.0.0-beta.1` |
| `TimeWarp.Mediator.Contracts.14.0.0-beta.1.nupkg` (+ snupkg) | `TimeWarp.Mediator.Contracts` | `14.0.0-beta.1` |
| `TimeWarp.Mediator.Analyzers.14.0.0-beta.1.nupkg` (no snupkg) | `TimeWarp.Mediator.Analyzers` | `14.0.0-beta.1` |
| `TimeWarp.Mediator.Generators.14.0.0-beta.1.nupkg` (no snupkg) | `TimeWarp.Mediator.Generators` | `14.0.0-beta.1` |

Pack printed `Package layout verified` for Analyzers (`analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll`) and Generators (Generators.dll + Analyzers.dll). `find artifacts -name '*13.0.0*'` was empty.

Tests: analyzers 6 passed; generators 19 passed; mediator tests 163 passed / 2 skipped with `DOTNET_ROLL_FORWARD=Major` (this machine has no net8.0 runtime; CI installs `8.0.x`).

Moving this child to in-progress also moved parent **005** to in-progress (column rollup). CI on the bump PR is the host `open-pr` node — not opened from this implementer.

### How to validate

**Smoke**

```bash
dotnet run --file tools/dev-cli/dev.cs -- pack
ls artifacts/packages
find artifacts -name '*13.0.0*'
```

**Expect**

- Exit 0, `Pack completed successfully!`
- `Package layout verified: TimeWarp.Mediator.Analyzers.14.0.0-beta.1.nupkg`
- `Package layout verified: TimeWarp.Mediator.Generators.14.0.0-beta.1.nupkg`
- Four nupkgs named `TimeWarp.Mediator{,.Contracts,.Analyzers,.Generators}.14.0.0-beta.1.nupkg`
- Snupkgs only for Mediator and Contracts (not Analyzers/Generators)
- `find artifacts -name '*13.0.0*'` prints nothing

**Automated gate**

```bash
dotnet run --file tools/dev-cli/dev.cs -- pack
# CI also: dotnet run --file tools/dev-cli/dev.cs -- test
# (requires net8.0 runtime, or DOTNET_ROLL_FORWARD=Major on a later runtime)
```

**Not in scope:** `dev release` / NuGet push (005-003); consumer docs (005-002).

### Review disposition

- **Outcome:** clean
- **Rounds:** 1
- **Effort / roster:** 1, general
- **Counts (final, round 1):** bug 0; suggestion 0; nit 0 — final open count 0
- **Wontfix / escalations:** none
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
