# Review framework — task 004-001

**Date:** 2026-08-31
**Host task:** kanban/in-progress/004-001-m1-generated-mediator-analyzer-and-state-golden-file/
**Diff scope:** commit `b42ce9b` vs `origin/master` — analyzer + generator packages, Contracts M1 surface, State golden-file tests, AOT sample, benchmarks, spec header/§9, docs
**Plan / brief:** M1 rewrite core per `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §14: TWM001/TWM002, handler-first membership, generated `sealed Mediator : IMediator`, `Send(object)` switch, ValueTask/`IAction` contracts, Host scope-resolved pipeline + Aot ServiceGen sample, IncrementActionSet + StateTransactionBehavior golden file matching `Reverse().Aggregate`, `mediator.manifest.json` v1, AOT sample trim/AOT-analyzer-clean with no IL2026/IL3050 `NoWarn`. Out of scope: `ISender<TScope>` (004-002), interceptors as default dispatch, pruning, streams, TimeWarp.State NuGet switch.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Review oracle: grok (2026-08-31); implementer: grok session 2438044 (2026-08-31); reopen implementer: grok 01a058f1-1b82-76d3-aef4-e895f0b2e2e2 (2026-09-01); round-3 review oracle: grok (2026-09-01)

## Round 3 scope (reopen 2026-09-01)

**Diff scope:** commit `c8c7caa` (pack-fix) vs `37ae9ee` (reopen); files: `TimeWarp.Mediator.Analyzers.csproj`, `TimeWarp.Mediator.Generators.csproj`, `Build.ps1`, `Agent.md`.
**Plan / brief:** PR #53 CI failed NU5017 packing an empty Analyzers snupkg (`IncludeBuildOutput=false` + repo-wide `IncludeSymbols`/`snupkg`). Slice: non-empty Analyzers nupkg with `analyzers/dotnet/cs` DLL; same for Generators; Mediator/Contracts still emit snupkg. Re-verify round-1 M1–M4 remain fixed. Do not start 004-002. Do not merge.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
