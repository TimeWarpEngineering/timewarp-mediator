# Review framework — task 043

**Date:** 2026-11-17
**Host task:** kanban/in-progress/043-declare-generator-emitted-requests-a-pattern-attribute-so-send-dispatches-request-types-another-source-generator-emits/
**Diff scope:** branch `task/043-declare-generator-emitted-requests-a-pattern` vs `master` (merge-base, product commit `2db66c6` plus kanban lands). 41 files, about 2594 insertions / 110 deletions.
**Plan / brief:** `GeneratedRequestPatternAttribute` so `Send` dispatches request types another source generator emits. Task description plus `## Goal (implementer restatement)` in `task.md`.
**Effort:** 3 (general only; by-diff budget, 2704 lines)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a1e82b-5468-71b2-a868-e298da18e9f3` (2026-11-17)

## Budget (by-diff)

- Lines changed: 2704
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
