# Review framework

## Budget (by-diff)

- Lines changed: 306
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope (task 036)

**Date:** 2026-11-12
**Host task:** kanban/in-progress/036-update-outdated-nuget-pins-before-the-beta7-release/
**Diff scope:** branch `task/036-update-outdated-nuget-pins-before-the-beta7-releas` vs `master` (`git diff master...HEAD`, product commits `24d7d15` and `a858e34`)
**Plan / brief:** `task.md` Description, Requirements, `### Resolution (Steven, 2026-11-12)`, and `## Goal (implementer restatement)`
**Effort:** 2 (by-diff budget above)
**Reviewer roster:** general
**Session IDs:** review oracle round 1 (Cursor, vendor google, model gemini-4-carbon-high, session not reported), max-turns uncapped (no --max-turns flag; bound 120), headless `ganda task work`, 2026-11-12

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
