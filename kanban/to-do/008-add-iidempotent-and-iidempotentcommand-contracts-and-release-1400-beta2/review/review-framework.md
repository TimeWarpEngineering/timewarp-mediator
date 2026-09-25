# Review framework — task 008

**Date:** 2026-09-25
**Host task:** kanban/to-do/008-add-iidempotent-and-iidempotentcommand-contracts-and-release-1400-beta2/
**Diff scope:** branch `task/008-add-iidempotent-and-iidempotentcommand-contracts-a` vs `7f18058` (master merge base), commit `0d4cc71`
**Plan / brief:** Add `IIdempotent`, `IIdempotentCommand` / `IIdempotentCommand<T>`, and their handlers to
TimeWarp.Mediator.Contracts; make `IQuery<T>` implement `IIdempotent`; add generator and reflection dispatch
tests; bump to 14.0.0-beta.2 and update docs. Also includes pre-existing gate fixes (SourceLink bump,
`RollForward`, `ganda repo audit --fix` output).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** cursor implementer-cursor review oracle (ganda task work, headless; no vendor session id exposed)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
