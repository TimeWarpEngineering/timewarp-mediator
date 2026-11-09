# Round 2 — merged findings
**Date:** 2026-11-09
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 1 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: Directory.Packages.props:26-27
- Description (round 1): `TimeWarp.Nuru.DevCli` was pinned at `4.0.0-beta.11` after `4.0.0-beta.12` was published.
- Source: general (round 1)
- Disposition notes: Fixed in `8d6fdef`. `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are both `4.0.0-beta.12`. The dev CLI moves to `net11.0`, and its handlers use `IRequestHandler`/`ValueTask<Unit>`. Re-verified: nuget.org lists beta.12 as the latest, `ganda nuget outdated --no-cache` is clean, and the dev CLI builds.

### M2 — Severity: nit — Status: fixed
- File: documentation/release-notes/15.0.0-beta.6.md:5
- Description (round 1): "that reference" and "the analyzer" were ambiguous.
- Source: general (round 1)
- Disposition notes: Fixed in `8d6fdef`. The sentence names the `TimeWarp.SourceGenerators` reference and "its generators".

## New findings

None.

## Duplicates / conflicts

- None. There was a single reviewer.
