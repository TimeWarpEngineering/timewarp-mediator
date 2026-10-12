# Review framework — task 013-003

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-003-runtime-correctness-sweep-mediator-wrappers-pipeline-behaviors-publishers/
**Diff scope:** `git diff 0e1e5f8...HEAD -- source tests` on `task/013-003-runtime-correctness-sweep-mediator-wrappers-pipeli` (523 insertions, 137 deletions, 660 lines). Ancestor merges 013-001 and 013-002 are out of scope. `origin/master` is still at `83f5c0a` (publish kanban 013), so a three-dot diff against `origin/master` also contains those already-reviewed tasks.
**Plan / brief:** Runtime correctness sweep for reflection-mediator findings C-8, C-9, C-10, C-11, C-12, C-13, C-15, C-16, C-17, C-22, C-26, C-27, and C-30. No public API additions. One commit per finding group.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle `01a12ebb-79f5-7bf3-aaaf-b42ea9f6cc0b` (2026-10-12)

## Budget (by-diff)

- Lines changed: 660
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Out of scope: C-25 style (indent, underscore fields), C-14 AOT annotations, C-18/C-19 XML cleanup, C-20 public-surface shrink — those are other 013 children
