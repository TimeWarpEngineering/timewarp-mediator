# Review framework

## Budget (by-diff)

- Lines changed: 994
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope (review oracle, 2026-10-24)

- **Host task:** kanban/to-do/021-001-benchmarks-first-interface-path-and-pipeline-baselines-with-a-regression-gate/
- **Diff scope:** branch `task/021-001-benchmarks-first-interface-path-and-pipeline-basel` vs `master` (175104e, 280a842, 513b137)
- **Plan / brief:** `## Goal (implementer restatement)` and `## Requirements` in task.md
- **Effort:** 3 (from by-diff budget), roster `general`
- **Session IDs:** review oracle (claude, claude-opus-5-5), session not reported

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
