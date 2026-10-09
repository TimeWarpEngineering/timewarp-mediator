# Update all NuGet packages to latest (incl. TimeWarp.Amuru 2.0.0-beta.2)

## Description
Steven wants every repo on the newest packages, pre-releases included (goal is latest, not stable). Run `ganda nuget outdated --update` in this repo and take every package to the newest version it can, not just Amuru. TimeWarp.Amuru and TimeWarp.Amuru.Tools must end on 2.0.0-beta.2 (or newer).

**Current:** TimeWarp.Amuru 1.0.0, TimeWarp.Amuru.Tools 1.0.0-beta.2.

## Checklist
- [ ] `ganda nuget outdated --dry-run`, then `ganda nuget outdated --update` (Directory.Packages.props)
- [ ] Also bump `#:package ...@version` pins in runfiles (scripts, .githooks are ganda-owned: refresh via `ganda hooks install attest` instead of editing)
- [ ] Fix Amuru 1.x -> 2.0 breaking changes (see timewarp-amuru documentation/release-notes/2.0.0.md): Git.*Master* helpers removed (use *Default*), Git methods return result objects instead of bool, no "master" default branchName, removed/renamed dotnet builder options, WithStandardInput("") now closes stdin
- [ ] Fix any other breaking changes from other bumped packages
- [ ] Build warning-free, tests green, `ganda repo audit` clean
- [ ] One PR, merge via `ganda pr merge`

## Notes
Filed 2026-10-09 at Steven's request (Amuru 2.0 sweep across live repos). If a package can't move (e.g. a dependency cycle such as Terminal <-> Amuru), record why here instead of forcing it.

## Session

- Created: 821248 (2026-10-09)
