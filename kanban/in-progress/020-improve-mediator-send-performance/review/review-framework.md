# Review framework

## Budget (by-diff)

- Lines changed: 589
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope (task 020)

**Date:** 2026-10-28
**Host task:** kanban/in-progress/020-improve-mediator-send-performance/
**Diff scope:** branch `task/020-improve-mediator-send-performance` vs `master` (merge-base `abe670e`), HEAD `6362884`, plus the uncommitted working-tree edits to `documentation/benchmarks/comparison-baseline.md` and `tests/timewarp-mediator-benchmarks-comparison/dispatch-benchmarks.cs`
**Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`
**Effort:** 2 (by-diff). Roster: general
**Reviewer:** reviewer-claude (claude-opus-5-5), headless review oracle

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space. Zero issues is a valid outcome
- Address the diff and surrounding call sites. Re-verify falsifiable claims against the repo
- Prior rounds are immutable. New work goes in `round-(N+1)/`
