# Review framework

## Budget (by-diff)

- Lines changed: 89
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Scope — task 022

**Date:** 2026-10-28
**Host task:** kanban/in-progress/022-update-outdated-packages-after-1500-beta1-keep-the-roslyn-500-floor/
**Diff scope:** branch `task/022-update-outdated-packages-after-1500-beta1-keep-the` (HEAD `2ca6dce`) vs `master` merge base (master at `7fa6491`). Product paths: `Directory.Packages.props`, `.githooks/pre-push.cs`, `glossary.md`.
**Plan / brief:** bump five outdated central pins (Nuru, Nuru.DevCli, DI, Test.Sdk, SourceLink). Keep both `Microsoft.CodeAnalysis.CSharp*` pins at 5.0.0. Apply `ganda repo audit --fix`. No version change.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle (claude, claude-opus-5-5), 2026-10-28

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
