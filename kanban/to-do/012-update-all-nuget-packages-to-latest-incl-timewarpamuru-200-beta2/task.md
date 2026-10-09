# Update all NuGet packages to latest (incl. TimeWarp.Amuru 2.0.0-beta.2)

## Description
Steven wants every repo on the newest packages, pre-releases included (goal is latest, not stable). Run `ganda nuget outdated --update` in this repo and take every package to the newest version it can, not just Amuru. TimeWarp.Amuru and TimeWarp.Amuru.Tools must end on 2.0.0-beta.2 (or newer).

**Current:** TimeWarp.Amuru 1.0.0, TimeWarp.Amuru.Tools 1.0.0-beta.2.

## Checklist
- [x] `ganda nuget outdated --dry-run`, then `ganda nuget outdated --update` (Directory.Packages.props)
- [x] Also bump `#:package ...@version` pins in runfiles (scripts, .githooks are ganda-owned: refresh via `ganda hooks install attest` instead of editing)
- [x] Fix Amuru 1.x -> 2.0 breaking changes (see timewarp-amuru documentation/release-notes/2.0.0.md): Git.*Master* helpers removed (use *Default*), Git methods return result objects instead of bool, no "master" default branchName, removed/renamed dotnet builder options, WithStandardInput("") now closes stdin
- [x] Fix any other breaking changes from other bumped packages
- [x] Release build exits 0, tests green, `ganda repo audit` clean (residual `net6.0` support warnings are in Notes)
- [ ] One PR, merge via `ganda pr merge`

## Notes
Filed 2026-10-09 at Steven's request (Amuru 2.0 sweep across live repos). If a package can't move (e.g. a dependency cycle such as Terminal <-> Amuru), record why here instead of forcing it.

Holdouts and overrides after `ganda nuget outdated --update --force` (2026-10-09):

- **TimeWarp.Amuru** — the updater's stable band offered `1.1.1` because `1.0.0` is a stable pin (stable stays stable-only). `TimeWarp.Amuru.Tools` `2.0.0-beta.2` depends on `TimeWarp.Amuru` >= `2.0.0-beta.2`, and this task requires that version. Pin set to `2.0.0-beta.2`. A later `ganda nuget outdated` reports it up to date.
- **Lamar** — `16.0.0` has `net9.0` and `net10.0` only. Tests and `samples/timewarp-mediator-examples-lamar` are `net8.0`. Held at `15.0.1`, the newest stable with a `net8.0` asset. This is the only pin `ganda nuget outdated` still reports.
- **TimeWarp.Terminal** moved `1.0.1` → `1.0.2`. No Terminal ↔ Amuru cycle. `1.0.2` depends on `TimeWarp.Builder` `1.0.0`, not Amuru.
- **Microsoft.Extensions.DependencyInjection** and **Abstractions** `10.0.12` are the updater's stable targets. Their `buildTransitive/netcoreapp2.0` targets warn that `10.0.12` does not support `net6.0` (support floor is `net8.0`; `net6.0` uses the `netstandard2.0` asset). The packable library still multi-targets `net6.0`, and the Windsor sample is `net6.0`. The warning has no code, `TreatWarningsAsErrors` does not fail the build, and `dotnet build` / `dev build -q` exit 0. Not rolled back: Autofac.Extensions `11.0.2` and LightInject.Microsoft.DependencyInjection `4.1.2` require abstractions 10.
- Stable packages were not moved onto newer prereleases (Mediator `3.1.0-rc.1`, Shouldly 5 preview, BenchmarkDotNet `0.16` preview, DI 11 preview). Packages already on a prerelease moved forward: Nuru and Nuru.DevCli `3.0.0-beta.79`, Amuru.Tools `2.0.0-beta.2`.
- No product runfile has a versioned `#:package` pin. `.githooks` use unversioned `#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` and resolve through `Directory.Packages.props`. Refreshed with `ganda hooks install attest`. `ganda repo audit --fix --checks memsearch-scaffold` removed leftover `.memsearch.toml`; the attest hooks no longer call memsearch.
- `.gitignore` gained the routine-journal globs (`*.next.md`, `*.progress.log`) that `routine-journals-gitignore` requires.

## Session

- Created: 821248 (2026-10-09)
- Implementation: 01a120a3 (2026-10-09)

## Results

`Directory.Packages.props` is on the updater's newest stable (or, for pins that were already prerelease, newest prerelease), with two deliberate exceptions: TimeWarp.Amuru and TimeWarp.Amuru.Tools are `2.0.0-beta.2`, and Lamar is `15.0.1`.

This repo does not call the removed Amuru 2.0 Git.*Master* helpers, bool-returning Git methods, or removed dotnet builder options. `Git.FindRoot` and the `DotNet.Build` / `Clean` / `Test` / `Pack` / `NuGet.Push` builders still compile against Tools `2.0.0-beta.2`.

Nuru `3.0.0-beta.79` moved `ICommand<>`, `ICommandHandler<,>`, and `Unit` to TimeWarp.Mediator and requires `Task<T>` handlers on public endpoint types. The dev CLI global usings and the five local commands follow that contract. `dotnet run --file tools/dev-cli/dev.cs --no-build -- --help` lists build, pack, test, verify-samples, workflow, check-version, clean, release, and self-install. `dev build -q` exits 0.

`dotnet build timewarp-mediator.slnx -c Release` exits 0. `dotnet test timewarp-mediator.slnx -c Release --no-build`: generators-tests 34 passed, analyzers-tests 6 passed, generators-compilation-tests 7 passed, mediator-tests 165 passed and 2 skipped (pre-existing Lamar open-generic skips), 0 failed. All five `.githooks/*.cs` runfiles build. `ganda repo audit` passes 30, fails 0, skips 1 (`runfile-project`).

The `net6.0` builds emit an uncoded support warning from Microsoft.Extensions.DependencyInjection.Abstractions `10.0.12` and a pre-existing one from System.IO.Hashing `10.0.12` (Microsoft.SourceLink.GitHub `10.0.401`, version unchanged). Samples and benchmarks still emit pre-existing RS0030 banned-API warnings (`TreatWarningsAsErrors` is false there) and NETSDK1138 for the Windsor sample's `net6.0`.

PR open and `ganda pr merge` are later host nodes. The checklist item stays open.

### How to validate

#### Smoke

```bash
ganda nuget outdated
dotnet build timewarp-mediator.slnx -c Release
dotnet test timewarp-mediator.slnx -c Release --no-build
dotnet run --file tools/dev-cli/dev.cs -- build -q
dotnet build .githooks/pre-push.cs -c Release
ganda repo audit
```

#### Expect

- `ganda nuget outdated` reports TimeWarp.Amuru and TimeWarp.Amuru.Tools up to date at `2.0.0-beta.2`. The only outdated pin is Lamar `15.0.1` → `16.0.0`.
- Solution build exits 0. Test summary is 212 passed, 2 skipped, 0 failed.
- `dev build -q` prints `Build completed successfully!` and exits 0.
- The pre-push runfile build exits 0 and restores TimeWarp.Amuru `2.0.0-beta.2` and TimeWarp.Amuru.Tools `2.0.0-beta.2`.
- `ganda repo audit` prints `Repository passes all audit checks.` and exits 0.
