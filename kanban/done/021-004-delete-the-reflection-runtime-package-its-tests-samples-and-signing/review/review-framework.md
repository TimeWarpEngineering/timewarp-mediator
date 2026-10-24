# Review framework

## Budget (by-diff)

- Lines changed: 35
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-24
**Host task:** kanban/in-progress/021-004-delete-the-reflection-runtime-package-its-tests-samples-and-signing/
**Diff scope:** branch `task/021-004-delete-the-reflection-runtime-package-its-tests-sa` vs `master` (d71f299, 6e35f96). 173 files, +247 / −10550. Most of the diff deletes files. The review read the 30 modified or renamed files line by line and grepped the tree for anything that still references a deleted artifact.
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Requirements` (D1, no shim; parent 021 findings F1, F16).
**Effort:** 3 (by-diff budget); roster axes: general only
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle session not reported by host

**Round 2 scope:** fix-pass delta `8fe815f..e1d0eb4` (04bdae6 product prose plus one MSBuild line, e1d0eb4 kanban). The Budget above is for this delta: 35 lines, effort 1, general. The round-1 Effort 3 line above records round 1.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Sibling ownership: 021-005 deletes TWM009; 021-007 owns agent.md, the skill, the package readmes, migration, and the benchmarks page. A finding those tasks will rewrite away may be deferred as `wontfix`, which matches the 021-002 M3 disposition.
