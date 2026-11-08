# Bump version to 15.0.0-beta.5 and release

## Description

Release TimeWarp.Mediator and TimeWarp.Mediator.Generators 15.0.0-beta.5, which ships
the `GeneratedHandlerAttribute` and TWM012 from task 029 (PR #123).

The bump itself was not needed: task 029 already set `<Version>` to 15.0.0-beta.5 and wrote
`documentation/release-notes/15.0.0-beta.5.md`. This task is the release trail only.

## Checklist

- [x] PR #123 (task 029) merged to master, CI green
- [x] `./bin/dev release --dry-run` guards all pass
- [x] `./bin/dev release` tagged v15.0.0-beta.5 at 19e4071 and created the GitHub Release
- [x] `release:published` run 42097280764 pushed both packages to nuget.org

- 2026-11-08: audit --fix untracked .vscode/settings.json and ignored it; MediatR benchmark pin 14.2.0 → 14.3.0 (Steven, same task)

## Session

- Created: 6a80da6d-bb0c-4bb4-b006-83f4e5ea795c (2026-11-05)
- 2026-11-05: merged PR #123, cut v15.0.0-beta.5 from synced master

## Notes

Release: https://github.com/TimeWarpEngineering/timewarp-mediator/releases/tag/v15.0.0-beta.5
