# Review framework

## Budget (by-diff)

- Lines changed: 486
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Framework — task 024

**Date:** 2026-10-28
**Host task:** kanban/in-progress/024-fix-unitvaluetask-recursive-type-load-on-mono-wasm-and-polish-the-150-migration-page/
**Diff scope:** branch `task/024-fix-unitvaluetask-recursive-type-load-on-mono-wasm` vs merge-base with `master` (commits `be80f71`, `847f5c5`, `ea87b18`)
**Plan / brief:** task.md `## Goal (implementer restatement)` + `## Checklist` — drop the `ValueTask<Unit>` backing field on `Unit`, add a reflection guard test, fill the migration.md gaps from timewarp-state task 128, bump to `15.0.0-beta.2` with release notes
**Effort:** 2 (by-diff budget), roster axis general
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle under `ganda task work` (headless print mode)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
