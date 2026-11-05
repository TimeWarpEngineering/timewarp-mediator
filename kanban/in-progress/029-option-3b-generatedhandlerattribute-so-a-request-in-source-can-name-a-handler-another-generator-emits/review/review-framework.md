# Review framework

## Budget (by-diff)

- Lines changed: 5840
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope (task 029)

**Date:** 2026-11-06
**Host task:** kanban/in-progress/029-option-3b-generatedhandlerattribute-so-a-request-in-source-can-name-a-handler-another-generator-emits/
**Diff scope:** branch `task/029-option-3b-generatedhandlerattribute-so-a-request-i` (HEAD `b0c49a7`) vs merge-base `1931c40` on `master`. Master is 2 commits ahead, and both are kanban landings only.
**Plan / brief:** `task.md` `## Goal (implementer restatement)` and `## Requirements`. Add a public abstract `GeneratedHandlerAttribute`, a declared-handler fact in the extractor and graph, dispatch to the declared name, TWM012, an incrementality test, and the bump to 15.0.0-beta.5.
**Effort:** 3, general axis only (the budget line above)
**Reviewer roster:** general (the review oracle, Claude Opus 5.5)
**Session IDs:** review oracle, ganda task work `review` node, 2026-11-06

About 2,000 of the 5,840 changed lines are partial-class splits. Before reading the new logic, the
review compared the old `message-graph-builder.cs`, `candidate-graph.cs`, and `mediator-emitter.cs`
line by line (sorted) with their new partials. `mediator-emitter.*` is a pure move. The other two
differ only by the declared-handler additions and the `AddHandler` dedupe.

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Re-check falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
