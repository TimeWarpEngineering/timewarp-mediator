# Review framework — task 013-010

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-010-generator-and-analyzer-replace-generated-code-compile-errors-with-twm-diagnostics/
**Diff scope:** commit 0bc5cac (`feat(generator): replace generated-code compile errors with TWM007-TWM011`) vs parent dad2483. 17 files, 1375 insertions, 103 deletions (1478 lines changed). Not the older origin/home history.
**Plan / brief:** Replace generated-code compile errors and silent drops with TWM diagnostics. Parent 013 findings G-9, G-11, G-13, G-14, G-17, G-18, G-21. Requirements are on task.md.
**Effort:** 3 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle 01a12f68-d196-7263-b637-322de95fe26c (2026-10-12)

## Budget (by-diff)

- Lines changed: 1478
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Scope is this task's commit only. Do not re-litigate 013-001 through 013-009.

## Review focus

- TWM007 accessibility: `IsSymbolAccessibleWithin`, one diagnostic per type, no bind, no extra TWM001
- TWM008 missing runtime: one diagnostic, emit nothing; Host vs Aot reference set
- TWM009 unsupported handler kinds; CreateStream message has no internal task ids
- TWM010 / TWM011 behavior registration; location at the attribute when present
- TWM003 description vs behavior; notifications; help links; CompilationEnd on TWM001–TWM003
- TWM004: conditional access, `ISender.Send` only, `Send(object)` ignored, host-only whole-program checks
- Diagnostics stay inside this compilation's syntax trees
- New ids in `AnalyzerReleases.Unshipped.md`; docs table and `documentation/diagnostics.md`
- Tests cover the requirements; no false greens
