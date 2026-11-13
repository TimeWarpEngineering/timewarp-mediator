# Review framework

## Budget (by-diff)

- Lines changed: 634
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Scope (task 039)

**Date:** 2026-11-14
**Host task:** kanban/in-progress/039-target-net11-0-only-drop-net10-0/
**Diff scope:** branch `task/039-target-net11-0-only-drop-net10-0` vs `master` (`git diff master...HEAD`, product commits `eafa561` and `4519b6b`, 634 lines changed across 41 non-kanban files)
**Plan / brief:** `task.md` Description, Requirements (including Steven's 2026-11-14 superseded directive moving the generator and analyzer to `net11.0`), and `## Goal (implementer restatement)`
**Effort:** 2 (by-diff budget above)
**Reviewer roster:** general
**Session IDs:** review oracle round 1 (Cursor, vendor google, model gemini-4-carbon-high, session not reported), max-turns uncapped (no --max-turns flag; bound 120), headless `ganda task work`, 2026-11-14

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
