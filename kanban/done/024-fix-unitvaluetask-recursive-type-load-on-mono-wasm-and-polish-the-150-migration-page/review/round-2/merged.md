# Round 2 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/migration.md:159
- Description: The `IncrementHandler` sample returned `Unit.ValueTask` from a non-generic `ValueTask Handle`. That was CS0029.
- Disposition notes: Fixed in `cd587ef`. The sample returns `ValueTask.CompletedTask`. `tests/timewarp-mediator-generators-tests/increment-handler.cs` compiles the same shape, and `ActionHandler_ReturnsCompletedValueTask` runs it. Tests pass.

### M2 — Severity: nit — Status: fixed
- File: documentation/migration.md:268
- Description: The Nuru bullet named the internal C# constants rather than the JSON values.
- Disposition notes: Fixed in `cd587ef`. The bullet lists the `kind` strings `"Query"`, `"IdempotentCommand"`, `"Command"`, `"Action"`, and `"Request"`. These match `manifest-emitter.cs:33-37,64`.

### M3 — Severity: nit — Status: fixed
- File: documentation/migration.md:193, documentation/release-notes/15.0.0-beta.2.md:19
- Description: CS0436 was cited where the error is CS0433.
- Disposition notes: Fixed in `cd587ef`. Both files cite CS0433 for a library that references two hosts, and name CS0436 as the conflict between a source type and an imported type.

## New findings

None.

## Duplicates / conflicts

- None. This round had a single reviewer.
