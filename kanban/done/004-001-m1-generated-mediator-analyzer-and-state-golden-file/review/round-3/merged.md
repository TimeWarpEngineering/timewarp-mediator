# Round 3 — merged findings
**Date:** 2026-09-01
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: src/TimeWarp.Mediator.Analyzers/MessageGraphBuilder.cs:365
- Description: `TryCloseBehavior` ended with a tautological ternary so a failed pipeline-shape check still returned the constructed type.
- Suggestion: Return `null` when `ImplementsPipeline` is false; add arity-1 `Unit`-only behavior regression.
- Source: general
- Disposition notes: Still `? constructed : null` (and the non-generic path at line 341). Pack-fix did not touch this. Round 3 re-verified.

### M2 — Severity: suggestion — Status: fixed
- File: src/TimeWarp.Mediator.Generators/ManifestEmitter.cs:1
- Description: Purpose/docs described a loose `mediator.manifest.json` and unused path wiring; emit was only `MediatorManifest.g.cs`.
- Suggestion: Narrow to embedded `MediatorManifest` v1 and drop unused path wiring.
- Source: general
- Disposition notes: Still an embedded JSON const. Pack-fix did not touch this. Round 3 re-verified.

### M3 — Severity: suggestion — Status: fixed
- File: src/TimeWarp.Mediator.Analyzers/MessageGraphBuilder.cs:269
- Description: `DiscoverBehaviors` walked member assemblies with no name sort before `SourceIndex`.
- Suggestion: Order member assemblies by `assembly.Name` (Ordinal) before assigning `SourceIndex`.
- Source: general
- Disposition notes: Still `OrderBy(a => a.Name, StringComparer.Ordinal)`. Pack-fix did not touch this. Round 3 re-verified.

### M4 — Severity: suggestion — Status: fixed
- File: src/TimeWarp.Mediator.Generators/MediatorEmitter.cs
- Description: Aot `Dispatch_*` skips `ClosedBehaviors` / `[assembly: MediatorBehavior]` with no mention in docs.
- Suggestion: Document that Aot/ServiceGen M1 drops behaviors.
- Source: general
- Disposition notes: Still documented in `Documentation/m1-generated-mediator.md`. Pack-fix did not touch this. Round 3 re-verified.

## Duplicates / conflicts

- None. Prior M1–M4 carried forward with status `fixed`. No new IDs. Pack-fix (`IncludeSymbols=false` + `Build.ps1` nupkg asserts) raised no findings.
