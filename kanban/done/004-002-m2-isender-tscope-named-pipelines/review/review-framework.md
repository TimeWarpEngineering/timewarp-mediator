# Review framework — task 004-002

**Date:** 2026-09-01
**Host task:** kanban/in-progress/004-002-m2-isender-tscope-named-pipelines/
**Diff scope:** commit `5f94fb5` vs `origin/master` — Contracts `ISender<TScope>` / `IPublisher<TScope>` / `[MediatorScope]`, analyzer TWM003/TWM004, generator per-scope Sender/Publisher emit, scoped tests, NamedPipelines sample, spec §9.1 fold-in, docs
**Plan / brief:** M2 named pipelines per `kanban/in-progress/004-002-m2-isender-tscope-named-pipelines/task.md` and `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §9.1: marker-type `ISender<TScope>` / `IPublisher<TScope>` as separate generated classes; membership via `[MediatorScope]` (closest type wins; assembly default); `[assembly: MediatorBehavior(..., Scope = typeof(TScope))]`; unscoped `AddGeneratedMediator()` vs `AddGeneratedMediator<TScope>()`; TWM004 compile error on typed wrong-scope send; `Send(object)` hard miss; behaviors do not cross pipelines; re-entrant Send stays in-scope; MS.DI resolves scopes independently. Out of scope: TimeWarp.State NuGet switch, call-site interceptors/pruning, streams, TimeWarp.ServiceGen.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Review oracle: grok (2026-09-01); implementer: grok session 01a0591d-acec-71b0-9b3e-30fcebf1e991 (2026-09-01)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
