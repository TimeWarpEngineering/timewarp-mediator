# Review framework — task 018

**Date:** 2026-10-13
**Host task:** kanban/to-do/018-add-tw-mediator-agent-skill-skillstw-mediatorskillmd-before-1400-stable/
**Diff scope:** branch `task/018-add-tw-mediator-agent-skill-skillstw-mediatorskill` vs `origin/master` (merge-base `f04f85f`), commit `b2897eb`. Product files: `skills/tw-mediator/SKILL.md`, `readme.md`, deletion of `skills/.gitkeep`. Kanban `task.md` session/results edits are not product claims.
**Plan / brief:** Add `skills/tw-mediator/SKILL.md` (dispatcher choice, minimal generated host, profiles, TWM001–TWM011, dogfood gotchas, 13.x→14.0 pointers) and a readme "For AI agents" paragraph, sourced from the existing docs so the two cannot disagree. Samples must match 14.0.0-beta.5 behavior. Length 250–350.
**Effort:** 2 (deeper single pass; host budget by-diff, 352 lines)
**Reviewer roster:** general
**Session IDs:** review oracle Grok session `01a13543-65d3-7a02-bd6a-f600ef005ce9` (2026-10-13). Implement oracle: implementer-claude, session not reported (2026-10-13).

## Budget (by-diff)

- Lines changed: 352
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
- Authoritative behavior is the code and the consumer docs (`documentation/diagnostics.md`, `documentation/generated-vs-legacy.md`, `documentation/m2-named-pipelines.md`, `migration.md`, `documentation/release-notes/release-notes-v14.0.0.md`), not the archived design notes
