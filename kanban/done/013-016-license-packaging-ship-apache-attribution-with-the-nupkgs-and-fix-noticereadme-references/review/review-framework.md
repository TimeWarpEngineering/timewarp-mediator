# Review framework — task 013-016

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-016-license-packaging-ship-apache-attribution-with-the-nupkgs-and-fix-noticereadme-references/
**Diff scope:** product commit `1a2caf5` vs parent `ae5151d` on `task/013-016-license-packaging-ship-apache-attribution-with-the` (89 lines in the commit, including the kitchen `task.md`). Product files: `notice`, `readme.md`, `source/Directory.Build.props`, `source/timewarp-mediator/timewarp-mediator.csproj`, `source/timewarp-mediator-contracts/timewarp-mediator-contracts.csproj`, `tools/dev-cli/endpoints/pack-command.cs`, `tools/dev-cli/services/repo-layout.cs`.
**Plan / brief:** Keep Apache attribution. Mediator and Contracts use `Unlicense AND Apache-2.0`; Generators and Analyzers stay `Unlicense`. Pack `notice` and `unlicense` into all four nupkgs. Fix readme file names, fill the notice Appendix, add years to `Copyright`. Parent findings B-1, B-2.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** Grok `01a12ffd-0563-7712-8e91-bbf01237d17c` (2026-10-12)

## Budget (by-diff)

- Lines changed: 89
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
