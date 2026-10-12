# Round 2 — general
**Date:** 2026-10-12
**Scope reviewed:** M1 fix in `agent.md` Build Properties, plus a scan of that bullet against `Directory.Build.props`, `tests/Directory.Build.props`, `tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj`, and the Analyzers/Generators `SignAssembly` overrides.

## Summary

M1 is fixed. The snk bullet now says the test tree turns signing off and that only `timewarp-mediator-tests` signs with `timewarp-mediator.snk` so `TimeWarp.Mediator` can grant `InternalsVisibleTo`. That matches `tests/Directory.Build.props` (`SignAssembly` false) and the single test csproj that sets `SignAssembly` true. The four-package split is unchanged and still matches the csproj files. The fix delta is that one sentence. No new defects.

## Issues

<!-- No new issues. -->
