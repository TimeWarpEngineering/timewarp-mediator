# Review framework — task 013-015

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-015-decide-the-target-framework-floor-and-test-the-netstandard20-asset/
**Diff scope:** product commit `339e65a` vs parent `0631929` on `task/013-015-decide-the-target-framework-floor-and-test-the-net` (179 lines). Kitchen results commit `f2ae79f` is out of product scope.
**Plan / brief:** net10.0 floor for runtime packages, tests, and samples. Generators and Analyzers stay netstandard2.0. Drop Bcl.AsyncInterfaces, the net6.0/net8.0/netstandard assets, and the 8.0.x CI SDK. LangVersion pin lifted. Parent findings B-12, C-24, part of D-11.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Grok `01a12ff2-bfdd-7472-83f9-f02170419d30` (2026-10-12)

## Budget (by-diff)

- Lines changed: 179
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
