# Review framework

## Budget (by-diff)

- Lines changed: 444
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope (task 034)

**Date:** 2026-11-11
**Host task:** kanban/in-progress/034-clear-ganda-audit-failures-suppression-baseline-pwsh-workflow-githooks-coverage/
**Diff scope:** branch `task/034-clear-ganda-audit-failures-suppression-baseline-pw` vs `master` (`git diff master...HEAD`, product commit `2294821`)
**Plan / brief:** `task.md` Requirements and `## Goal (implementer restatement)`. The task's `## Review` section asks for a line-by-line bash vs pwsh comparison of the release guards. A guard that fails open blocks the PR.
**Effort:** 2 (by-diff budget above)
**Reviewer roster:** general
**Session IDs:** review oracle (Claude, claude-opus-5-5), headless `ganda task work`, 2026-11-11

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity: bug | suggestion | nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Re-check falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
