# Review framework

## Budget (by-diff)

- Lines changed: 407
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope — task 026

**Date:** 2026-10-31
**Host task:** kanban/in-progress/026-drop-the-roslyn-500-floor-guard-that-the-referenced-compiler-is-not-newer-than-the-sdks/
**Diff scope:** branch `task/026-drop-the-roslyn-500-floor-guard-that-the-reference` vs `master`. The merge base is the parent of `e48b6c0`, and HEAD is `1bc8de7`. The product commit is `30b91b7`. The 2 master-only commits touch only this task's kanban files.
**Plan / brief:** task.md Requirements and `## Goal (implementer restatement)`: drop the Roslyn 5.0.0 floor, move the pins to the newest stable, and guard that the referenced compiler is not newer than the SDK's.
**Effort:** 2 (by-diff budget). The roster axes are general only.
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle under ganda task work, 2026-10-31

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Address the diff and the call sites around it. Re-check any claim that can be falsified against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
