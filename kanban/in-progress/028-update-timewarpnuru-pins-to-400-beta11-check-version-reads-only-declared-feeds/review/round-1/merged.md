# Round 1 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: documentation/release-notes/15.0.0-beta.4.md:3
- Description: The opening paragraph says "the shipped packages are unchanged since
  15.0.0-beta.2". That is false. Since `v15.0.0-beta.2`, the shipped generator and analyzer
  dropped `VersionOverride="5.0.0"` and now reference `Microsoft.CodeAnalysis.CSharp` 5.9.0. On
  an SDK whose compiler is older than that, they report CS9057 and the generator emits nothing.
  The same note documents this further down, and the F14 row in `15.0.0.md` links here for it.
  The task addendum supplied the sentence.
- Suggestion: Replace the clause with one saying beta.4 ships what beta.3 would have shipped and
  that only the dev tooling changed (DevCli 4.0.0-beta.11, `check-version` reads only nuget.org).
  Keep the existing "dispatcher is unchanged since 15.0.0-beta.2 … no runtime change" paragraph.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None (single reviewer).
