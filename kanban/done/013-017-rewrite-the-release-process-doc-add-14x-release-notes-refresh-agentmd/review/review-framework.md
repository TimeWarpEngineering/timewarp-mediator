# Review framework — task 013-017

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-017-rewrite-the-release-process-doc-add-14x-release-notes-refresh-agentmd/
**Diff scope:** commits `5131840` and `b6031c5` vs parent `f6f70ca` (merge of PR #88) on `task/013-017-rewrite-the-release-process-doc-add-14x-release-no`. 511 lines changed (319 insertions, 192 deletions). Product files: `agent.md`, `documentation/generated-vs-legacy.md`, `documentation/overview.md`, `documentation/release-notes/release-notes-v13.0.0.md` (rename from repo root), `documentation/release-notes/release-notes-v14.0.0.md`, `readme.md`. Kitchen `task.md` is in the same range and is out of product-review scope except for Results accuracy.
**Plan / brief:** Rewrite `documentation/overview.md` to the real release flow (version bump PR, GitHub Release, OIDC via `workflow.yml`, probe and break-glass, `dev workflow` / `dev release`, four packages, `Directory.Build.props` versioning, link `tw-release`). Move v13 notes under `documentation/release-notes/`, add a 14.0.0-beta rolling note (beta.1–beta.4 and the not-a-drop-in warning), link from readme. Refresh `agent.md` from the csproj files and `./bin/dev --help`. Parent findings B-4, B-5, B-15, D-1, D-6. Out of scope: analysis-file archive (013-019), readme restructure and 13.x→14.x migration (013-018).
**Effort:** 2 (general only; by-diff budget, not a specialist roster)
**Reviewer roster:** general
**Session IDs:** Grok `01a13008-1f8a-78f2-a86b-e35ce86f7320` (2026-10-12)

## Budget (by-diff)

- Lines changed: 511
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
- This repo may differ from the generic `tw-release` promotion pipeline. Prefer `workflow.yml` and `tools/dev-cli` in this worktree over the skill's default "promote the CI artifact" story.
