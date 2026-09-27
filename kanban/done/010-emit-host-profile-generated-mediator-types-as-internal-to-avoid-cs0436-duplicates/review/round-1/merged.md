# Round 1 — merged findings
**Date:** 2026-09-26
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: nit — Status: fixed
- File: readme.md:155, documentation/generated-vs-legacy.md:38
- Description: Docs claim no duplicate generated types for a project referencing two hosts, but
  `InternalsVisibleTo` from both hosts re-exposes the internal types.
- Suggestion: Add an `InternalsVisibleTo` caveat to the readme paragraph and the beta.4 changelog.
- Source: general
- Disposition notes: fixed in the review commit. The readme sentence now ends "unless both hosts grant it
  `InternalsVisibleTo`", and the beta.4 changelog bullet adds that `InternalsVisibleTo` re-exposes the types.
  Docs-only; re-verified in round 2.

## Duplicates / conflicts

- None (single reviewer).
