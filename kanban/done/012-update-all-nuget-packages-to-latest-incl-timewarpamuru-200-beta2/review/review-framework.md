# Review framework

## Budget (by-diff)

- Lines changed: 268
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 012

**Date:** 2026-10-09
**Host task:** kanban/to-do/012-update-all-nuget-packages-to-latest-incl-timewarpamuru-200-beta2/
**Diff scope:** branch task/012-update-all-nuget-packages-to-latest-incl-timewarpa vs master (master...HEAD)
**Plan / brief:** NuGet pins to latest (Amuru/Amuru.Tools 2.0.0-beta.2, Nuru 3.0.0-beta.79, etc.); dev-cli adapted to Nuru beta.79; githooks refresh.
**Effort:** 2 (general axis only, per Budget.ByDiff)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, ganda task work); general reviewer subagent a694c81243998a97e

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
