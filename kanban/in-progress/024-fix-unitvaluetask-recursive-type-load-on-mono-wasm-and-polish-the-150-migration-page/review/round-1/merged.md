# Round 1 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: documentation/migration.md:159
- Description: The `IncrementHandler : ActionHandler<Increment.Action>` example returns
  `Unit.ValueTask` (`ValueTask<Unit>`) from `override ValueTask Handle(...)`. That is
  CS0029, reproduced with a throwaway runfile. The example meant to document the
  `IActionHandler<>` replacement does not compile.
- Suggestion: Return `ValueTask.CompletedTask` (or `default`). Add an `ActionHandler<T>`
  subclass to the generators test project in the same shape so the doc example stays
  compiled.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: documentation/migration.md:268
- Description: The Nuru bullet names the internal C# constants (`KindQuery`, …) as what the
  library reads from the JSON. The library cannot name them. The JSON values are `"Query"`,
  `"IdempotentCommand"`, `"Command"`, `"Action"`, and `"Request"`.
- Suggestion: List the JSON string values.
- Source: general
- Disposition notes:

### M3 — Severity: nit — Status: open
- File: documentation/migration.md:193 (also `documentation/release-notes/15.0.0-beta.2.md`)
- Description: CS0436 is cited for a compilation that references two hosts with public
  manifests. A library that references two such assemblies gets CS0433. CS0436 applies
  only when a source type conflicts with an imported type.
- Suggestion: Cite CS0433 (or both codes) in both files.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. A single reviewer.

## Verified without findings

- `source/timewarp-mediator/unit.cs`: no static field of type `ValueTask<Unit>`. `Instance` is
  a static field of type `Unit`, which is not the reported cycle.
- `tests/timewarp-mediator-generators-tests/unit-value-task-tests.cs` and the Nuru-style
  handler compile and pass. `./bin/dev test --quiet` succeeded in this worktree.
- `<Version>` 15.0.0-beta.2, install lines, skill pin, release notes, and the 15.0.0 notes
  cross-link are consistent.
- The goal restatement matches the task. No misread.
