# Round 2 — general
**Date:** 2026-10-28
**Scope reviewed:** fix delta `e1cdd00..0448d20` (round-1 `base.sha` to head). Product change: six csproj files drop `<RollForward>LatestMajor</RollForward>`. Kitchen change: task.md Results "Fix pass (review round 1)". The round-1 M1 and M2 IDs were re-verified against that delta.

## Summary

The fix pass removes `RollForward=LatestMajor` from the six hosts that multi-target `net10.0;net11.0`. No other `RollForward` is left in any csproj, props, or json file. The docs never mentioned the property. CI still installs `10.0.x`, so a net10.0 test leg has its runtime without rolling forward. Results include a `COREHOST_TRACE` that chose `10.0.14` for net10.0 and `11.0.0-rc.3` for net11.0, plus build and test counts on both frameworks. That closes M1.

M2 asked for a recorded gate run, and Results now has one. The gate failed: `TimeWarpGeneratedMonomorphicSend` measured 10.509 ns against a 7.524 ns limit, and `TimeWarpGeneratedMonomorphicSendUnit` measured 8.063 ns against 5.828 ns. M2 is closed, but the result raises a new issue (Issue 1).

The restatement still matches the original task, and the fix-pass addendum matches round-1 M1 and M2. No misread.

## Issues

### Issue 1 — Severity: suggestion
- File: Directory.Packages.props:54
- Description: This branch leaves `./bin/dev benchmark --gate` red, and nothing on the board owns turning it green again. The gate measures net10.0 with BenchmarkDotNet `0.16.0-preview.2` against a baseline that `0.15.8` recorded, and the two monomorphic rows came out about 2x the baseline. The net11.0 run gets about the same numbers (9.919 ns and 8.091 ns). So the cause could be the measurement tool or a real regression, and nobody has checked which. Results says 023-001 "still owns the move off `0.16.0-preview.2`". But 023-001 moves to `0.16.0` stable, and that stays a different tool from the one that recorded the baseline. The gate stays red after 023-001 unless someone re-records the baseline or pins the net10.0 leg back to the old version. Neither is planned. `--gate` is the documented perf-regression check for the 15.0 hot path (`agent.md` benchmark row), so after merge it can no longer tell a regression apart from noise.
- Suggestion: Condition the BenchmarkDotNet pin on `$(TargetFramework)` the same way the DI pin is: `0.15.8` when it is `net10.0` and `0.16.0-preview.2` when it is `net11.0`. Then re-run `./bin/dev benchmark --gate`, record pass or fail in Results, and update the `agent.md` benchmark row and the props comment. If the gate passes, the preview tool caused the failure. If it still fails on `0.15.8`, the regression is real: say so in Results and give it a task. If the per-TFM pin does not restore or run, record why in Results. Then give 023-001 (or a new to-do) an explicit item to get the gate green again, either by re-recording `comparison-baseline.md` with Steven's approval or by keeping the 0.15.8 pin.
- Status: open
