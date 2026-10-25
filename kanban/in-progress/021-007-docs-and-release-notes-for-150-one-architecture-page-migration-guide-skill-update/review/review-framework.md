# Review framework

## Budget (by-diff)

- Lines changed: 1350
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Scope — task 021-007

**Date:** 2026-10-26
**Host task:** kanban/in-progress/021-007-docs-and-release-notes-for-150-one-architecture-page-migration-guide-skill-update/
**Diff scope:** branch `task/021-007-docs-and-release-notes-for-150-one-architecture-pa` vs merge-base `24488ac` (commits `a87f662..dcaf5a3`): 23 files, docs, readmes, skill, `.gitignore`, kitchen
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Requirements`. Parent review `documentation/design/2026-10-13-15.0-breaking-redesign-review.md` sections 3, 5, 6
**Effort:** 3, roster axis general
**Reviewer roster:** general (review oracle, Claude Opus 5.5). Every claim was checked in the foreground against `source/`, `tools/dev-cli`, the parent review, the 14.0.0 release notes and tags, and a throwaway compile probe. Two read-only fact-check subagents covered (a) architecture and migration against the source and (b) release notes, readmes, and agent docs. The orchestrator re-verified their output against the source before merging it.
**Session IDs:** review oracle, headless print mode, session id not exposed

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- For a docs task, a wrong technical claim a consumer would act on is a `bug`
- Prior rounds are immutable; new work goes in `round-(N+1)/`
