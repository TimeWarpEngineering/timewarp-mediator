# Update NuGet packages to latest and bump version to 15.0.0-beta.6

## Goal (implementer restatement)

Bring the central package pins up to the latest version on each pin's current channel, and stamp the product as `15.0.0-beta.6`, so the next prerelease is cut from current dependencies. A stable pin stays on the latest stable. A prerelease pin stays on the latest prerelease. This walk does not tag or publish.

Round 1 left two open findings. After that review, nuget.org also published `TimeWarp.Nuru` `4.0.0-beta.12`. Both Nuru pins move to that prerelease. DevCli beta.12 speaks Mediator 15 (`IRequestHandler`, `ValueTask`), and Nuru beta.12 ships `lib/net11.0` only and depends on Mediator `15.0.0-beta.4`, so the dev CLI moves to `net11.0` and its handlers follow that contract. The release note must name the `TimeWarp.SourceGenerators` reference the Roslyn projects remove, and name its generators.

Acceptance:

- `Directory.Packages.props` pins match the latest version on nuget.org for that channel. `tools/dev-cli/dev.cs` has no `#:package` pin. `TimeWarp.SourceGenerators` is `1.0.0-beta.12`. `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are `4.0.0-beta.12`.
- Task 023-001 keeps the `global.json` SDK pin, the `net11.0` `Microsoft.Extensions.DependencyInjection` release-candidate pin, the CI `dotnet-quality: preview` line, and both BenchmarkDotNet pins.
- The `net10.0` DependencyInjection pin stays on the latest `10.0.x` patch (`10.0.14`). A net10.0 host does not restore an 11 package.
- `Microsoft.CodeAnalysis.CSharp` stays at `5.9.0`, which is the latest stable and is not newer than the SDK compiler assembly `5.12.0.0`. `ReferencedCompiler_IsNotNewerThanTheSdkCompiler` stays green.
- `source/Directory.Build.props` `<Version>` is `15.0.0-beta.6`. Install snippets that name the packed version name `15.0.0-beta.6`.
- `documentation/release-notes/15.0.0-beta.6.md` lists the dependency refresh in the prose style of the beta.5 note. It says the generator project, the analyzer project, and their test hosts remove the `TimeWarp.SourceGenerators` reference so its generators do not run inside those Roslyn components. It says `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are `4.0.0-beta.12`, and that the dev CLI is `net11.0` because that Nuru package ships `lib/net11.0` only.
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` succeeds on those pins. `ganda repo audit --fix` is applied and committed. `./bin/dev workflow` is green. `./bin/dev check-version --strict` passes.
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
- [x] Review round 1 M1 and M2 fixed
- [ ] PR opened

## Results

`ganda nuget outdated --no-cache` reported one outdated pin: `TimeWarp.SourceGenerators` `1.0.0-beta.11` to `1.0.0-beta.12` on nuget.org. The other 25 literal pins were already the latest on their channel. `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` are skipped because they float on `$(TimeWarpMediatorVersion)`. `tools/dev-cli/dev.cs` has no `#:package` directive. The TimeWarp GitHub Packages feed has no copies of these pins. `TimeWarp.Amuru` `3.0.0-beta.2` and `TimeWarp.Terminal` `2.0.0-beta.1` exist on nuget.org and stay off these pins because the pins are stable.

`TimeWarp.SourceGenerators` is `1.0.0-beta.12`. `<Version>` is `15.0.0-beta.6`. Install lines in the root readme, the three package readmes, and `skills/tw-mediator/SKILL.md` name that version. `documentation/release-notes/15.0.0-beta.6.md` records the refresh. `global.json`, the `net11.0` DependencyInjection pin, both BenchmarkDotNet pins, and the CI `dotnet-quality: preview` line are unchanged.

`ganda repo audit --fix` exited 0. Passed 38, failed 0, skipped 3. The only fix wrote the local VS Code window title. `.vscode/settings.json` stays gitignored, so the audit added no tracked file.

`./bin/dev workflow` (PR mode, `GITHUB_EVENT_NAME` unset) exited 0 and printed `Pipeline SUCCEEDED`. Build: 0 warnings, 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 50, compilation 80, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.6.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.6.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0.

`dotnet test --no-build -c Release --filter ReferencedCompiler_IsNotNewerThanTheSdkCompiler` passed 1/1 on net10.0 and on net11.0. The SDK compiler assembly is `5.12.0.0`. `Microsoft.CodeAnalysis.CSharp` stays at `5.9.0`.

`./bin/dev check-version --strict` exited 0. Source `15.0.0-beta.6` is one prerelease increment ahead of published `15.0.0-beta.5`. Packages checked: `TimeWarp.Mediator`, `TimeWarp.Mediator.Generators`.

### Review (round 1)

The review ran at effort 1 with one general reviewer. It opened 2 findings: 1 bug and 1 nit.

- M1 (bug): nuget.org published `TimeWarp.Nuru.DevCli` `4.0.0-beta.12` at 2026-11-09T08:54Z, just before implement finished. `ganda nuget outdated --no-cache` flagged the `4.0.0-beta.11` pin.
- M2 (nit): in the release-note sentence about removing the reference, "that reference" is ambiguous.

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

The rest of that diff checks out. The 023-001 pins are untouched. `Microsoft.CodeAnalysis.CSharp` stays at `5.9.0`. Every install snippet names `15.0.0-beta.6`.

### Fix pass (review round 1)

M1 is fixed by moving both Nuru pins. By the time this pass looked, nuget.org also listed `TimeWarp.Nuru` `4.0.0-beta.12` (nuspec commit `77a9540e`, the same commit as DevCli beta.12). That package ships `lib/net11.0` only and depends on `TimeWarp.Mediator` `15.0.0-beta.4` and `TimeWarp.Mediator.Generators` `15.0.0-beta.4`. DevCli beta.12 handlers implement `IRequestHandler` and return `ValueTask<Unit>`. Nuru beta.11 still depends on Mediator 14 (`TimeWarp.Mediator.Contracts` `14.0.0` and `TimeWarp.Mediator.Generators` `14.0.0`) and ships `lib/net10.0` only, so DevCli beta.12 is not the pair for that Nuru. Both pins are `4.0.0-beta.12`. `tools/dev-cli/dev.cs` is `#:property TargetFramework=net11.0`. The six local endpoint handlers implement `IRequestHandler<TCommand, Unit>` and return `ValueTask<Unit>`. `WorkflowCommand.Handler` stays internal. `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` are still internal in DevCli beta.12.

M2 is fixed in `documentation/release-notes/15.0.0-beta.6.md`. The sentence names the `TimeWarp.SourceGenerators` reference and its generators. The note also records the Nuru and DevCli move and the `net11.0` dev CLI.

`ganda nuget outdated --no-cache` then printed `All packages are up to date!` for all 25 literal pins. The two `$(TimeWarpMediatorVersion)` pins stay skipped. `dotnet run --file tools/dev-cli/dev.cs -- self-install` installed `./bin/dev`. `./bin/dev check-version --strict` exited 0. Source `15.0.0-beta.6` is one prerelease increment ahead of `15.0.0-beta.5`. Packages checked: `TimeWarp.Mediator`, `TimeWarp.Mediator.Generators`.

`./bin/dev workflow` (PR mode, `GITHUB_EVENT_NAME` unset) exited 0 and printed `Pipeline SUCCEEDED`. Build: 0 warnings, 0 errors. Tests passed on net10.0 and net11.0: generators 61, analyzers 50, compilation 80, failed 0. Samples verified, including NativeAOT publish of the AOT sample on both frameworks. Pack wrote `TimeWarp.Mediator.15.0.0-beta.6.nupkg` and `TimeWarp.Mediator.Generators.15.0.0-beta.6.nupkg`. Smoke printed `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator` on net10.0 and on net11.0.

`ganda repo audit` exited 0. Passed 38, failed 0, skipped 3. It changed no tracked file.

### Review (round 2) and disposition

Round 2 ran at effort 1 with one general reviewer. It covered the fix delta `8944f20..8d6fdef`: the Nuru pins, `agent.md`, the release note, `tools/dev-cli/dev.cs`, and the six endpoint handlers. M1 (bug) and M2 (nit) are both `fixed`. The round raised no new findings. Final counts: bug 0 open / 1 fixed / 0 wontfix, suggestion 0 / 0 / 0, nit 0 open / 1 fixed / 0 wontfix.

What the reviewer checked:

- nuget.org lists `4.0.0-beta.12` as the latest `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli`.
- `ganda nuget outdated --no-cache` printed `All packages are up to date!`.
- `dotnet build tools/dev-cli/dev.cs` exited 0 on `net11.0`.
- `./bin/dev check-version --strict` exited 0.
- No `ICommandHandler` or `Task<Unit>` remains in the dev CLI.
- CI installs `11.0.x` before it runs the CLI.

The reviewer did not rerun the full `./bin/dev workflow`. That result comes from the fix pass above.

**Disposition: clean** (2 rounds, 0 open, no wontfix). Paths: `review/review-framework.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md`.

### How to validate

Smoke: from this worktree, run `ganda nuget outdated --no-cache`, then `./bin/dev check-version --strict`, then `./bin/dev workflow`.

Expect: outdated prints `All packages are up to date!`, including `TimeWarp.Nuru` `4.0.0-beta.12`, `TimeWarp.Nuru.DevCli` `4.0.0-beta.12`, and `TimeWarp.SourceGenerators` `1.0.0-beta.12`. `check-version --strict` exits 0 and says source `15.0.0-beta.6` is one prerelease increment ahead of `15.0.0-beta.5`. Workflow prints `0 Warning(s)`, the test counts above, `Samples verified successfully!`, both `15.0.0-beta.6` nupkgs, `package-pong` on both frameworks, and `Pipeline SUCCEEDED`.

## Session

- Created: 4bddab61-0b48-4f1d-8a19-57e7f9d047b3 (2026-11-09)
- Implementation: 01a1bf7c-4397-7450-bc11-e300e32d4a1c (2026-11-09)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bf7c-4397-7450-bc11-e300e32d4a1c, max-turns 500, Done — 2026-11-09T08:59:34Z
- Review oracle round 1: Claude Code (claude-opus-5-5), effort 1, general reviewer, turn cap 80, Blocked (fix pass needed for M1, M2) — 2026-11-09
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 03bfb466-76aa-4326-a61c-8104cacd0c6f, max-turns 80, artifacts general.md, merged.md — 2026-11-09T09:02:18Z
- Fix pass: implement-fix-2 by implementer-grok (grok, vendor xai, model grok-4.7), session 01a1bf8b-48ab-77d1-8b07-61397314e59f — 2026-11-09. M1 and M2 fixed. Both Nuru pins are `4.0.0-beta.12`. The dev CLI is `net11.0`.
- Review oracle round 2: Claude Code (claude-opus-5-5), effort 1, general reviewer, turn cap 80, artifacts round-2/general.md, round-2/merged.md, disposition.md. Disposition clean, Done — 2026-11-09

## Review

- Round 1: M1 and M2 fixed on this branch. The host runs review round 2.
- Round 2: Done
## Notes

- beta.5 was tagged `v15.0.0-beta.5` (task 029). beta.3 is a dead tag; do not reuse it.
- Release after merge goes through tw-release from a clean synced master.
