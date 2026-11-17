# Review framework — task 045

**Date:** 2026-11-17
**Host task:** kanban/in-progress/045-bump-time-warp-source-generators-to-1-0-0-beta-17/
**Diff scope:** branch `task/045-bump-time-warp-source-generators-to-1-0-0-beta-17` vs `master` (merge-base `cace7f9`). Product change is the `TimeWarp.SourceGenerators` pin in `Directory.Packages.props`. Kitchen edits are `task.md` (move from to-do plus implementer Results).
**Plan / brief:** Bump the central pin from 1.0.0-beta.16 to 1.0.0-beta.17, record the upstream delta, and record whether `TimeWarp.Mediator` generated source or public API changed. No version bump unless shipped output needs a release note.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok `01a1e9d2-5336-7e72-b155-d44611606a08` (2026-11-17). Implementer session `4e297d87-c0c9-4656-bc7a-51368a603563` (claude).

## Budget (by-diff)

- Lines changed: 129
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
