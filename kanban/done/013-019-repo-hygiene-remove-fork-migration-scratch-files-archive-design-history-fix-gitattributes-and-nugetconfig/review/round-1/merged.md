# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: agent.md:94
- Description: The new snk note says the test tree is signed with `timewarp-mediator.snk` so `InternalsVisibleTo` works. `tests/Directory.Build.props` turns signing off. Only `timewarp-mediator-tests` signs with that key. The four-package signing split in the same bullet is accurate.
- Suggestion: Name `timewarp-mediator-tests` as the signed test assembly and point at the test-tree opt-out.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One reviewer. Nothing to collapse.
