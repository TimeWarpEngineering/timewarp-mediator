# Round 2 — merged findings
**Date:** 2026-10-28
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 1 | 1 | 0 |
| nit | 0 | 1 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: the six multi-targeted host csproj files (generator tests, compilation tests, analyzer tests, comparison benchmark, named-pipelines, aot)
- Description: `RollForward=LatestMajor` would move the net10.0 legs onto the 11 runtime once 11.0.0 GA is installed.
- Source: general (round 1)
- Disposition notes: Fixed in 0448d20. All six `<RollForward>LatestMajor</RollForward>` lines are gone, and no other `RollForward` is left in any csproj, props, or json file. Results show a `COREHOST_TRACE` that chose `10.0.14` for the net10.0 leg and `11.0.0-rc.3` for net11.0, with tests 57/42/69 passing on both frameworks.

### M2 — Severity: nit — Status: fixed
- File: Directory.Packages.props:54
- Description: No gate run on BenchmarkDotNet `0.16.0-preview.2` was recorded.
- Source: general (round 1)
- Disposition notes: Fixed. Results record the gate run: it failed (exit 1) on the two monomorphic rows, and the baseline file is unchanged. Round-2 M3 follows up on what that result means.

### M3 — Severity: suggestion — Status: open
- File: Directory.Packages.props:54
- Description: The net10.0 `--gate` is red on BenchmarkDotNet `0.16.0-preview.2` against the `0.15.8` baseline. Monomorphic Send is about 2x the baseline, and nobody has checked whether the tool or the code causes it. 023-001 moves to `0.16.0` stable, which still is not the tool that recorded the baseline, so no task owns getting the gate green again.
- Suggestion: Pin BenchmarkDotNet per TFM (`0.15.8` on `net10.0`, `0.16.0-preview.2` on `net11.0`), re-run `--gate`, and record the result. Update the `agent.md` benchmark row and the props comment. If the gate still fails on `0.15.8`, record it as a real regression and give it a task. If the per-TFM pin does not work, record why. Either way, give a task an explicit item to get the gate green again.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. One reviewer.
