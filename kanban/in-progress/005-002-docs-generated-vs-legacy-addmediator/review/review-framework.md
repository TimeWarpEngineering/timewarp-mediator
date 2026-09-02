# Review framework — task 005-002

**Date:** 2026-09-02
**Host task:** kanban/in-progress/005-002-docs-generated-vs-legacy-addmediator/
**Diff scope:** branch `task/005-002-docs-generated-vs-legacy-addmediator` vs `origin/master` (product commit `846f731` docs: distinguish generated AddGeneratedMediator from reflection AddMediator; kitchen `d6b4de2` docs(kanban): record 005-002 implementer results)
**Plan / brief:** Consumer docs so State/Nuru do not call `AddMediator()` and think they got source-gen. README + `documentation/`: `AddMediator()` = 13.x reflection fork (still in this repo); `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` = generated. Membership `[assembly: MediatorAssembly]`, `[MediatorScope]`, `[MediatorBehavior]`. Named pipelines are marker types (`ClientPipeline` / `ServerPipeline`), not strings. Host packages: Contracts + Generators (Analyzers only if generator is not referenced). Explicit: 14.0.0-beta is untested outside M1/M2 golden files; not a drop-in for 13.0.0. Issue #52 stays open until a stable 14.0.0.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
