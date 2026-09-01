# TimeWarp repo conformance replace MediatR fork layout

## Description

This repo is still Jimmy Bogard’s MediatR tree with a rename. `ganda repo audit` today: **Passed 1 / Failed 18 / Skipped 7**. No `tools/dev-cli`, no `bin/dev`, `src/` + `test/` + `Documentation/` + PascalCase files, 9 `.ps1` scripts, no CPM, 112 kebab failures, ~188 `.cs` files in fork style.

**004** (source-gen) and **005** (14.0.0-beta) do not make it a TimeWarp repo. **005-003 must not publish** until this epic’s audit gate is green (or the operator explicitly waives).

NuGet **package ids** stay `TimeWarp.Mediator` (and siblings). File/folder names and repo layout change; public type names stay PascalCase.

## Children

- **006-001** Scaffold: audit `--fix` + TimeWarp layout dirs
- **006-002** Dev CLI; replace `.ps1`
- **006-003** Path kebab: `source/` `tests/` `documentation/` + folder/csproj/slnx names
- **006-004** C# kebab files + tw-csharp (file-scoped ns, global-usings, explicit types)
- **006-005** `ganda repo audit` error checks green

## Requirements

- `ganda repo audit` exits 0 for **error** severity (warnings like memsearch/vscode may remain until their children or `--fix`)
- `bin/dev` exists; `dev build` / `dev test` / `dev pack` (or this repo’s equivalent) replace `Build.ps1`
- Layout matches TimeWarp: `source/`, `tests/`, `documentation/`, `tools/dev-cli/`, `kanban/` only (drop `Kanban/` and `src/` / `test/`)
- Kebab-case paths (TW0001 / `kebab-path-names`)
- Dual `kanban/` + `Kanban/` resolved to lowercase `kanban/`

## Out of scope

- Deleting the reflection `AddMediator()` runtime (still Phase A)
- State 080 / Nuru 443 product switch
- Shipping 14.0.0 **stable**

## Notes

- Snapshot 2026-09-01 on `master`: no Directory.Packages.props, no slnx (has `.sln`), no tools/dev-cli, workflow.yml already exists.
- PS1: `Build.ps1`, `Add-AttributionComments.ps1`, `Create-FileMapping.ps1`, `Filter-Files.ps1`, `Tools/GitHooks/*.ps1`, `Tools/FileSync/SyncConfigurableFiles.ps1`, `.github/scripts/Push.ps1`, `SyncConfigurableFiles.ps1`.
- Old PascalCase `Kanban/ToDo` leftover (007, 009, …) is not this epic’s product work.

## Session

- Created: 162284 (2026-09-01)
