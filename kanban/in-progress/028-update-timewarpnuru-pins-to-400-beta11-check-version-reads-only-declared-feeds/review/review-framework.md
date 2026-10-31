# Review framework

## Budget (by-diff)

- Lines changed: 274
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 028

**Date:** 2026-10-31
**Host task:** kanban/in-progress/028-update-timewarpnuru-pins-to-400-beta11-check-version-reads-only-declared-feeds/
**Diff scope:** branch `task/028-update-timewarpnuru-pins-to-400-beta11-check-versi` vs merge-base with `master` (product commit 9ad9aac), product paths
**Plan / brief:** task.md Requirements and the Addendum (move Nuru to beta.11, `<Version>` to 15.0.0-beta.4, rename the release note)
**Effort:** 2 (from the by-diff budget above), roster general only
**Reviewer roster:** general
**Session IDs:** review oracle claude-opus-5-5 (Claude Code, headless print mode)

## Ground rules

- Reviewers do not edit product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Every new finding starts open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the call sites around it. Check every falsifiable claim against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
