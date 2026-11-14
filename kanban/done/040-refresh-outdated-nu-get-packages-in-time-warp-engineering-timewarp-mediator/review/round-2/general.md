# Round 2 — general
**Date:** 2026-11-14
**Scope reviewed:** delta `d6a7f9e..f68a938` (round-1 base through HEAD). Product files: `.timewarp/suppression-baseline.json` and `tools/dev-cli/dev.cs`. Recorded gates in `## Results` and `### How to validate`, plus the fix-pass oracle log. Gates were not re-run.

## Summary

The fix removes the dev CLI's TWA0038 silence. `tools/dev-cli/dev.cs` no longer sets `#:property NoWarn=$(NoWarn);TWA0038`, and the comment that named the two Nuru.DevCli content files is gone with it. `.timewarp/suppression-baseline.json` no longer has a `tools/dev-cli/dev.cs` entry; the file still parses as one `files` object, and the `tools/dev-cli/Directory.Build.props` suppressions are unchanged. No `TWA0038` string remains under `tools/dev-cli/`. The recorded `self-install`, build (0 warnings), 382 tests, and audit (including `source-file-line-cap`) cover this edit. M1 is fixed. No new issues.

## Resolved prior

### M1 — Severity: bug — Status: fixed
- File: tools/dev-cli/dev.cs:2
- Description: Round 1 found `#:property NoWarn=$(NoWarn);TWA0038` at `tools/dev-cli/dev.cs:3-7` and `TWA0038: 1` for that path in `.timewarp/suppression-baseline.json`, after `TimeWarp.Nuru.DevCli` `4.0.0-beta.13` left no content file at the 500-line cap. Both are removed in `f68a938`. The runfile still sets `TargetFramework=net11.0` and still builds the Nuru app. The baseline keeps the other dev CLI `NoWarn` codes on `tools/dev-cli/Directory.Build.props` only.
- Suggestion: none remaining.
- Status: fixed

## Issues

<!-- No new issues. -->
