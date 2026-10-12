# Review framework — task 013-006

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-006-annotate-the-reflection-runtime-for-trimming-and-aot-analysis/
**Diff scope:** commit `8224719` (`feat(runtime): annotate reflection entry points for trimming and AOT`) vs its parent. 196 insertions, 24 deletions (220 lines). Not the whole branch vs `origin/master` (that range includes already-reviewed 013-001..013-005 work).
**Plan / brief:** Parent finding C-14. Annotate `AddMediator` and the legacy reflection entry points with `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` (polyfill or `#if` for netstandard2.0 / net6.0). Cache `MethodInfo` per (request, response, exception) in the exception behaviors. Document in `documentation/generated-vs-legacy.md` that the reflection runtime is not trim or AOT safe and the generated path is. AOT sample publish stays free of new warnings.
**Effort:** 2 (by-diff, 220 lines; roster axis general only)
**Reviewer roster:** general
**Session IDs:** review oracle Grok session `01a12ef0-c0e7-7dc0-aedc-0f1c19826ae7` (2026-10-12)

## Budget (by-diff)

- Lines changed: 220
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
