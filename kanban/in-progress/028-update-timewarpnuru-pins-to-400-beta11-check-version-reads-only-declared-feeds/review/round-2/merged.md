# Round 2 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/release-notes/15.0.0-beta.4.md:3
- Description (round 1): the opening paragraph said the shipped packages were unchanged since
  15.0.0-beta.2. That was false, because the Roslyn reference moved from 5.0.0 to 5.9.0.
- Source: general (round 1)
- Disposition notes: fixed in a1a07fc. The paragraph now says beta.4 ships what beta.3 would
  have shipped and that only the dev tooling changed. Round 2 checked this with
  `git diff v15.0.0-beta.3 HEAD -- source/ Directory.Packages.props`: only the Nuru pins and
  version strings changed.

No new findings in round 2.

## Duplicates / conflicts

- None (single reviewer).
