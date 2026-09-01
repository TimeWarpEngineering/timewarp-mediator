# Scaffold audit-fix layout dirs and CPM

## Description

Parent: **006**. Mechanical TimeWarp scaffold. Prefer `ganda repo audit --fix` where Fixable=YES. Do **not** kebab-rename the tree here (**006-003**).

Today’s errors this slice should kill or shrink: envrc, routine-journals-gitignore, Directory.Packages.props, banned-symbols, banned-api-analyzers, assembly-metadata (Build.Tasks), msbuild/repository.props, source/Directory.Build.props, slnx, editorconfig sentinels (editorconfig is **not** auto-overwritten — add sentinels by hand), kanban/backlog|in-progress|archived placeholders.

## Requirements

- Central Package Management (`Directory.Packages.props`)
- `msbuild/repository.props` imported from root Directory.Build.props
- `source/Directory.Build.props` exists (may still point at `src/` until 006-003)
- Root `.slnx` (can wrap existing `.sln` projects)
- `.envrc` PATH_add bin; gitignore routine journals
- TimeWarp `.editorconfig` sentinels
- Empty `kanban/{backlog,in-progress,archived}/` as required by directory-structure

## Out of scope

- `tools/dev-cli` body (**006-002**)
- Renaming `src/` → `source/` contents (**006-003**)
- Rewriting 188 `.cs` files (**006-004**)

## Session

- Created: 162284 (2026-09-01)
