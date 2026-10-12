# Review framework — task 013-011

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-011-generator-make-the-pipeline-incremental-and-stop-double-reporting-diagnostics/
**Diff scope:** commit b9387f4 (`feat(generator): incremental equatable pipeline and single TWM001-003 reporter`) vs its parent. 23 files, 650 insertions, 131 deletions (781 lines changed). Not the older origin/home history.
**Plan / brief:** Make the generator pipeline incremental and stop double-reporting TWM001–TWM003. Parent 013 findings G-1, G-2, G-19. Requirements are on task.md.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle 01a12f98-c1ac-7f41-be3c-dfcbe39395a6 (2026-10-12)

## Budget (by-diff)

- Lines changed: 781
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
- Scope is this task's commit only. Do not re-litigate 013-001 through 013-010.

## Review focus

- Equatable pipeline: Options → MediatorModel (strings and value records, no Compilation or ISymbol) → Sources / Diagnostics, each step `WithTrackingName`
- Unrelated edit leaves Options Cached, Model Unchanged, and the AddSource output Cached
- A new handler marks Model and Sources Modified and the new handler appears in generated text
- Generator host reports TWM001–TWM003 once when the bundled analyzer also runs; libraries still get them from the analyzer
- TWM004 still reports in a generator host
- Analyzer reads build properties only from GlobalOptions, builds the graph lazily, and passes a cancellation token through membership and the type walk
- Diagnostic locations round-trip onto the compilation's syntax tree (path + span)
