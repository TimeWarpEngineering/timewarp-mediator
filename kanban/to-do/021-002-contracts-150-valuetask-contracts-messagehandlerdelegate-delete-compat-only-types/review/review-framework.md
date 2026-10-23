# Review framework

## Budget (by-diff)

- Lines changed: 3152
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-23
**Host task:** kanban/to-do/021-002-contracts-150-valuetask-contracts-messagehandlerdelegate-delete-compat-only-types/
**Diff scope:** branch `task/021-002-contracts-150-valuetask-contracts-messagehandlerde` vs `master` (commit 6d7892e)
**Plan / brief:** task.md Requirements and `## Goal (implementer restatement)`; parent 021 findings F3-F6, F8, F10, F18; decisions D2, D3, D6
**Effort:** 3 (by-diff budget); roster axes: general
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle under `ganda task work` (session id not exposed to the oracle)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
