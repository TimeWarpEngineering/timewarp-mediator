# Round 2 — general
**Date:** 2026-09-02
**Scope reviewed:** commit ee2dc97 vs 2845702 (M1 Version SSOT assert + M2 wipe pack output)

## Summary

M1 is fixed: `AssertVersionSsot` runs in release after pack and before shared `check-version`/push; `RepoLayout.TryReadVersion` matches TimeWarp.Nuru.DevCli `CheckVersionCommand.GetVersionFromSource` (MSBuild xmlns `http://schemas.microsoft.com/developer/msbuild/2003` then bare `Version`); mismatch or missing sets `ExitCode` 1 and aborts; root and `source/Directory.Build.props` both read `13.0.0`. M2 is fixed: `PackAsync` recursively deletes `artifacts/packages` when present, then recreates it, so `FindNupkg` and push cannot see leftover nupkgs from a prior run. No new defects on the fix delta.

## Issues
