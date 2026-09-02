# Review framework — task 006-002

**Date:** 2026-09-02
**Host task:** kanban/in-progress/006-002-dev-cli-replace-ps1-scripts/
**Diff scope:** branch `task/006-002-dev-cli-replace-ps1-scripts` vs `origin/master` (product commit `248a031` feat: replace PowerShell with TimeWarp Nuru dev CLI; kitchen `2845702` docs(kanban): record 006-002 implementer results)
**Plan / brief:** Add TimeWarp `tools/dev-cli` + `bin/dev`. Replace PowerShell as the build/test/pack/push/hooks path. Map former `Build.ps1` / `Push.ps1`. CI `workflow.yml` must call `dev` (or `dotnet run --file tools/dev-cli/dev.cs --`) not ad-hoc pwsh. GitHooks: TimeWarp `.githooks` / ganda memsearch, not `Tools/GitHooks/*.ps1`. Delete leftover `.ps1`. Out of scope: kebab rename (006-003), full audit green (006-005).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02); round-2 review oracle grok (2026-09-02)

## Round 2 scope (fix loop 2026-09-02)

**Diff scope:** commit `ee2dc97` (M1 Version SSOT assert + M2 wipe pack output) vs `2845702` (implementer results). Files: `tools/dev-cli/services/repo-layout.cs`, `tools/dev-cli/endpoints/workflow-command.cs`, `tools/dev-cli/endpoints/pack-command.cs`.
**Plan / brief:** Re-verify M1/M2 against the post-fix diff. Scan the fix delta for new defects. Do not re-litigate round-1 empty areas. Do not start 006-003. Do not merge.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
