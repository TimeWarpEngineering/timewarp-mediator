# Round 2 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: agent.md:94
- Description: The snk note said the test tree is signed. It now says `tests/Directory.Build.props` turns signing off and only `timewarp-mediator-tests` signs with this key so `TimeWarp.Mediator` can grant `InternalsVisibleTo`.
- Suggestion: Done in round 2.
- Source: general
- Disposition notes: Re-checked against `tests/Directory.Build.props` and `tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj`. No other test csproj sets `SignAssembly`.

## Duplicates / conflicts

- No new findings. M1 carried forward from round 1.
