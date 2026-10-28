# Review framework

## Budget (by-diff)

- Lines changed: 602
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-28
**Host task:** kanban/in-progress/023-build-on-net-11-rc3-and-cross-target-timewarpmediator-for-net100-and-net110/
**Diff scope:** branch `task/023-build-on-net-11-rc3-and-cross-target-timewarpmedia` vs `master`, merge base `a9761eea33971145e64ceda9093949e44e1c8c75`, head `e1cdd00` (commits f14d475, d84a854, 065a7a3 plus kanban moves)
**Plan / brief:** task.md `## Requirements` and `## Goal (implementer restatement)`: multi-target `TimeWarp.Mediator` and the hosts at net10.0;net11.0 on the 11 RC3 SDK, per-TFM DI pins, CI on both SDKs, two-lib-folder pack gate, separate net11 benchmark record
**Effort:** 2 (by-diff budget), roster general only
**Reviewer roster:** general
**Session IDs:** review oracle, claude-opus-5-5 (ganda task work, headless)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
