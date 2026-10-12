# Review framework — task 013-019

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-019-repo-hygiene-remove-fork-migration-scratch-files-archive-design-history-fix-gitattributes-and-nugetconfig/
**Diff scope:** commit `7e7353a` vs parent `1843dd2` (merge of PR #90) on `task/013-019-repo-hygiene-remove-fork-migration-scratch-files-a`. 678 lines changed (69 insertions, 609 deletions) across 22 files. Product scope: scratch-file deletions, `git mv` of `analysis/` and the 2026-06-17 collaboration thread into `documentation/design/` and `documentation/archive/`, the orphan blip move, `.gitattributes`, `nuget.config`, `agent.md` snk note, `build-command.cs` comment, and link rewrites in m1, m2, and `.agents/collaboration/readme.md`. Kitchen `task.md` is out of product-review scope except for Results accuracy. Historical kanban snapshots that still name `Analysis/` are out of scope.
**Plan / brief:** Parent task 013 findings B-6, B-14, B-17, B-18, D-7, D-8, D-9. Remove the four fork-migration scratch files. Move the live spec to `documentation/design/` and superseded analyses plus the agent thread to an archive; fix links in m1, m2, and `.agents`. Fix `.gitattributes`, `nuget.config`, and the build-command comment. Note the committed snk and which assemblies are signed. Use `git mv` for moves.
**Effort:** 2 (general only; by-diff budget, not a specialist roster)
**Reviewer roster:** general
**Session IDs:** Grok `01a1302a-bd57-7e33-ae63-6cb21c134225` (2026-10-12)

## Budget (by-diff)

- Lines changed: 678
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
