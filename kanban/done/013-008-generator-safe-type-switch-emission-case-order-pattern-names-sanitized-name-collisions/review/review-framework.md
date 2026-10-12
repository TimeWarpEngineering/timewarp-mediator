# Review framework — task 013-008

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-008-generator-safe-type-switch-emission-case-order-pattern-names-sanitized-name-collisions/
**Diff scope:** commit `3d1d2f5` vs its parent (`git show 3d1d2f5`). 640 insertions, 84 deletions, 724 lines, 7 files. Ancestor merges of 013-001 through 013-007 are out of scope. `origin/master` is still behind those already-reviewed tasks.
**Plan / brief:** Most-derived `Send(object)` case order so a base case cannot subsume a derived one (G-5), contravariant `Publish` so a notification reaches every bound base and interface (G-5), fixed pattern locals `r0`/`n0` (G-6), and hash-suffixed sanitized member names when `A.B_C` and `A_B.C` collide (G-15). Document the variance decision. Tests cover base-before-derived, interface notifications, keyword names, and colliding sanitized names.
**Effort:** 2 (general only; by-diff budget, no specialist axes)
**Reviewer roster:** general
**Session IDs:** Grok review oracle `01a12f23-5fd4-73c0-899b-f5666f67b7c1` (2026-10-12)

## Budget (by-diff)

- Lines changed: 724
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
- Out of scope: incremental generation (G-1), duplicate analyzer reporting (G-2), AOT behavior omission (G-7), and the other 013 children
