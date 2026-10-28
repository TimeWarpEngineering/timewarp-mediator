# Round 1 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: suggestion — Status: open
- File: tests/timewarp-mediator-generators-tests/timewarp-mediator-generators-tests.csproj:5 (and the compilation-tests, analyzers-tests, benchmarks-comparison, named-pipelines and aot csproj files)
- Description: `RollForward=LatestMajor` on the multi-targeted projects sends every net10.0 leg
  (tests, `dotnet run -f net10.0` samples, the net10.0 benchmark gate host) to the 11 runtime once
  11.0.0 GA is installed. The pipeline stays green, but nothing runs on the 10 runtime any more.
  Today the net10.0 leg runs on 10.0.14, because the host does not roll a release request onto a
  prerelease.
- Suggestion: Drop `<RollForward>LatestMajor</RollForward>` from those six projects, or condition
  it off `net10.0`. Say in Results that a net10.0 leg runs on the 10 runtime.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: Directory.Packages.props:54
- Description: The net10.0 `--gate` now measures with BenchmarkDotNet `0.16.0-preview.2` against
  a baseline recorded by `0.15.8`. Results do not show a gate run after the bump.
- Suggestion: Run `./bin/dev benchmark --gate` once and record pass or fail in Results, or say in
  Results that 023-001 owns that check.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. One reviewer.
