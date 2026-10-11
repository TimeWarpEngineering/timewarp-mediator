# Review framework — task 013-001

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-001-fix-legacy-registrar-generic-registration-limits-and-timeout/
**Diff scope:** branch `task/013-001-fix-legacy-registrar-generic-registration-limits-a` vs `origin/master` (commit `3592c60`)
**Plan / brief:** Honor documented legacy-registrar limits. C-1: `RegistrationTimeout <= 0` disables the timeout and a real timeout wraps the inner `OperationCanceledException`. C-2: thread the cancellation token through every `ConnectImplementationsToTypesClosing` / `AddAllConcretionsThatClose` pass. C-3: guard the combination cap on `MaxGenericTypeRegistrations > 0` with a saturating multiply. Required tests: timeout 0 does not throw; cap 0 disables; a cap of N throws past N.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** general reviewer is this review-oracle session `ses_ed3da47abffeSdWFr7Ixl4aJro` (2026-10-12). A spawned general subagent (`ses_ed3d6881bffeA7WsE6WutUH4Pd`) failed before writing findings (`invalid_grant`); the oracle completed the general review in-session rather than blocking the walk.

## Budget (by-diff)

- Lines changed: 194
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
