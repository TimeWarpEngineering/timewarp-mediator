# Review framework

## Budget (by-diff)

- Lines changed: 139
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope (task 023-001)

**Date:** 2026-11-11
**Host task:** kanban/in-progress/023-001-swap-net-11-rc3-pins-for-the-ga-sdk-and-packages/
**Diff scope:** branch `task/023-001-swap-net-11-rc3-pins-for-the-ga-sdk-and-packages` vs `master` (`git diff master...HEAD`, product commit `b8432b5` in round 1; round 2 delta `9d5ada5..HEAD`, fix commit `6598517`)
**Plan / brief:** `task.md` Requirements and `## Goal (implementer restatement)` — swap .NET 11 RC3 SDK, package, and CI preview pins for .NET 11 GA (`11.0.100` / `11.0.0`), keep BenchmarkDotNet split (`0.15.8` on `net10.0`, `0.16.0-preview.2` on `net11.0` while `0.16.0` stable is unpublished), keep `comparison-baseline.md` unchanged and green, and bump `<Version>` to `15.0.0-beta.7` with release notes.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle round 1 (Cursor, gemini-4-carbon-high), review oracle round 2 (Cursor, gemini-4-carbon-high), headless `ganda task work`, 2026-11-11

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
