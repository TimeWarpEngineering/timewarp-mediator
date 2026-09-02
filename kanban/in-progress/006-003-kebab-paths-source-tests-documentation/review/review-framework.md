# Review framework — task 006-003

**Date:** 2026-09-02
**Host task:** kanban/in-progress/006-003-kebab-paths-source-tests-documentation/
**Diff scope:** branch `task/006-003-kebab-paths-source-tests-documentation` vs `origin/master` (product commit `c074fe3` chore: kebab-case layout folders, projects, and non-cs paths; kitchen `a4b7a7b` docs(kanban): record 006-003 implementer results)
**Plan / brief:** Move the tree to TimeWarp layout and kebab-case **folders / csproj / slnx / non-cs files**. Leave `.cs` basenames PascalCase for **006-004**. Package **ids** stay `TimeWarp.Mediator*`. `src/` → `source/`, `test/` → `tests/`, `Documentation/` → `documentation/`, `Assets/` → `assets/`. Dual `Kanban/` archived. Delete `TimeWarp.Mediator.sln`; keep `timewarp-mediator.slnx`; CI path filters `source/**` `tests/**`. Strong-name `.snk` bytes unchanged. Out of scope: C# language style (006-004), full audit green (006-005).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok (2026-09-02); round-2 review oracle grok (2026-09-02)

## Round 2 scope (fix loop 2026-09-02)

**Diff scope:** live docs `documentation/m1-generated-mediator.md` and `documentation/m2-named-pipelines.md` after M1 path refresh vs round-1 product commit `c074fe3`.
**Plan / brief:** Re-verify M1 (stale PascalCase sample/`Analysis/` paths) against the post-fix files. Scan the fix delta for new defects. Do not re-litigate round-1 empty areas. Do not start 006-004. Do not merge.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
