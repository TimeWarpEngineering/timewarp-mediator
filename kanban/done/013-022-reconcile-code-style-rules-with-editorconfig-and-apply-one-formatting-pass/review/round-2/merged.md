# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: tests/timewarp-mediator-tests/publish-tests.cs:52
- Description: Whitespace format left lambda bodies at the brace column in 13 files.
- Suggestion: Re-run `dotnet format whitespace --folder --include source/ tests/ samples/`.
- Source: general
- Disposition notes: Applied. `--verify-no-changes` exits 0.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/registration/service-registrar.cs:321
- Description: New IDE1006 rules were already violated by camelCase `logger` fields, PascalCase deconstruction locals, and PascalCase method-local consts.
- Suggestion: Rename fields to `Blogger` and those locals to camelCase.
- Source: general
- Disposition notes: Applied. `dotnet format style --diagnostics IDE1006 --verify-no-changes` exits 0.

## Resolved prior

- M1 fixed
- M2 fixed

## Duplicates / conflicts

- Single reviewer. No new IDs.
