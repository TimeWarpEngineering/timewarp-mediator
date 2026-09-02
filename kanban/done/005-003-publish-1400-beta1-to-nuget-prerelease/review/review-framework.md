# Review framework — task 005-003

**Date:** 2026-09-02
**Host task:** kanban/in-progress/005-003-publish-1400-beta1-to-nuget-prerelease/
**Diff scope:** branch `task/005-003-publish-1400-beta1-to-nuget-prerelease` vs `origin/master` (product commit `ecdb66d` docs: record that nuget.org serves 14.0.0-beta.1 as prerelease; kitchen `1257816` docs(kanban): record 005-003 implementer results). External publish at tag `v14.0.0-beta.1` peeled to `40a9841270ab14d6694c0373bf6d83bb3dde9d6e` (origin/master HEAD).
**Plan / brief:** After 005-001 version bump and 006-005 audit gate, cut NuGet **prerelease** `14.0.0-beta.1` via `dev release` / trusted-publishing workflow. Four package ids on nuget.org as prerelease. Do not unlist/overwrite 13.0.0. Do not ship stable `14.0.0` / tag `v14.0.0`. GitHub Latest stays `v13.0.0`. Repo docs (`readme.md`, `documentation/generated-vs-legacy.md`) no longer say nuget.org still serves only 13.0.0 until this task.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Requirements to re-verify

- Tag/version is `14.0.0-beta.1` (prerelease)
- All four packages on nuget.org as **prerelease**: `TimeWarp.Mediator{,.Contracts,.Analyzers,.Generators}`
- Do **not** unlist or overwrite 13.0.0
- Do **not** ship 14.0.0 without `-beta`
- Issue #52 stays open (stable 14.0.0 is not this epic)
