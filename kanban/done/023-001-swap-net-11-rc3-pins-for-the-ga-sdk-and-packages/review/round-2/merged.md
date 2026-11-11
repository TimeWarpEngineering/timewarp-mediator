# Round 2 — merged findings
**Date:** 2026-11-11
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 2 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: fixed
- File: documentation/release-notes/15.0.0-beta.7.md:7
- Description: Paragraph 3 in round 1 claimed the shipped `net11.0` asset of `TimeWarp.Mediator` depends on `Microsoft.Extensions.DependencyInjection` `11.0.0` (instead of `11.0.0-rc.3.26522.148`) and the `net10.0` asset stays on `10.0.14`, whereas `TimeWarp.Mediator` has zero NuGet package dependencies and those `Directory.Packages.props` pins serve this repository's test, benchmark, sample, and smoke hosts.
- Suggestion: Rewrite `documentation/release-notes/15.0.0-beta.7.md:7` so it states `TimeWarp.Mediator` has no NuGet dependencies, its `lib/net11.0` asset is compiled against the GA `net11.0` targeting pack, and the `Directory.Packages.props` DependencyInjection and Logging pins (`11.0.0` on `net11.0`, `10.0.14` on `net10.0`) serve the repository's test, benchmark, sample, and smoke hosts.
- Source: general
- Disposition notes: Verified fixed in round 2 (`6598517`). `documentation/release-notes/15.0.0-beta.7.md:7` accurately describes the zero-dependency `TimeWarp.Mediator` package, its GA `lib/net11.0` compilation, and the `Directory.Packages.props` host/test pins, matching `TimeWarp.Mediator.nuspec` and `Directory.Packages.props`.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/readme.md:8 (also `source/timewarp-mediator-generators/readme.md:8-9`, `source/timewarp-mediator-analyzers/readme.md:20`, `readme.md:33-34,133`, and `skills/tw-mediator/SKILL.md:43-44,58`)
- Description: `<Version>` moved to `15.0.0-beta.7` in round 1 while the packed package readmes, root `readme.md`, `source/timewarp-mediator-analyzers/readme.md`, and `skills/tw-mediator/SKILL.md` still pinned `15.0.0-beta.6`, and `readme.md` omitted the link to `documentation/release-notes/15.0.0-beta.7.md`.
- Suggestion: Update those install/pin lines to `15.0.0-beta.7` across all five files, add `[15.0.0-beta.7](documentation/release-notes/15.0.0-beta.7.md),` to `readme.md:133`, and rebuild the `.nupkg` files so the packed readmes match `<Version>`.
- Source: general
- Disposition notes: Verified fixed in round 2 (`6598517`). All five files name `15.0.0-beta.7`, `readme.md:133` links `documentation/release-notes/15.0.0-beta.7.md`, and both rebuilt `15.0.0-beta.7` `.nupkg` archives in `artifacts/packages/` carry `readme.md` entries referencing `--version 15.0.0-beta.7`.

## Duplicates / conflicts

- None. Single general reviewer; no new findings in round 2.
