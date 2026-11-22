# Review framework — task 044

**Date:** 2026-11-22
**Host task:** kanban/in-progress/044-test-generated-request-pattern-with-contracts-and-a-derived-attribute-in-a-referenced-assembly-and-a-one-of-response-template/
**Diff scope:** branch `task/044-test-generated-request-pattern-with-contracts-and` vs merge-base `8dd270fd` (`master`). Product diff is the request-pattern metadata-name fallback, the referenced-assembly compilation tests, the beta.11 version pins, and the readme / architecture paragraph. Kanban move of `task.md` is in the same range. The branch is one commit behind `master` (`be0a180`, kanban land, 0 lines).
**Plan / brief:** `task.md` description, checklist, and `## Goal (implementer restatement)`. Prove `GeneratedRequestPatternAttribute` for referenced contracts, a metadata-only derived attribute, a two-arity `OneOf` response, a referenced void interface and its behavior, and TWM013 when a referenced name does not resolve. Fix the inaccessible duplicate-metadata-name miss.
**Effort:** 2 (by diff, 681 lines). Roster axes: general only.
**Reviewer roster:** general
**Session IDs:** review oracle grok session `01a20381-de97-7c62-a2fa-668d1dba30ac`. Implementer session `185bce2a-93a7-4d7b-bb06-2b65ebaf3c79` (recorded on `task.md`).

## Budget (by-diff)

- Lines changed: 681
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/` and the task kitchen sections this oracle owns
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
