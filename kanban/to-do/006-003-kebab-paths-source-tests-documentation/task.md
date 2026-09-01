# Kebab paths source tests documentation

## Description

Parent: **006**. Move the tree to TimeWarp layout and kebab-case **folders / csproj / slnx / non-cs files**. Leave `.cs` file kebab + body style to **006-004** if that keeps the diff reviewable; or do `.cs` renames here and style in 006-004 — pick one and don’t split a file’s rename across both.

Audit today: **112** non-kebab path names. `src/` → `source/`, `test/` → `tests/`, `Documentation/` → `documentation/`, `Assets/` → `assets/`. `TimeWarp.Mediator/` folder → `timewarp-mediator/`. Dual `Kanban/` + `kanban/` → `kanban/` only.

## Depends on

- 006-001

## Requirements

- Package **ids** remain `TimeWarp.Mediator` etc.
- Solution and CI paths updated
- Samples under `samples/` kebab
- `kebab-path-names` no longer fails on folders/csproj (`.cs` only if included in this slice)
- Strong-name `.snk` stay put; don’t bikeshed crypto in this task

## Out of scope

- C# language style (**006-004**)
- Dev CLI (**006-002**) — rebase if both land

## Session

- Created: 162284 (2026-09-01)
