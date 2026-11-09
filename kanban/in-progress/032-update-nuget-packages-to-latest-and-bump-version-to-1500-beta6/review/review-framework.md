# Review framework

## Budget (by-diff)

- Lines changed: 143
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 032

**Date:** 2026-11-09
**Host task:** kanban/in-progress/032-update-nuget-packages-to-latest-and-bump-version-to-1500-beta6/
**Diff scope:** branch `task/032-update-nuget-packages-to-latest-and-bump-version-t` vs `master` (merge base), product paths only. Master is 2 commits ahead, and both are kanban landings.
**Plan / brief:** move `TimeWarp.SourceGenerators` to `1.0.0-beta.12`, set `<Version>` to `15.0.0-beta.6`, update install snippets, add the `15.0.0-beta.6` release note. The task 023-001 pins stay as they are.
**Effort:** 1 (general only)
**Reviewer roster:** general (the review oracle reviewed inline; no subagents)
**Session IDs:** review oracle, Claude Code (claude-opus-5-5), session id not shown to the reviewer

## Ground rules

- Reviewers do not edit product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Review the diff and the code that calls it. Check each claim that can be tested against the repo and nuget.org.
- Prior rounds do not change. New work goes in `round-(N+1)/`.
