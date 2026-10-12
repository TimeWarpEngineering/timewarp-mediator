# Review framework — task 013-004

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-004-decide-and-reduce-the-public-api-surface-before-1400-stable/
**Diff scope:** `git diff 01e74ea...HEAD` (commit `3504496` vs its parent). 117 insertions, 26 deletions, 143 lines. Ancestor merges 013-001 through 013-003 are out of scope. `origin/master` is still at `83f5c0a`, so a three-dot diff against `origin/master` also contains those already-reviewed tasks.
**Plan / brief:** Reduce the `TimeWarp.Mediator` runtime public API before 14.0.0 stable (parent findings C-20, C-21, and the internal half of C-4). Internalize request and notification wrappers and `ServiceRegistrar`, remove `ServiceFactory`, rename the `serviceFactory` parameter, and record the breaking changes in the 14.0.0 release notes.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle `01a12ed7-1122-7ca0-8548-9c99ad6445b3` (2026-10-12)

## Budget (by-diff)

- Lines changed: 143
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
- Out of scope: generator diagnostics, AOT annotations, XML-doc cleanup, and the 013-017 release-process rewrite. Those are other 013 children
