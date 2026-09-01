# Review framework — task 006-001

**Date:** 2026-09-01
**Host task:** kanban/in-progress/006-001-scaffold-audit-fix-layout-dirs-and-cpm/
**Diff scope:** branch `task/006-001-scaffold-audit-fix-layout-dirs-and-cpm` vs `origin/master` (commit `2127c74` chore: scaffold TimeWarp layout dirs and Central Package Management)
**Plan / brief:** Mechanical TimeWarp scaffold. Prefer `ganda repo audit --fix` for Fixable=YES. In scope: CPM, `msbuild/repository.props`, `source/Directory.Build.props`, root `.slnx`, `.envrc`, gitignore journals, editorconfig sentinels, empty `kanban/{backlog,in-progress,archived}/`. Out of scope: `tools/dev-cli` (006-002), kebab-rename of `src/` (006-003), rewriting `.cs` files (006-004).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-01); round-2 review oracle grok (2026-09-01); round-3 review oracle grok (2026-09-02)

## Round 2 scope (reopen 2026-09-01)

**Diff scope:** commit `e5b3f7f` (MSB1011 fix) vs `9a2e436` (reopen); files: `Build.ps1`, `Agent.md`. Task kitchen also updated.
**Plan / brief:** PR #55 CI failed MSB1011 because root has both `TimeWarp.Mediator.sln` and `timewarp-mediator.slnx`. Slice: point `Build.ps1` (and Agent.md unadorned `dotnet` commands) at `timewarp-mediator.slnx`. Keep the `.sln`. Do not start 006-002/006-003. Do not merge. Re-verify round-1 empty finding list still holds for the original scaffold.

## Round 3 scope (reopen 2026-09-02)

**Diff scope:** commit `aca68a2` (timeout-test serialization) vs `5c93c61` (reopen); product file: `test/TimeWarp.Mediator.Tests/AssemblyInfo.cs`. Task kitchen also updated (`7500b6e`).
**Plan / brief:** PR #55 CI still red after MSB1011: `GenericRequestHandlerTests.ShouldThrowExceptionWhenTimeoutOccurs` expected `TimeoutException`, got `ArgumentException` MaxTypesClosing 100. Slice: test-only fix unless MaxTypesClosing 0→100 is a real scaffold regression. Prefer not skip / not widen assertion. Do not start 006-002/006-003. Do not merge. Re-verify round-1/round-2 empty finding lists still hold (scaffold + slnx CI path).

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
