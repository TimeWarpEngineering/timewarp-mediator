# Update outdated NuGet pins before the beta.7 release

## Description

`ganda nuget outdated` on master (2026-11-12) reports 7 outdated pins. Update them before 15.0.0-beta.7 ships. The first beta.7 release run was cancelled before it pushed anything, and its tag and GitHub release were deleted, so beta.7 is cut again after this merges. `<Version>` is already `15.0.0-beta.7`. Leave it there.

## Requirements

### Pins in `Directory.Packages.props`

| Package | From | To |
|---|---|---|
| `Microsoft.SourceLink.GitHub` | 10.0.403 | 11.0.100 |
| `TimeWarp.Build.Tasks` | 1.0.0 | 2.0.0 (major, Steven 2026-11-12) |
| `TimeWarp.SourceGenerators` | 1.0.0-beta.12 | 1.0.0-beta.14 |
| `Microsoft.NET.Test.Sdk` | 18.11.0 | 18.12.0 |
| `coverlet.collector` | 6.0.4 | 10.1.0, or remove it (see below) |
| `Microsoft.Extensions.DependencyInjection`, `net10.0` condition | 10.0.14 | latest `10.0.x` |
| `Microsoft.Extensions.Logging`, `net10.0` condition | 10.0.14 | latest `10.0.x` |

- The `net10.0`-conditioned DependencyInjection and Logging pins stay on the 10.0 line. `ganda nuget outdated` suggests 11.0.0 for them. Ignore that: a net10.0 host must not restore an 11 package. The `net11.0` pins are already `11.0.0`.
- `TimeWarp.Build.Tasks` 2.0.0 is a major bump. Read its release notes or package diff (tw-dotnet-inspect) and adapt the build to any breaking change. Record what changed under `## Notes`.
- `coverlet.collector`: task 034 moved coverage to `dev test` (Cobertura into `artifacts/coverage`). If nothing uses coverlet anymore, remove the pin and the test-project references instead of bumping. If something still uses it, bump to 10.1.0.
- Do not touch BenchmarkDotNet (`0.15.8` on net10.0, `0.16.0-preview.2` on net11.0). 0.16.0 stable is not on NuGet.
- Check `#:package` pins in `tools/dev-cli/dev.cs` and `.githooks/*.cs` too.

### .NET 11 targeting

- Confirm every project targets .NET 11: the 9 multi-target projects are `net10.0;net11.0`, the dev CLI is `net11.0`, and `global.json` pins SDK `11.0.100`. The generator and analyzer stay `netstandard2.0` (Roslyn loads them). Do not drop `net10.0`.
- `./bin/dev pack` must still assert `lib/net10.0` and `lib/net11.0`.

### Gates

- `ganda nuget outdated` shows nothing outdated except the two `net10.0` 10.0.x pins (expected) and BenchmarkDotNet.
- `ganda repo audit` is clean.
- `./bin/dev workflow` is green.
- `./bin/dev check-version --strict` passes for 15.0.0-beta.7.
- Add the package bumps to `documentation/release-notes/15.0.0-beta.7.md`.

## Checklist

- [ ] SourceLink 11.0.100, Build.Tasks 2.0.0, SourceGenerators beta.14, Test.Sdk 18.12.0
- [ ] coverlet bumped or removed
- [ ] net10.0 DependencyInjection and Logging on latest 10.0.x
- [ ] Build.Tasks 2.0.0 breaking changes handled and recorded
- [ ] .NET 11 targeting confirmed, net10.0 kept
- [ ] beta.7 release notes updated
- [ ] `ganda repo audit` clean
- [ ] `./bin/dev workflow` green
- [ ] PR opened

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-12)

## Notes

- If the pre-push hook fails with `Failed to map file ... pre-push.deps.json`, run `ganda runfile cache --clear` and push again.
