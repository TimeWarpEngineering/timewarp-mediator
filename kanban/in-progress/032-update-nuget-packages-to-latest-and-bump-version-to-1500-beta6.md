# Update NuGet packages to latest and bump version to 15.0.0-beta.6

## Description

Bring the central package pins up to their latest versions and bump `<Version>` from `15.0.0-beta.5` to `15.0.0-beta.6`, so the next prerelease ships on current dependencies. After merge, Steven's session cuts the release with `./bin/dev release` (tw-release). This task does not tag or publish.

## Requirements

- Update every pin in `Directory.Packages.props` (and any `#:package` pin in `tools/dev-cli/dev.cs`) to the latest version on nuget.org or the TimeWarp feed (https://nuget.pkg.github.com/TimeWarpEngineering/index.json). Keep each pin on its current channel: a stable pin moves to the latest stable, a prerelease pin (TimeWarp betas) moves to the latest prerelease.
- Run `ganda repo audit --fix` and commit what it changes.
- Out of scope, owned by task 023-001: the `global.json` SDK pin, the `net11.0` `Microsoft.Extensions.DependencyInjection` RC pin, the CI `dotnet-quality: preview` line, and both BenchmarkDotNet pins (`0.15.8` on `net10.0`, `0.16.0-preview.2` on `net11.0`). Leave them alone.
- The `net10.0` `Microsoft.Extensions.DependencyInjection` pin may move to the latest `10.0.x` patch. A net10.0 host must not restore an 11 package.
- `Microsoft.CodeAnalysis.CSharp` must stay at or below the Roslyn version of the pinned SDK compiler. `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` must stay green.
- `source/Directory.Build.props`: `<Version>` becomes `15.0.0-beta.6`.
- Add `documentation/release-notes/15.0.0-beta.6.md` in the style of `15.0.0-beta.5.md`, listing the dependency bumps.
- `./bin/dev workflow` is green, and `./bin/dev check-version --strict` passes.

## Checklist

- [ ] Package pins updated (023-001 pins untouched)
- [ ] `ganda repo audit --fix` applied and committed
- [ ] `<Version>` is `15.0.0-beta.6`
- [ ] Release notes `15.0.0-beta.6.md` added
- [ ] `./bin/dev workflow` green
- [ ] `./bin/dev check-version --strict` passes
- [ ] PR opened

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-09)

## Notes

- beta.5 was tagged `v15.0.0-beta.5` (task 029). beta.3 is a dead tag; do not reuse it.
- Release after merge goes through tw-release from a clean synced master.
