# ganda repo audit error checks green

## Description

Parent: **006**. Gate: `ganda repo audit` exits **0** for error-severity checks. This is what unblocks **005-003** (NuGet 14.0.0-beta.1).

## Depends on

- 006-002
- 006-004

## Requirements

- `ganda repo audit` — no **Error** FAILs
- Warning-only (memsearch, vscode-window-icon) may remain unless cheap `--fix`
- `dev test` (or equivalent) green
- Record remaining warning exceptions in `.editorconfig` `[ganda.audit]` with a reason if we keep them

## Checklist

- [ ] Audit table: Error FAILs = 0
- [ ] Tests green
- [ ] Note on **005-003**: unblocked

## Out of scope

- Publishing NuGet (005-003)
- State/Nuru consume

## Session

- Created: 162284 (2026-09-01)
