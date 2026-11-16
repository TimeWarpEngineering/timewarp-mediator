# Update outdated NuGet pins (coverlet.collector 10.1.1)

## Description

Bring every central pin in `Directory.Packages.props` up to what `ganda nuget outdated` reports as latest. On 2026-11-17 the only outdated pin is `coverlet.collector` 10.1.0 → 10.1.1 (patch, test-only, not shipped in either package). Re-run `ganda nuget outdated` at start and take anything else that has appeared since.

Dependabot PR #134 makes the same coverlet bump. After this task merges, close #134 as superseded (or, if #134 merged first, take only what is still outdated).

## Requirements

- `ganda nuget outdated` reports no outdated packages on the branch (packages with non-literal versions, `$(TimeWarpMediatorVersion)`, are skipped by the tool and stay as they are).
- No `<Version>` bump unless a bump changes what ships in `TimeWarp.Mediator` or `TimeWarp.Mediator.Generators`; a test-only or dev-CLI-only pin change does not.
- If a dev CLI pin moves, rebuild with `./bin/dev self-install`.
- Gates: `./bin/dev build`, `./bin/dev test`, `./bin/dev verify-samples`, `./bin/dev pack`, `ganda repo audit` clean.

## Checklist

- [ ] Re-run `ganda nuget outdated` and record the list
- [ ] Update the pins in `Directory.Packages.props`
- [ ] Build, test, verify-samples, pack, audit all green
- [ ] Close Dependabot #134 as superseded after merge

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-17)

## Notes

- Requested by Steven 2026-11-17: "just create task to update `ganda nuget outdated`". Filed only; not launched.
