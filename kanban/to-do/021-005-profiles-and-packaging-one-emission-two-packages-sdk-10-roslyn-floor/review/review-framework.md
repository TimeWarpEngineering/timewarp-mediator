# Review framework

## Budget (by-diff)

- Lines changed: 1746
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Task 021-005

**Date:** 2026-10-24
**Host task:** kanban/to-do/021-005-profiles-and-packaging-one-emission-two-packages-sdk-10-roslyn-floor/
**Diff scope:** branch `task/021-005-profiles-and-packaging-one-emission-two-packages-s` vs `master`
(commit 2f8b932, `refactor!: ship one mediator emission and two packages`), kanban paths excluded.
**Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`: one
IServiceProvider emission (Aot/Link, ServiceGen, TWM005/006/009 removed), contracts renamed to the
`TimeWarp.Mediator` package carrying the TWM001–TWM004 analyzer, Generators as the second nupkg,
Roslyn floor at Microsoft.CodeAnalysis.CSharp 5.0.0, floor test project deleted, `dev pack`
asserts exactly two nupkgs, AOT sample publishes with a woven behavior.
**Effort:** 3
**Reviewer roster:** general (two independent general passes: the review oracle, and one
read-only general-purpose sub-agent briefed with the task requirements but not with the oracle's
findings). Both passes write into `round-N/general.md`.
**Session IDs:** reviewer-claude review oracle (headless, claude-opus-5-5), session id not
exposed in print mode; sub-agent a489660e36348b5b0.

### Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
