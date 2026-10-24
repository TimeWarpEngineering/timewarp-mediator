# Review framework

## Budget (by-diff)

- Lines changed: 1172
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope — task 021-006

**Date:** 2026-10-24
**Host task:** kanban/in-progress/021-006-generator-incrementality-syntax-provider-candidates-and-a-cached-assertion/
**Diff scope:** branch `task/021-006-generator-incrementality-syntax-provider-candidate` vs `master` (commit 7110b45 `perf: cache the mediator generator model on unrelated edits`)
**Plan / brief:** task.md `## Goal (implementer restatement)` + `## Requirements`: the model step is a pure function of equatable syntax-provider and metadata-reference facts; the step is `Cached` on an unrelated edit; emitted output and diagnostics are unchanged.
**Effort:** 3
**Reviewer roster:** general. Two independent general passes ran in parallel, split by file: (a) extraction (`candidate-extractor.cs`, `type-model.cs`, `candidate-facts.cs`, `referenced-assembly-candidates.cs`), (b) graph and emitters (`candidate-graph.cs`, `emission-graph.cs`, emitter diffs). The orchestrator owned the pipeline, props, tests, and verification.
**Session IDs:** review oracle (claude-opus-5-5, Claude Code headless); subagents aee4966440ea66a39 (general-a), a0e0c0b5a6062138b (general-b)

## Scope — round 2

**Date:** 2026-10-24
**Diff scope:** fix delta `be72432..a1979f8` (`fix: match generator facts to the symbol walk`), re-verifying M1–M12 and scanning the delta for new defects.
**Effort:** 3 (budget by diff: 1172 lines). **Roster:** general, as a single pass by the review oracle. The comparison reference is `source/timewarp-mediator-analyzers/message-graph-builder.cs`.
**Verification run:** `./bin/dev build`, `./bin/dev test` (57 + 42 + 69 passed), and `./bin/dev pack` (package smoke exited 0).
**Session IDs:** review-2 oracle (claude-opus-5-5, Claude Code headless; session id not reported).

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
