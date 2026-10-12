# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: tests/timewarp-mediator-tests/publish-tests.cs:52
- Description: `dotnet format whitespace --folder --include source/ tests/ samples/ --verify-no-changes` is not clean. Thirteen files still have lambda bodies indented at the brace column. The formatter wants two more spaces on those lines.
- Suggestion: Re-run the documented whitespace command and keep the indentation-only result.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: .editorconfig:90
- Description: The naming rules added in this change already fail IDE1006. Six `logger` fields of type `IBlogger<T>` remain camelCase. `service-registrar.cs` deconstructs into PascalCase locals `Service` and `Implementation`. Method-local consts `Source` (12) and `Bindings` (1) are PascalCase. The product hits are errors under `TreatWarningsAsErrors` when style analysis runs.
- Suggestion: Rename those fields to `Blogger` and those locals and method consts to camelCase.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- Single reviewer. No overlaps.
