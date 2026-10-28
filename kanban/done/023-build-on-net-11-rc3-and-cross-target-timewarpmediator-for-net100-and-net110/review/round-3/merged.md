# Round 3 — merged findings
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
- File: the six multi-targeted host csproj files
- Description: `RollForward=LatestMajor` would move the net10.0 legs onto the 11 runtime after 11.0.0 GA.
- Source: general (round 1)
- Disposition notes: Fixed in 0448d20 and verified in round 2. The round-3 delta does not touch it.

### M2 — Severity: nit — Status: fixed
- File: Directory.Packages.props:54
- Description: No gate run on BenchmarkDotNet `0.16.0-preview.2` was recorded.
- Source: general (round 1)
- Disposition notes: Fixed. Results record the failed gate run. The round-3 delta does not touch it.

### M3 — Severity: suggestion — Status: open
- File: Directory.Packages.props:54
- Description: The net10.0 `--gate` is red on BenchmarkDotNet `0.16.0-preview.2` against the `0.15.8` baseline, and nobody has checked whether the tool or the code causes it. No task has an item to get the gate green again.
- Suggestion: Pin BenchmarkDotNet per TFM (`0.15.8` on `net10.0`, `0.16.0-preview.2` on `net11.0`), re-run `--gate`, record the result, and update the `agent.md` benchmark row and the props comment. If it still fails on `0.15.8`, record the regression and give it a task. If the per-TFM pin does not work, record why. Either way, give a task an explicit item to get the gate green again. If Steven declines, mark this `wontfix` with his rationale.
- Source: general (round 2)
- Disposition notes: Round 3 found no fix. The delta `0448d20..4563d15` is kitchen-only, the props pin is unchanged, and task 023-001 has no gate item.

## Duplicates / conflicts

- None. One reviewer.
