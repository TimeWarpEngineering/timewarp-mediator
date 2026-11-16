# Update outdated NuGet pins (coverlet.collector 10.1.1)

## Description

Bring every central pin in `Directory.Packages.props` up to what `ganda nuget outdated` reports as latest. On 2026-11-17 the only outdated pin is `coverlet.collector` 10.1.0 → 10.1.1 (patch, test-only, not shipped in either package). Re-run `ganda nuget outdated` at start and take anything else that has appeared since.

Dependabot PR #134 makes the same coverlet bump. After this task merges, close #134 as superseded (or, if #134 merged first, take only what is still outdated).

## Goal (implementer restatement)

Bring every literal central pin in `Directory.Packages.props` to the latest version `ganda nuget outdated` reports, with no package `<Version>` bump because nothing shipped changes.

Acceptance:

- `ganda nuget outdated` on the branch reports 0 outdated packages. The `$(TimeWarpMediatorVersion)` entries are skipped and stay as they are.
- `source/Directory.Build.props` `<Version>` is unchanged.
- build, test, verify-samples, pack, and `ganda repo audit` all pass.
- Closing Dependabot #134 happens after merge. That step is outside this walk.

## Requirements

- `ganda nuget outdated` reports no outdated packages on the branch (packages with non-literal versions, `$(TimeWarpMediatorVersion)`, are skipped by the tool and stay as they are).
- No `<Version>` bump unless a bump changes what ships in `TimeWarp.Mediator` or `TimeWarp.Mediator.Generators`; a test-only or dev-CLI-only pin change does not.
- If a dev CLI pin moves, rebuild with `./bin/dev self-install`.
- Gates: `./bin/dev build`, `./bin/dev test`, `./bin/dev verify-samples`, `./bin/dev pack`, `ganda repo audit` clean.

## Checklist

- [x] Re-run `ganda nuget outdated` and record the list
- [x] Update the pins in `Directory.Packages.props`
- [x] Build, test, verify-samples, pack, audit all green
- [ ] Close Dependabot #134 as superseded after merge

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-17)
- Implemented 2026-11-17 by ganda task work (implement oracle, Claude Opus 5.5).

## Results

`ganda nuget outdated` at start (2026-11-17): 25 pins checked, 1 outdated: `coverlet.collector` 10.1.0 -> 10.1.1 (patch, dev-only). The two `$(TimeWarpMediatorVersion)` entries are skipped by the tool. Nothing else had appeared since the task was filed.

Change: `Directory.Packages.props` `coverlet.collector` 10.1.0 -> 10.1.1. No `<Version>` bump: coverlet is a test-only collector and is not in either shipped package. No dev CLI pin moved, so there was no `self-install`.

### Gates

| Gate | Result |
|---|---|
| `./bin/dev build --quiet` | pass |
| `./bin/dev test --no-build` | pass: generators-tests 61/61, analyzers-tests 50/50, compilation-tests 80/80 |
| `./bin/dev verify-samples --no-build` | pass (AOT and named-pipelines samples ran) |
| `./bin/dev pack --no-build` | pass: both 15.0.0-beta.9 nupkgs verified, smoke printed `package-pong` |
| `ganda nuget outdated` | "All packages are up to date!" |
| `ganda repo audit` | pass, with 1 advisory warning: `vscode-window-icon`, `.vscode/settings.json` missing |
| `ganda repo audit --fix` | pass, 44 passed / 0 failed. The fix wrote the gitignored, per-worktree `.vscode/settings.json`, so nothing was committed |

### How to validate

- Smoke: `ganda nuget outdated && ./bin/dev build && ./bin/dev test`
- Expect: `ganda nuget outdated` prints "All packages are up to date!", `Directory.Packages.props` pins `coverlet.collector` at `10.1.1`, and all 191 tests pass.

### Follow-up

- After merge, close Dependabot PR #134 as superseded. This is a post-merge step outside the walk.

## Review

skip
## Notes

- Requested by Steven 2026-11-17: "just create task to update `ganda nuget outdated`". Filed only; not launched.
