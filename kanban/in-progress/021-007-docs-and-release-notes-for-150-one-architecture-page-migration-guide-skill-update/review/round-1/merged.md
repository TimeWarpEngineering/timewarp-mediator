# Round 1 — merged findings
**Date:** 2026-10-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 3 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: documentation/migration.md:66 (also :170–172; documentation/release-notes/15.0.0.md:34; skills/tw-mediator/SKILL.md:220–222)
- Description: The docs say that under `using static TimeWarp.Mediator.Unit` the name `ValueTask` "binds to the property and hides `System.Threading.Tasks.ValueTask`". That is wrong. `ValueTask<T>` in type position still binds to the System type. A bare `ValueTask` in an expression is CS0229 (ambiguous). A compile probe confirmed both on net10.0 with ImplicitUsings on. "`Unit.Task` used to hide `Task`" is the same mistake. The text came from the task Notes (021-002 review M2).
- Suggestion: Describe the real rule: signatures are unaffected, and bare `ValueTask.X` in an expression is ambiguous. Write `Unit.ValueTask` for the value and `System.Threading.Tasks.ValueTask.X` for the type's statics. Apply it in migration (two places), the 15.0.0 F8 row, and the skill gotcha.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: source/timewarp-mediator/readme.md:8 (also source/timewarp-mediator-generators/readme.md:8–9, source/timewarp-mediator-analyzers/readme.md:19, readme.md:34–35)
- Description: The install lines lost `--version`. `dotnet add package TimeWarp.Mediator` resolves the latest stable version, 14.0.0, which is the reflection runtime. 15.0 ships as `15.0.0-beta.1`. The packed readmes are the 15.0 nuget.org pages, so a reader who follows them gets the wrong major version.
- Suggestion: Pin `--version 15.0.0-beta.1` (kept in step with `<Version>`), or use `--prerelease` with a note to drop it at stable. Match the skill's `Directory.Packages.props` pin.
- Source: general
- Disposition notes:

### M3 — Severity: bug — Status: open
- File: source/timewarp-mediator/readme.md:11 (also :35; skills/tw-mediator/SKILL.md:57–59; documentation/migration.md:14–17)
- Description: The `TimeWarp.Mediator` nuget page says 14.0.0 was "`TimeWarp.Mediator.Contracts` plus a separate analyzers package" and that the runtime used this id "in 13.x". In fact `TimeWarp.Mediator` 14.0.0 *is* the reflection runtime (release-notes-v14.0.0.md:22–23, :147). The skill's previous-surface list leaves out the runtime. The migration package table never says the id is reused for the contracts.
- Suggestion: Say that through 14.0.0 this id was the reflection runtime (`AddMediator`), that the 14.0.0 contracts were `TimeWarp.Mediator.Contracts`, and that 15.0 reuses this id for the contracts and the analyzer. Add the id-reuse sentence under the migration table and the runtime to the skill list.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: documentation/benchmarks/comparison-baseline.md:3–5 (and :13)
- Description: The diff hand-edits the generated header: it rewords "Do not edit by hand", adds a prose paragraph that repeats the numbers, and adds `- Task: 021-003`. `dev benchmark --record` regenerates that header (`tools/dev-cli/endpoints/benchmark-command.cs:161–165`), so the next record erases all of it, and in the meantime the paragraph can drift from the table.
- Suggestion: Revert the header edits. The 021-003 provenance already lives in architecture.md, 15.0.0.md, and agent.md. If the file must name the task, have the recorder emit that line.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. One reviewer. The `.gitignore` header duplication was considered and not raised, under the audit-fix policy (see general.md).

## Goal restatement check

The `## Goal (implementer restatement)` matches the task Description and Requirements: the
architecture page, the migration page with three consumer sections and both notes, the release
notes with finding numbers and manifest v2, agent.md and the skill, absolute-link readmes, the
benchmark pointer, a green workflow, and committed and pushed. No misread.
