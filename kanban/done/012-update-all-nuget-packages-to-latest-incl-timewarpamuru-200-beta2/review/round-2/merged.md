# Round 2 — merged findings
**Date:** 2026-10-09
**Sources:** general (orchestrator re-verification of the fix delta)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 1 |
| nit | 0 | 1 | 0 |

## Resolved prior

- M1 fixed: both shipped components resolve Microsoft.CodeAnalysis.CSharp 4.8.0 (project.assets.json). Release build exits 0. Tests: 34 + 6 + 7 + 165 pass, 2 are skipped, 0 fail. Pack layout is verified.
- M2 wontfix: carried with rationale from round 1.
- M3 fixed: no literal versions remain in the csproj comments.

## New issues

- None on the fix delta.
