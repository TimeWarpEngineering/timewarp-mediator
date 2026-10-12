# Review framework — task 013-007

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-007-generator-bind-every-handler-interface-per-class-skip-inherited-bindings-order-handlers-deterministically/
**Diff scope:** commit `9bc7518` vs its parent (`git show 9bc7518`). 475 insertions, 49 deletions, 524 lines, 8 files. Ancestor merges of 013-001 through 013-006 are out of scope. `origin/master` is still at `83f5c0a`, so a three-dot diff against `origin/master` also contains those already-reviewed tasks.
**Plan / brief:** Bind every handler interface on a class (G-3), skip interfaces a concrete base handler in a member assembly already implements (G-4), and order member assemblies and handler lists so notification order and TWM002 are stable (G-10). Document the rules. Tests cover multi-interface handlers, derived handlers, and two member assemblies referenced in either order.
**Effort:** 2 (general only; by-diff budget, no specialist axes)
**Reviewer roster:** general
**Session IDs:** Grok review oracle `01a12f11-0cf1-7942-ae9e-83adf1bc56cb` (2026-10-12)

## Budget (by-diff)

- Lines changed: 524
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
- Out of scope: incremental generation (G-1), duplicate analyzer reporting (G-2), type-switch order (G-5), AOT behavior omission (G-7), and the other 013 children
