# Round 4 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 2 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: the six multi-targeted host csproj files
- Description: `RollForward=LatestMajor` would move the net10.0 legs onto the 11 runtime after 11.0.0 GA.
- Source: general (round 1)
- Disposition notes: Fixed in 0448d20 and verified in round 2. The round-4 delta does not touch it.

### M2 — Severity: nit — Status: fixed
- File: Directory.Packages.props:54
- Description: No gate run on BenchmarkDotNet `0.16.0-preview.2` was recorded.
- Source: general (round 1)
- Disposition notes: Fixed. Results record the failed preview gate run. The round-4 delta does not touch it.

### M3 — Severity: suggestion — Status: fixed
- File: Directory.Packages.props:53-58
- Description: The net10.0 `--gate` was red on BenchmarkDotNet `0.16.0-preview.2` against the `0.15.8` baseline, and no task owned getting it green.
- Source: general (round 2)
- Disposition notes: Fixed in ee2cc5b. BenchmarkDotNet is pinned per framework (`0.15.8` on net10.0, `0.16.0-preview.2` on net11.0), verified with `dotnet list package`. Results record `--gate` passing on `0.15.8` (exit 0, baseline unchanged). The props comment and the `agent.md` row match. Task 023-001 carries the gate item (`d544794` on `origin/master`).

## Duplicates / conflicts

- None. One reviewer, no new findings.
