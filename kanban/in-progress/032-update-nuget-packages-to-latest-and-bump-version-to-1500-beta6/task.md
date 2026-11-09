# Update NuGet packages to latest and bump version to 15.0.0-beta.6

## Goal (implementer restatement)

Bring the central package pins up to the latest version on each pin's current channel, and stamp the product as `15.0.0-beta.6`, so the next prerelease is cut from current dependencies. A stable pin stays on the latest stable. A prerelease pin stays on the latest prerelease. This walk does not tag or publish.

Acceptance:

- `Directory.Packages.props` pins match the latest version on nuget.org for that channel. `tools/dev-cli/dev.cs` has no `#:package` pin. The only pin behind its channel is `TimeWarp.SourceGenerators`, which moves from `1.0.0-beta.11` to `1.0.0-beta.12`.
- Task 023-001 keeps the `global.json` SDK pin, the `net11.0` `Microsoft.Extensions.DependencyInjection` release-candidate pin, the CI `dotnet-quality: preview` line, and both BenchmarkDotNet pins.
- The `net10.0` DependencyInjection pin stays on the latest `10.0.x` patch (`10.0.14`). A net10.0 host does not restore an 11 package.
- `Microsoft.CodeAnalysis.CSharp` stays at `5.9.0`, which is the latest stable and is not newer than the SDK compiler assembly `5.12.0.0`. `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` stays green.
- `source/Directory.Build.props` `<Version>` is `15.0.0-beta.6`. Install snippets that name the packed version name `15.0.0-beta.6`.
- `documentation/release-notes/15.0.0-beta.6.md` lists the dependency refresh in the prose style of the beta.5 note.
- `ganda repo audit --fix` is applied and committed. `./bin/dev workflow` is green. `./bin/dev check-version --strict` passes.
- The pull request is a later host node.

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

- [x] Package pins updated (023-001 pins untouched)
- [x] `ganda repo audit --fix` applied and committed
- [x] `<Version>` is `15.0.0-beta.6`
- [x] Release notes `15.0.0-beta.6.md` added
- [x] `./bin/dev workflow` green
- [x] `./bin/dev check-version --strict` passes
- [ ] PR opened

## Results

`ganda nuget outdated --no-cache` reported one outdated pin: `TimeWarp.SourceGenerators` `1.0.0-beta.11` to `1.0.0-beta.12` on nuget.org. The other 25 literal pins were already the latest on their channel. `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` are skipped because they float on `$(TimeWarpMediatorVersion)`. `tools/dev-cli/dev.cs` has no `#:package` directive. The TimeWarp GitHub Packages feed has no copies of these pins. `TimeWarp.Amuru` `3.0.0-beta.2` and `TimeWarp.Terminal` `2.0.0-beta.1` exist on nuget.org and stay off these pins because the pins are stable.

`TimeWarp.SourceGenerators` is `1.0.0-beta.12`. `<Version>` is `15.0.0-beta.6`. Install lines in the root readme, the three package readmes, and `skills/tw-mediator/SKILL.md` name that version. `documentation/release-notes/15.0.0-beta.6.md` records the refresh. `global.json`, the `net11.0` DependencyInjection pin, both BenchmarkDotNet pins, and the CI `dotnet-quality: preview` line are unchanged.

`ganda repo audit --fix` exited 0. Passed 38, failed 0, skipped 3. The only fix wrote the local VS Code window title. `.vscode/settings.json` stays gitignored, so the audit added no tracked file.

`./bin/dev workflow` (PR mode, `GITHUB_EVENT_NAME` unset) exited 0 and printed `Pipeline SUCCEEDED`. Build: 0 warnings, 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 50, compilation 80, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.6.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.6.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0.

`dotnet test --no-build -c Release --filter ReferencedCompiler_IsNotNewerThanTheSdkCompiler` passed 1/1 on net10.0 and on net11.0. The SDK compiler assembly is `5.12.0.0`. `Microsoft.CodeAnalysis.CSharp` stays at `5.9.0`.

`./bin/dev check-version --strict` exited 0. Source `15.0.0-beta.6` is one prerelease increment ahead of published `15.0.0-beta.5`. Packages checked: `TimeWarp.Mediator`, `TimeWarp.Mediator.Generators`.

### How to validate

Smoke: from this worktree, run `ganda nuget outdated --no-cache`, then `./bin/dev check-version --strict`, then `./bin/dev workflow`.

Expect: outdated prints `1 outdated package` only before the pin move, and `up to date` for `TimeWarp.SourceGenerators` `1.0.0-beta.12` after it. `check-version --strict` exits 0 and says source `15.0.0-beta.6` is one prerelease increment ahead of `15.0.0-beta.5`. Workflow prints `0 Warning(s)`, the test counts above, `Samples verified successfully!`, both `15.0.0-beta.6` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`. A filtered `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` run reports `Passed: 1` on net10.0 and net11.0.

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-09)
- Implementation: 01a1bf7c-4397-7450-bc11-e300e32d4a1c (2026-11-09)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bf7c-4397-7450-bc11-e300e32d4a1c, max-turns 500, Done — 2026-11-09T08:59:34Z

## Notes

- beta.5 was tagged `v15.0.0-beta.5` (task 029). beta.3 is a dead tag; do not reuse it.
- Release after merge goes through tw-release from a clean synced master.
