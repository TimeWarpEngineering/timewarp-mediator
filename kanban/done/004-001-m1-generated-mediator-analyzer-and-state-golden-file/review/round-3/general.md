# Round 3 — general
**Date:** 2026-09-01
**Scope reviewed:** commit `c8c7caa` (pack-fix) vs `37ae9ee` — Analyzers/Generators `IncludeSymbols=false`, `Build.ps1` nupkg DLL asserts, `Agent.md` pack notes; re-verify M1–M4

## Summary

PR #53 failed because repo-wide `IncludeSymbols`/`snupkg` plus `IncludeBuildOutput=false` produced a valid Analyzers nupkg then NU5017 on the empty snupkg. The fix disables symbols on Analyzers and Generators, keeps Mediator/Contracts snupkg, and has `Build.ps1` assert the analyzer DLL entries. Local Artifacts and MSBuild `-getProperty` confirm the override; CI still runs `./Build.ps1` on `src/**` PRs. M1–M4 remain fixed. No new issues.

## Issues

<!-- none — pack-fix verified; M1–M4 still fixed; NU5017 path closed -->
