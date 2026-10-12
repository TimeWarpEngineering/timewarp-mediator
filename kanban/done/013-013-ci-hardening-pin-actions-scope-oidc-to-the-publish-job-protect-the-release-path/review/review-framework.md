# Review framework — task 013-013

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-013-ci-hardening-pin-actions-scope-oidc-to-the-publish-job-protect-the-release-path/
**Diff scope:** `5fdd7fd..HEAD` (commits `6e8f31f`, `ec1ffad`, `e6ae3f6`). Product files: `.github/workflows/workflow.yml`, `.github/dependabot.yml`, `documentation/overview.md`, `tools/dev-cli/dev.cs`, `tools/dev-cli/endpoints/workflow-command.cs`. 238 insertions, 61 deletions (299 lines; matches Budget.ByDiff). Not `origin/master...HEAD` — that range still contains the earlier 013 task merges, which are not on `origin/master`.
**Plan / brief:** task.md — SHA-pin actions, split the OIDC publish job, pass the NuGet key via `NUGET_API_KEY`, guard tag/master/green CI, add concurrency/timeouts/cache, drop path filters, keep trusted publishing on `workflow.yml`.
**Effort:** 2 (Budget.ByDiff: 299 lines changed; roster axes: general; turn cap 120)
**Reviewer roster:** general
**Session IDs:** Grok session `01a12fcb-8780-7f83-b446-2936ad131c6f` (2026-10-12)

## Budget (by-diff)

- Lines changed: 299
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
