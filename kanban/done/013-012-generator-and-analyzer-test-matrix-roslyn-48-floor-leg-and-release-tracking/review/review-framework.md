# Review framework — task 013-012

**Date:** 2026-10-12
**Host task:** kanban/to-do/013-012-generator-and-analyzer-test-matrix-roslyn-48-floor-leg-and-release-tracking/
**Diff scope:** commit f85c41c (`test(generators): shape matrix, analyzer negatives, Roslyn 4.8 floor leg, release tracking`) vs its parent 4d7e730. 30 files, 1531 insertions, 104 deletions (1635 lines changed). Not the older origin/home history.
**Plan / brief:** Shape matrix, analyzer negatives with locations, manifest JSON parsing, Roslyn 4.8 floor leg, TWM001–TWM004 release tracking, minimum SDK docs. Parent 013 findings G-16, G-20, G-23, G-24, B-16. Requirements are on task.md.
**Effort:** 3 (general only)
**Reviewer roster:** general
**Session IDs:** Grok review oracle 01a12fb7-6d0f-7b23-93ef-efc796fc4ca6 (2026-10-12)

## Budget (by-diff)

- Lines changed: 1635
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Scope is this task's commit only. Do not re-litigate 013-001 through 013-011.

## Review focus

- Shape matrix: each Host/Aot row compiles with no warnings or errors and binds the listed request and notification handlers
- Analyzer negatives: near-miss shapes stay quiet in library and host membership; TWM001 and TWM004 locations are the declared identifiers or the wrong `Send` invocation
- Manifest assertions parse `MediatorManifest.Json` fields; substring matches on the JSON are gone
- `PipelineLog` is per async flow, and the serializing xunit collection is removed
- Floor project links the compilation tests, pins `Microsoft.CodeAnalysis.CSharp` 4.8.0, and guards the loaded compiler version
- Struct requests omit `request is null` (CS0037); reference-type sends still throw `ArgumentNullException`
- TWM001–TWM004 live under `## Release 14.0.0` in `AnalyzerReleases.Shipped.md` (RS2007 rejects a prerelease header) with beta.1 in the Notes column; Unshipped keeps TWM005–TWM011
- Minimum SDK 8.0.100 / VS 2022 17.8 is in `readme.md` and `documentation/generated-vs-legacy.md`, and the release checklist moves Unshipped rows
