# Round 1 — merged findings
**Date:** 2026-09-25
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 1 |

## Issues

### M1 — Severity: nit — Status: wontfix
- File: Directory.Packages.props:13
- Description: SourceLink 10.0.401 pulls `System.IO.Hashing` 10.0.12 (build-time), which warns
  "doesn't support net6.0" when packing the `net6.0` target of `source/timewarp-mediator`.
- Suggestion: Suppress with `SuppressTfmSupportBuildWarnings`, or drop the `net6.0` target.
- Source: general
- Disposition notes: wontfix (review oracle). Build-time only; published nuspec dependency groups are
  unchanged and the warning is not fatal under `TreatWarningsAsErrors`. The SourceLink bump is required to
  clear the NU1902 advisory on `Microsoft.Build.Tasks.Git` 8.0.0. Suppressing TFM support warnings
  globally would hide real ones, and dropping `net6.0` is a TFM policy change outside this task's scope.

## Duplicates / conflicts

- None (single reviewer).
