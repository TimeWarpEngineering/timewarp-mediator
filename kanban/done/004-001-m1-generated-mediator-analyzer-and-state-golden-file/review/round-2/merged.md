# Round 2 — merged findings
**Date:** 2026-08-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 3 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: src/TimeWarp.Mediator.Analyzers/MessageGraphBuilder.cs:363
- Description: `TryCloseBehavior` ended with `return ImplementsPipeline(...) ? constructed : constructed;`, so a failed pipeline-shape check still returned the constructed type.
- Suggestion: Return `null` when `ImplementsPipeline` is false; add arity-1 `Unit`-only behavior regression.
- Source: general
- Disposition notes: Returns `? constructed : null`. Regression `UnitOnlyBehavior_ClosesOntoUnitRequestsOnly` asserts Increment closes `UnitOnlyBehavior<>` and Ping does not. Generators.Tests 10 passed.

### M2 — Severity: suggestion — Status: fixed
- File: src/TimeWarp.Mediator.Generators/ManifestEmitter.cs:1
- Description: Purpose/docs described a loose `mediator.manifest.json` and unused `IntermediateOutputPath` / `ProjectDir` wiring; emit was only `MediatorManifest.g.cs`.
- Suggestion: Narrow to embedded `MediatorManifest` v1 and drop unused path wiring.
- Source: general
- Disposition notes: Purpose, csproj description, and `Documentation/m1-generated-mediator.md` now describe the embedded JSON const. Membership no longer stores `ProjectDir`/`IntermediateOutputPath`; generator props no longer expose those CompilerVisibleProperties. Golden test no longer writes a file.

### M3 — Severity: suggestion — Status: fixed
- File: src/TimeWarp.Mediator.Analyzers/MessageGraphBuilder.cs:266
- Description: `DiscoverBehaviors` walked `MemberAssemblies` with no name sort before `SourceIndex`, so multi-assembly order:0 ties could be hash-set-dependent.
- Suggestion: Order member assemblies by `assembly.Name` (Ordinal) before assigning `SourceIndex`.
- Source: general
- Disposition notes: `OrderBy(a => a.Name, StringComparer.Ordinal)` before the attribute walk.

### M4 — Severity: suggestion — Status: fixed
- File: src/TimeWarp.Mediator.Generators/MediatorEmitter.cs:352
- Description: Aot `Dispatch_*` skips `ClosedBehaviors` / `[assembly: MediatorBehavior]` with no mention in docs.
- Suggestion: Document that Aot/ServiceGen M1 drops behaviors.
- Source: general
- Disposition notes: Documented in `MediatorEmitter` Design region and `Documentation/m1-generated-mediator.md`. Host/State remains the scoped pipeline path.

## Duplicates / conflicts

- None. Prior M1–M4 carried forward with updated status. No new IDs.
