# Review framework

## Budget (by-diff)

- Lines changed: 209
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 025

**Date:** 2026-10-31
**Host task:** kanban/in-progress/025-update-timewarpnuru-pins-to-400-beta10-keep-the-roslyn-500-floor/
**Diff scope:** branch `task/025-update-timewarpnuru-pins-to-400-beta10-keep-the-ro` vs merge-base with `master` (commits 0b13f31, 402d9a7, 7a7986c). Product paths: `Directory.Packages.props`, `agent.md`, `tools/dev-cli/endpoints/workflow-command.cs`. The remaining lines are the kanban task move.
**Plan / brief:** task.md Requirements and `## Goal (implementer restatement)`.
**Effort:** 2 (by-diff budget), roster axis general only.
**Reviewer roster:** general (the review oracle, inline)
**Session IDs:** review oracle, Claude Opus 5.5, ganda task work headless print mode

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
