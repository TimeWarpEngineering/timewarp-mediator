# Round 1 — merged findings
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
- Description: Paragraph 3 claims the shipped `net11.0` asset of `TimeWarp.Mediator` depends on `Microsoft.Extensions.DependencyInjection` `11.0.0` (instead of `11.0.0-rc.3.26522.148`) and the `net10.0` asset stays on `10.0.14`, contrasting them with "The test-only `Microsoft.Extensions.Logging` pin". In fact, `source/timewarp-mediator/timewarp-mediator.csproj` has zero NuGet package dependencies (`TimeWarp.Mediator.nuspec` has empty `<group targetFramework="net10.0" />` and `<group targetFramework="net11.0" />` groups). Both `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` in `Directory.Packages.props` are central pins for this repository's test, benchmark, sample, and smoke hosts (`agent.md:89`). What changed in the shipped `lib/net11.0` asset is compilation on the .NET 11 GA SDK (`11.0.100`) against the GA `net11.0` targeting pack.
- Suggestion: Rewrite `documentation/release-notes/15.0.0-beta.7.md:7` in the style of `15.0.0-beta.6.md:9` so it describes the `Directory.Packages.props` host/test pins (`Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` at `11.0.0` on `net11.0` and `10.0.14` on `net10.0`) rather than claiming the shipped `TimeWarp.Mediator` assets depend on `Microsoft.Extensions.DependencyInjection`.
- Source: general
- Disposition notes: Fixed in the fix pass. Paragraph 3 of `documentation/release-notes/15.0.0-beta.7.md` now says `TimeWarp.Mediator` has no NuGet dependencies, that its `lib/net11.0` asset is compiled by the GA SDK against the GA targeting pack, and that the `Directory.Packages.props` DependencyInjection and Logging pins (`11.0.0` on `net11.0`, `10.0.14` on `net10.0`) serve the repository's test, benchmark, sample, and smoke hosts.

### M2 — Severity: bug — Status: fixed
- File: source/timewarp-mediator/readme.md:8 (also `source/timewarp-mediator-generators/readme.md:8-9`, `source/timewarp-mediator-analyzers/readme.md:20`, `readme.md:33-34,133`, and `skills/tw-mediator/SKILL.md:43-44,58`)
- Description: `<Version>` moved to `15.0.0-beta.7`, but the packed nuget.org readmes (`source/timewarp-mediator/readme.md:8`, `source/timewarp-mediator-generators/readme.md:8-9`), `source/timewarp-mediator-analyzers/readme.md:20`, the root `readme.md:33-34` (which explicitly states at line 38 "keep it equal to `<Version>` until then"), and `skills/tw-mediator/SKILL.md:43-44,58` ("Use the version this repo is packing, `15.0.0-beta.6`") all still name `15.0.0-beta.6`, and `readme.md:132-135` omits the link to `documentation/release-notes/15.0.0-beta.7.md`. Both packed `15.0.0-beta.7` `.nupkg` files currently ship a `readme.md` telling users to install `--version 15.0.0-beta.6`.
- Suggestion: Update those install/pin lines to `15.0.0-beta.7` across all five files and add `[15.0.0-beta.7](documentation/release-notes/15.0.0-beta.7.md),` to `readme.md:133` (matching prior bump commit `0627399a`), leaving `kanban/` and older release notes untouched. Re-run `./bin/dev pack` (or `./bin/dev workflow`) to refresh and verify the packed `.nupkg` readmes.
- Source: general
- Disposition notes: Fixed in the fix pass. The install and pin lines in all five files name `15.0.0-beta.7`, and `readme.md` links `documentation/release-notes/15.0.0-beta.7.md`. `./bin/dev pack` rebuilt both `.nupkg` files, and the packed `readme.md` in each names `--version 15.0.0-beta.7`.

## Duplicates / conflicts

- None. Single general reviewer.
