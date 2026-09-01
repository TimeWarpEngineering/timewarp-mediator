# Review framework — task 006-001

**Date:** 2026-09-01
**Host task:** kanban/in-progress/006-001-scaffold-audit-fix-layout-dirs-and-cpm/
**Diff scope:** branch `task/006-001-scaffold-audit-fix-layout-dirs-and-cpm` vs `origin/master` (commit `2127c74` chore: scaffold TimeWarp layout dirs and Central Package Management)
**Plan / brief:** Mechanical TimeWarp scaffold. Prefer `ganda repo audit --fix` for Fixable=YES. In scope: CPM, `msbuild/repository.props`, `source/Directory.Build.props`, root `.slnx`, `.envrc`, gitignore journals, editorconfig sentinels, empty `kanban/{backlog,in-progress,archived}/`. Out of scope: `tools/dev-cli` (006-002), kebab-rename of `src/` (006-003), rewriting `.cs` files (006-004).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-01)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
