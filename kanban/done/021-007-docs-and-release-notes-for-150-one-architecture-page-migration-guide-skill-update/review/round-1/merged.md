# Round 1 — merged findings
**Date:** 2026-10-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 5 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 1 | 0 | 0 |

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

### M5 — Severity: bug — Status: open
- File: documentation/architecture.md:154–157, :172, :176 (also documentation/migration.md:184–185; agent.md:108–109; skills/tw-mediator/SKILL.md:211–215)
- Description: The docs say `TimeWarpMediatorAssembly=false` stops a generator-referencing library from emitting a mediator, and they tell Nuru to rely on that. The generator never reads the property. `CandidateGraph.Create` always adds the host and emits (`candidate-graph.cs:52–55`, `:333–334`). Only the analyzer's `Membership` reads `IsMsBuildMember` (`membership.cs:81`, `:111`). The table headed "properties the generator reads" lists two properties that only the analyzer reads.
- Suggestion: Get a maintainer call on which side is wrong. Either the generator honors `false`, or the docs say the property is analyzer membership only and that a library avoids emission by not referencing Generators. Fix the Nuru bullet and the table heading to match.
- Source: general (fact-check subagent, re-verified by orchestrator)
- Disposition notes:

### M6 — Severity: bug — Status: open
- File: documentation/architecture.md:165–166 (also :92)
- Description: The page says "the first attribute is outermost, `Order` is the tie-break". The code makes `Order` the primary key, with source position as the tie-break (`candidate-graph.cs:179–180`, `CompareOrdered`). Separately, :92 says a handler class is "registered once per interface". It is bound once per interface and registered once per concrete type (`mediator-emitter.cs:985–993`).
- Suggestion: Say that a lower `Order` is outermost and declaration order breaks ties. Change :92 to "bound per interface, registered once".
- Source: general (fact-check subagents, re-verified by orchestrator)
- Disposition notes:

### M7 — Severity: suggestion — Status: open
- File: documentation/release-notes/15.0.0.md:26, :29
- Description: The typed-`Send` row should cite F2, because the non-boxing switch is what returns the `Dispatch_*` `ValueTask` directly. The `[GeneratedMediator]` row should cite F10, because the parent review proposes the marker there (:311–315).
- Suggestion: Cite F2 and F3 (F7 optional) on the first, and F10 and F11 on the second.
- Source: general (fact-check subagent, re-verified by orchestrator)
- Disposition notes:

### M8 — Severity: suggestion — Status: open
- File: readme.md:41–58 (also documentation/m2-named-pipelines.md:30–42)
- Description: The quick start puts top-level statements after type declarations (CS8803). It also resolves `ISender` from an undeclared root `provider`, while :58 says to resolve it from a scope. In m2, `[assembly: …]` comes after type declarations (CS1730).
- Suggestion: Put the statements first, use `CreateScope()`, and move the assembly attributes to the top.
- Source: general (fact-check subagent, re-verified by orchestrator)
- Disposition notes:

### M9 — Severity: nit — Status: open
- File: source/timewarp-mediator-analyzers/readme.md:40 (also skills/tw-mediator/SKILL.md:3; documentation/release-notes/15.0.0.md:19)
- Description: These phrases are stale:
  - The analyzer readme says "even when the library does not reference `TimeWarp.Mediator`". That is impossible in 15.0.
  - The skill description's range "TWM007-TWM011" includes the retired TWM009.
  - The runtime is called "the 14.x dispatcher". In 14.x it was the legacy reflection dispatcher.
- Suggestion: Drop the clause, list the live ids, and say "the 13.x/14.x reflection dispatcher".
- Source: general (fact-check subagent, re-verified by orchestrator)
- Disposition notes:

## Duplicates / conflicts

- Both fact-check subagents independently reported M2, M3, M4, and M6, and they are collapsed into those ids. The architecture subagent also flagged M1's shadowing claim as unverified. The compile probe settles it.
- The `.gitignore` header duplication was considered and not raised, under the audit-fix policy (see general.md).
- Not raised: `<Version>` is still 14.0.0. Neither this task nor parent 021 scopes the bump, and the release flow handles it in its own PR. Note for the release: packing 14.0.0 with 15.0 contents reuses an id and version already on nuget.org, so `check-version` must stop a push until the bump lands.

## Goal restatement check

The `## Goal (implementer restatement)` matches the task Description and Requirements: the
architecture page, the migration page with three consumer sections and both notes, the release
notes with finding numbers and manifest v2, agent.md and the skill, absolute-link readmes, the
benchmark pointer, a green workflow, and committed and pushed. No misread.
