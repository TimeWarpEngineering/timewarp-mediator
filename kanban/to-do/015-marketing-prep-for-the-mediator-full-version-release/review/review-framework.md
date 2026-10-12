# Review framework

## Budget (by-diff)

- Lines changed: 884
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 015

**Date:** 2026-10-13
**Host task:** kanban/to-do/015-marketing-prep-for-the-mediator-full-version-release/
**Diff scope:** branch `task/015-marketing-prep-for-the-mediator-full-version-relea` vs `master` (commit `62f3594`)
**Plan / brief:** Mediator 14.0.0 launch pack: refreshed NuGet readmes (four packages + repo readme), GitHub Release draft, launch and follow-up blog drafts, X/Nostr blips, 1200×675 card, video script. Nothing published.
**Effort:** 3 (by-diff budget)
**Reviewer roster:** general
**Session IDs:** Claude Code review oracle session (ganda task work, 2026-10-13); implementer Grok session 01a13140-27e9-76b2-9cfd-0e3b7a04b48f

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Claims checked against the repo

- Benchmark figures vs `documentation/benchmarks/comparison-baseline.md` (69.010 / 29.904 / 8.770 ns; 224 / 96 / 24 B; ratio 0.43 / 0.43; commit d478415; BDN 0.15.8; .NET 10.0.11; 7950X3D; Ubuntu)
- Feature matrix vs `migration.md` § 13.x reflection to 14.x generated
- Diagnostic severities vs `documentation/diagnostics.md` (TWM001/002 Error, TWM005/009 Warning)
- Aot/Link emits no `AddGeneratedMediator()` vs `generated-vs-legacy.md` and `mediator-emitter.cs`
- Removed / internalized types vs `release-notes-v14.0.0.md` breaking-change table
- License expressions vs the csproj / `source/Directory.Build.props`
- `RequestHandlerDelegate<TResponse>(CancellationToken)` signature vs Contracts
- Card PNG dimensions (1200×675) and blip lengths (259 / 265 / 278 ≤ 280)
