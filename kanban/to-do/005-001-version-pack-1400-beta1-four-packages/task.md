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

- [ ] Version bump
- [ ] Local `Build.ps1` (or equivalent) packs 14.0.0-beta.1 for all four
- [ ] CI green on the bump PR
- [ ] No 13.0.0 nupkgs from this branch

## Out of scope

- NuGet publish (005-003)
- Docs (005-002)
- Deleting legacy `AddMediator()`

## Session

- Created: 150754 (2026-09-01)
