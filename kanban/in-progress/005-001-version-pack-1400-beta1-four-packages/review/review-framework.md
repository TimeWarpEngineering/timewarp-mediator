# Review framework — task 005-001

**Date:** 2026-09-02
**Host task:** kanban/in-progress/005-001-version-pack-1400-beta1-four-packages/
**Diff scope:** branch `task/005-001-version-pack-1400-beta1-four-packages` vs `origin/master` (product commit `828e729` chore: bump Version to 14.0.0-beta.1; kitchen `4069b23` docs(kanban): record 005-001 implementer results)
**Plan / brief:** Change unified `<Version>` from `13.0.0` to `14.0.0-beta.1` so the source-gen rewrite is not published as NuGet 13.0.0 (the last reflection MediatR-fork line). Pack Contracts, Mediator, Analyzers, Generators at that version. Package ids stay `TimeWarp.Mediator{,.Contracts,.Analyzers,.Generators}`. Keep Analyzers/Generators `IncludeSymbols=false` (004-001 NU5017). Do not `dev release` / NuGet push (005-003). Do not write consumer docs (005-002). Align both `Directory.Build.props` and `source/Directory.Build.props` so pack/release `AssertVersionSsot` stays green.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
