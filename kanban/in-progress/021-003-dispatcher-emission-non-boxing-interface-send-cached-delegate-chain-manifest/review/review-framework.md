# Review framework

## Budget (by-diff)

- Lines changed: 988
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope

**Date:** 2026-10-24
**Host task:** kanban/in-progress/021-003-dispatcher-emission-non-boxing-interface-send-cached-delegate-chain-manifest/
**Diff scope:** branch `task/021-003-dispatcher-emission-non-boxing-interface-send-cach` vs `master` (commit 902e5d5 over base 1c7f2b0)
**Round 2 scope:** the fix-pass delta `231ed4e..91058b9` (product commit 2a2a10d, baseline commit d768a68), effort 3, roster general
**Plan / brief:** task.md Requirements + `## Goal (implementer restatement)`; parent 021 findings F2, F4, F7, F12, F13 (`documentation/design/2026-10-13-15.0-breaking-redesign-review.md`)
**Effort:** 3 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5) under ganda task work; session id not reported by host

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
