# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commit `7e7353a` vs `1843dd2` (product files listed in `review/review-framework.md`)

## Summary

The commit does the hygiene the task asked for. The four scratch files are gone, the live spec is under `documentation/design/`, superseded analyses and the collaboration thread are under `documentation/archive/` via rename, m1/m2 and `.agents/collaboration/readme.md` point at the new paths, and 29 relative links in those trees resolve. `.gitattributes` drops `core.eol` and every `merge=union` line and marks `*.snk` binary. `nuget.config` has no BOM, a `<clear />`, and a `*` mapping to nuget.org. The build-command comment no longer claims a `.sln`. The snk note names the right two signed packages, then overclaims that the test tree is signed.

## Issues

### Issue 1 — Severity: suggestion
- File: agent.md:94
- Description: The new snk note says "Tests are signed with the same key so `InternalsVisibleTo` works." `tests/Directory.Build.props` sets `SignAssembly` to false for the test tree. The only test project that opts back in is `tests/timewarp-mediator-tests/timewarp-mediator-tests.csproj`, and its comment says that is so `TimeWarp.Mediator` can grant `InternalsVisibleTo`. The four-package split in the same bullet (Mediator and Contracts signed; Analyzers and Generators `SignAssembly=false`) matches the csproj files.
- Suggestion: Name `timewarp-mediator-tests` as the signed test assembly and point at the test-tree opt-out.
- Status: open
