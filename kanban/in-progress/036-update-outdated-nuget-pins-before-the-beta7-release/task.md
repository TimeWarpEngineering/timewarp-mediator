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

## Goal (implementer restatement)

Bring every outdated central pin current before beta.7 is cut again, without changing `<Version>` (`15.0.0-beta.7`) or the dispatcher.

Acceptance criteria:

- `Directory.Packages.props`: `Microsoft.SourceLink.GitHub` 11.0.100, `TimeWarp.Build.Tasks` 2.0.0, `TimeWarp.SourceGenerators` 1.0.0-beta.14, `Microsoft.NET.Test.Sdk` 18.12.0. The `net10.0`-conditioned `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` pins move to the latest 10.0.x (10.0.15), not 11. The `net11.0` pins and both BenchmarkDotNet pins stay.
- `coverlet.collector`: `dev test` still collects with `--collect:"XPlat Code Coverage"`, so coverlet is still used. Bump it to 10.1.0 and keep the test-project references.
- `TimeWarp.Build.Tasks` 2.0.0 breaking change (the task assembly is `net11.0` only, and non-Core MSBuild fails with TWBT0001) is handled in the build and recorded under `## Notes`.
- `#:package` lines in `.githooks/*.cs` have no version (central pins), and `tools/dev-cli/dev.cs` has none. Confirm both, change nothing there.
- .NET 11 targeting confirmed: the 9 multi-target projects are `net10.0;net11.0`, the dev CLI is `net11.0`, `global.json` is SDK `11.0.100`, and the generator and analyzer are `netstandard2.0`. `dev pack` still asserts `lib/net10.0` and `lib/net11.0`.
- Gates: `ganda nuget outdated` lists only the two `net10.0` 10.0.x pins (their 11.0.0 suggestion is ignored), `ganda repo audit` is clean, `./bin/dev workflow` is green, and `./bin/dev check-version --strict` passes.
- `documentation/release-notes/15.0.0-beta.7.md` lists the bumps.

## Checklist

- [ ] SourceLink 11.0.100, Build.Tasks 2.0.0, SourceGenerators beta.14, Test.Sdk 18.12.0 (pins set; beta.14 blocks the dev CLI compile, see Notes)
- [x] coverlet bumped or removed
- [x] net10.0 DependencyInjection and Logging on latest 10.0.x
- [x] Build.Tasks 2.0.0 breaking changes handled and recorded
- [x] .NET 11 targeting confirmed, net10.0 kept
- [x] beta.7 release notes updated
- [x] `ganda repo audit` clean
- [ ] `./bin/dev workflow` green (green with the installed `bin/dev`; `dotnet run --file tools/dev-cli/dev.cs` does not compile on beta.14)
- [ ] PR opened

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-12)
- Implement (2026-11-12): pins bumped, Build.Tasks 2.0.0 adapted, TWA0004 Purpose regions added. Boy-scout: `ganda repo audit --fix` cleared `vscode-window-icon` (wrote the gitignored `.vscode/settings.json`; nothing committed). Parked on the SourceGenerators beta.14 dev CLI break below.
- Implement oracle: implemented by implementer-cursor (cursor, vendor anthropic, model claude-opus-5-5-high), session not reported, max-turns uncapped (no --max-turns flag; bound 500), Blocked — 2026-11-12T06:25:21Z

## Notes

- If the pre-push hook fails with `Failed to map file ... pre-push.deps.json`, run `ganda runfile cache --clear` and push again.

### TimeWarp.Build.Tasks 2.0.0

- Release: https://github.com/TimeWarpEngineering/timewarp-build-tasks/releases/tag/v2.0.0 (PR #11, "retarget to net11.0"). Package diff 1.0.0 to 2.0.0: the task DLL moves from `build/netstandard2.0` to `build/net11.0`. The `UsingTask` now has `Condition="'$(MSBuildRuntimeType)' == 'Core'"`. `TimeWarpSetGitMetadata` raises `TWBT0001` when MSBuild is not Core or its runtime is below 11.0. Task name, outputs (`CommitHash`, `CommitDate`), and the `TimeWarpEnableGitMetadata` switch are unchanged.
- Effect here: `dotnet build` on the pinned 11.0.100 SDK loads it and still stamps `CommitHash` and `CommitDate` (checked in `TimeWarp.Mediator.dll`). Visual Studio and `MSBuild.exe` would fail with `TWBT0001`.
- Adaptation: `Directory.Build.props` sets `TimeWarpEnableGitMetadata=false` when `$(MSBuildRuntimeType)` is not `Core`. Those builds compile without the metadata stamp. Nothing in the repo reads `CommitHash` or `CommitDate`.

### coverlet.collector

- Kept and bumped to 10.1.0. `tools/dev-cli/endpoints/test-command.cs` still collects with `--collect:"XPlat Code Coverage"`, which is coverlet.collector. The only breaking change between 6 and 10 is the .NET 8 floor (8.0.0). `dev test` still writes `coverage.cobertura.xml` (6 files: 3 suites on 2 TFMs).

### TimeWarp.SourceGenerators 1.0.0-beta.14 (blocker)

- beta.13 adds TWA0004 (`#region Purpose`) and TWA0038 (500-line cap), both warnings and errors here under `TreatWarningsAsErrors`.
- TWA0004 on our files is fixed: `#region Purpose` added to the 9 `source/timewarp-mediator` files it flagged and to the global-usings, assembly-info, and `program.cs` files it flagged in tests and samples. The solution builds with 0 warnings.
- The dev CLI runfile (`dotnet run --file tools/dev-cli/dev.cs`, which is also how CI runs `workflow`) fails TWA0038 on two files that `TimeWarp.Nuru.DevCli` 4.0.0-beta.12 compiles in as package content: `content/any/endpoints/release-command.cs` (609 lines) and `content/any/services/nuget-version-service.cs` (704 lines). This repo cannot edit them. 4.0.0-beta.12 is the latest Nuru.DevCli, and timewarp-nuru master still has both files at 609 and 704 lines.
- Not done: lowering TWA0038 or marking NuGet sources `generated_code` (timewarp-state's `external-sources.globalconfig` pattern), removing TimeWarp.SourceGenerators from the dev CLI, or holding beta.12 (timewarp-jaribu holds beta.11 for this reason). Each is a suppression or skips the required pin.
- Unblock: TimeWarp.Nuru.DevCli ships a release with both content files under 500 lines (or TimeWarp.SourceGenerators stops reporting TWA0038 on package-injected sources). Then pin that Nuru.DevCli (and Nuru, same release train), run `dotnet run --file tools/dev-cli/dev.cs -- self-install`, and rerun the gates.

## Results

Pins in `Directory.Packages.props`: SourceLink 11.0.100, Build.Tasks 2.0.0, SourceGenerators 1.0.0-beta.14, Test.Sdk 18.12.0, coverlet.collector 10.1.0, and the `net10.0` DependencyInjection and Logging pins 10.0.15. The `net11.0` 11.0.0 pins and both BenchmarkDotNet pins are unchanged. The `.githooks/*.cs` `#:package` lines carry no version (central pins), and `tools/dev-cli/dev.cs` has no `#:package` line.

.NET 11 targeting: 9 projects are `net10.0;net11.0` (`timewarp-mediator`, 3 test suites, package smoke, 2 benchmark projects, 2 samples), the generator and analyzer are `netstandard2.0`, the dev CLI is `net11.0`, and `global.json` is `11.0.100`. `dev pack` verified `lib/net10.0` and `lib/net11.0`.

Gates on this branch: `./bin/dev workflow` (installed binary) green: 61, 50, and 80 tests on each TFM, samples including NativeAOT, pack, and smoke `package-pong` on net10.0 and net11.0. `./bin/dev check-version --strict` passes for 15.0.0-beta.7. `ganda repo audit` passes all checks. `ganda nuget outdated` lists only the two `net10.0` pins (10.0.15 to 11.0.0, ignored by design). Open: `dotnet run --file tools/dev-cli/dev.cs` fails TWA0038 on Nuru.DevCli content (see Notes).

### How to validate

Smoke:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
./bin/dev workflow
./bin/dev check-version --strict
ganda nuget outdated
ganda repo audit
```

Expect:

- `self-install` compiles the dev CLI with no TWA0038 error (today it fails on `release-command.cs` and `nuget-version-service.cs` from TimeWarp.Nuru.DevCli until upstream ships the split).
- `workflow` ends `Pipeline SUCCEEDED`, with smoke printing `package-pong` on net10.0 and net11.0, and `artifacts/coverage` holding `coverage.cobertura.xml`.
- `check-version --strict` prints `Version in source is new`.
- `nuget outdated` lists only `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` 10.0.15 (net10.0) with an 11.0.0 suggestion.
- `repo audit` prints `Repository passes all audit checks.`

## Blocked

2026-11-12: TimeWarp.Nuru.DevCli must ship a release with content/any/endpoints/release-command.cs and content/any/services/nuget-version-service.cs under 500 lines (or TimeWarp.SourceGenerators must stop reporting TWA0038 on package-injected sources); then pin it, run `dotnet run --file tools/dev-cli/dev.cs -- self-install`, and rerun the gates https://github.com/TimeWarpEngineering/timewarp-nuru

- https://github.com/TimeWarpEngineering/timewarp-nuru
