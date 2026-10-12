# Review framework — task 013-009

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-009-generator-aot-profile-behaviors-silently-dropped-while-the-manifest-lists-them/
**Diff scope:** commit `d86e920` vs its parent (`7fb3101`) — the 013-009 implement commit (537 lines). Not the whole history vs an older `origin/master`.
**Plan / brief:** G-7 / G-8. Aot/Link skips closed behaviors while the manifest listed them, and ServiceGen emitted `new Handler()` for handlers with constructor parameters. Decision recorded on the task: diagnose, do not weave. TWM005 (warning) per dropped behavior, empty `behaviors` in the Aot/Link manifest, TWM006 (error) for handlers with no accessible parameterless constructor, `default!` so TWM006 is the only generated-code error, singleton lifetime documented.
**Effort:** 2 (by-diff, 537 lines; roster axes: general only)
**Reviewer roster:** general
**Session IDs:** Grok session `01a12f38-b61a-7b50-9b71-466403888c2c` (2026-10-12)

## Budget (by-diff)

- Lines changed: 537
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
