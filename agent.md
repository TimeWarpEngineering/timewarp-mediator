# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimeWarp Mediator is a fork of the MediatR library that implements the mediator pattern for .NET applications. 15.0 is one source-generated dispatcher and two packages. Handlers return `ValueTask`.

## Build Commands

All repo tasks go through the dev CLI (`tools/dev-cli`, a TimeWarp.Nuru runfile). Bootstrap the
AOT binary once per clone:

```bash
dotnet run --file tools/dev-cli/dev.cs -- self-install
```

| Command | What it does |
|---|---|
| `./bin/dev build [--clean] [--quiet]` | Build `timewarp-mediator.slnx` |
| `./bin/dev test [--no-build] [--quiet]` | Run the test suite |
| `./bin/dev pack [--no-build]` | Pack `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators` to `artifacts/packages`. Assert exactly those two nupkgs, the TWM001–TWM004 analyzer DLL inside `TimeWarp.Mediator`, `buildTransitive` props on Generators, exactly `lib/net10.0` and `lib/net11.0`, `notice`, `unlicense`, and each package's own `readme.md` with absolute links only. Then build and run the package smoke consumer on both frameworks; each run must print `package-pong` |
| `./bin/dev verify-samples [--no-build]` | Build every `samples/**/*.csproj`, then run each Exe sample and require exit code 0 (aot and named-pipelines also check their output) |
| `./bin/dev benchmark [--project comparison] [--framework net10.0\|net11.0] [--filter <glob>] [--record] [--gate] [--aot]` | Run BenchmarkDotNet on the comparison project. The default framework is `net10.0`. `--record` on `net10.0` rewrites `documentation/benchmarks/comparison-baseline.md`. `--framework net11.0 --record` writes `documentation/benchmarks/comparison-baseline-net11.md` and leaves the net10.0 gate file alone. `--gate` runs `net10.0` and fails when a `TimeWarp*` row exceeds `comparison-baseline.md`. `--aot` publishes that project with `PublishAot` (`BenchmarkAot=true`, BenchmarkDotNet omitted) and writes `documentation/benchmarks/comparison-baseline-aot.md`. Those numbers are not gated. Pass only one of `--record`, `--gate`, and `--aot`. The comparison project pins BenchmarkDotNet `0.15.8` on `net10.0` and `0.16.0-preview.2` on `net11.0`. `0.15.8` throws on a net11.0 host (`RuntimeMoniker.NotRecognized`) before it measures, and the net10.0 pin is the tool that recorded the gate file. Not part of `workflow`. CI runs `--gate` on a weekly schedule and `workflow_dispatch` mode `benchmark` |
| `./bin/dev workflow [--mode pr\|merge\|release]` | CI pipeline: clean → build → test → verify-samples → pack. Release mode: clean → build → pack → check-version → push |
| `./bin/dev check-version [--strict]` | Fail if `<Version>` is already published on nuget.org for `TimeWarp.Mediator` or `TimeWarp.Mediator.Generators` |
| `./bin/dev release [--dry-run]` | Tag `v<Version>` and create the GitHub Release |
| `./bin/dev clean` | Clean the solution and build artifacts |

`./bin/dev --capabilities` prints the machine-readable command list. If a command is missing from
`./bin/dev --help`, the AOT binary is stale: rerun `self-install`.

Single test project or class:

```bash
dotnet test tests/timewarp-mediator-generators-tests/timewarp-mediator-generators-tests.csproj --filter "ClassName~DispatchSoundnessTests"
```

## Architecture Overview

### Packages (`source/`)

| Project | Package | Target | Contents |
|---|---|---|---|
| `timewarp-mediator` | `TimeWarp.Mediator` | `net10.0;net11.0` | `IRequest`, `ICommand`, `IQuery`, `IAction`, `IIdempotentCommand`, `INotification`, `IRequestHandler` / `INotificationHandler`, `ActionHandler<T>`, `IPipelineBehavior` / `MessageHandlerDelegate`, `ISender` / `IPublisher` / `IMediator` (and the `<TScope>` forms), `MediatorAssembly`, `MediatorScope`, `MediatorBehavior`, `GeneratedMediatorAttribute`, `NoHandlerException`, `Unit`. Packs the TWM001–TWM004 analyzer |
| `timewarp-mediator-generators` | `TimeWarp.Mediator.Generators` | `netstandard2.0` | Source generator: compile-time message graph, `internal sealed Mediator`, named pipelines, `MediatorManifest` version 2, `AddGeneratedMediator()`. Reports TWM007, TWM008, TWM010, TWM011, and TWM001–TWM003 in a host |
| `timewarp-mediator-analyzers` | none (asset of `TimeWarp.Mediator`) | `netstandard2.0` | TWM001–TWM004. Not a NuGet package |

### Dispatcher

One generated dispatcher. A host references `TimeWarp.Mediator` and
`TimeWarp.Mediator.Generators` and calls `AddGeneratedMediator()` from
`TimeWarp.Mediator.Generated` (or `TimeWarpMediatorNamespace`). The constructor takes
`IServiceProvider`. Behaviors registered with `[assembly: MediatorBehavior]` are woven.
`Send<TResponse>` and void `Send<TRequest>` return the handler `ValueTask` from a type switch.
`Send(object)` and `Publish(object)` are the boxing paths. There is no reflection dispatcher,
no `CreateStream`, and no `TimeWarpMediatorProfile`.

`MediatorManifest` (version 2, no profile field) exposes per-request kind, response, scope,
and the ordered behavior list. The host assembly is marked
`[assembly: GeneratedMediator(typeof(Mediator))]`.

See `documentation/architecture.md` and `documentation/diagnostics.md`.

## Test Architecture (`tests/`)

- `timewarp-mediator-generators-tests`: generator golden files and dispatch tests
- `timewarp-mediator-generators-compilation-tests`: compilation, host dispatch, and incrementality (`Cached` on an unrelated edit)
- `timewarp-mediator-analyzers-tests`: TWM001–TWM004
- `timewarp-mediator-benchmarks-comparison`: BenchmarkDotNet (`./bin/dev benchmark`). Baseline files `documentation/benchmarks/comparison-baseline.md` (net10.0 gate), `comparison-baseline-net11.md`, and `comparison-baseline-aot.md` (`--aot`). The scoped martinothamar peer is a separate project so that compilation has one ServiceLifetime
- `timewarp-mediator-package-smoke`: outside-consumer smoke over the packed nupkgs; not in the slnx, run by `dev pack`

The reflection runtime tests and `timewarp-mediator-generators-roslyn-floor-tests` are removed.
`ReferencedCompiler_IsNotNewerThanTheSdkCompiler` in the compilation tests fails when the shipped generator references a `Microsoft.CodeAnalysis` assembly newer than the SDK compiler.

`samples/` holds the generated samples (AOT with a woven behavior, named pipelines). See `samples/readme.md`.

## Build Configuration

### Target Frameworks

- `TimeWarp.Mediator`: `net10.0;net11.0`
- `TimeWarp.Mediator.Generators` and the analyzer project: `netstandard2.0` (Roslyn loads them in-process, including .NET Framework MSBuild and Visual Studio)
- Tests, benchmarks, samples, and the package smoke consumer: `net10.0;net11.0`
- Dev CLI (`tools/dev-cli/dev.cs`): `net10.0`, pinned with `#:property TargetFramework=net10.0`. `TimeWarp.Nuru` 4.0.0-beta.11 ships `lib/net10.0` only. `global.json` pins the SDK to `11.0.100-rc.3` (`rollForward: latestFeature`, `allowPrerelease: true`). CI installs `11.0.x` with `dotnet-quality: preview` and keeps `10.0.x` so the net10.0 runtime pack restores.

### Key Dependencies

- `Microsoft.Extensions.DependencyInjection` for test, benchmark, sample, and smoke hosts. `Directory.Packages.props` pins `10.0.14` when `$(TargetFramework)` is `net10.0` and `11.0.0-rc.3.26522.148` when it is `net11.0`. A net10.0 host does not restore the 11 RC. A generated host needs `Microsoft.Extensions.DependencyInjection.Abstractions` (`IServiceCollection`, `IServiceProvider`). A missing reference is TWM008 and the generator emits nothing.
- `Microsoft.CodeAnalysis.CSharp` for the shipped analyzer and generator, at the central pin. The packages need a .NET SDK whose compiler is at least the Roslyn version they reference (11.0.100 or later; a 10.0.x band only if its compiler is at least that version); an older SDK reports CS8032/CS9057 and the generator emits nothing.

### Build Properties

- `Directory.Build.props` (root): nullable, strict features, banned-API analyzer, Source Link,
  and `TreatWarningsAsErrors`. `tests/` and `samples/` set `TreatWarningsAsErrors` back to
  `false`. The AOT sample sets it back to `true` so trim and AOT warnings fail publish.
  Product assemblies are unsigned. The reflection runtime's `.snk` left with that package
  (review F16). Analyzer and generator `InternalsVisibleTo` entries stay so the generator and
  the incrementality tests can see internal symbols. Those entries are not a product
  strong-name key. A consumer that is itself strong-named still binds: the generator reads
  the host name and public key from the compilation.
- `source/Directory.Build.props`: the single `<Version>` and the shared package metadata.
  `dev check-version` and `dev release` read that version for `TimeWarp.Mediator` and
  `TimeWarp.Mediator.Generators`.
- No repo-wide `LangVersion` pin: `net10.0` and `net11.0` projects use the SDK default; the netstandard2.0
  Roslyn projects set `latest`. Generator-emitted code stays conservative so consumers on
  older language versions still compile it.
- Generator MSBuild properties (from `TimeWarp.Mediator.Generators` buildTransitive props):
  `TimeWarpMediatorNamespace` (default `TimeWarp.Mediator.Generated`; the generator reads this),
  `TimeWarpMediatorAssembly` (default `true`; the analyzer reads this for membership; `false`
  does not stop emission), `TimeWarpMediatorGenerator` (`true` on a generator reference; the
  analyzer then skips TWM001–TWM003). A library avoids emission by not referencing
  `TimeWarp.Mediator.Generators`. `TimeWarpMediatorProfile` and `TimeWarpMediatorAssemblyName`
  are removed. Host identity comes from the compilation.

## Code Style

`.editorconfig` is the style source of truth and matches the TimeWarp `tw-csharp` conventions:

- C#: 2-space indentation, LF line endings, no trailing whitespace, Allman braces.
- Fields and other class-scope members are PascalCase with no underscore prefix; parameters and
  locals are camelCase. A field takes its type's name (`IServiceProvider ServiceProvider`) unless
  that collides with another member.
- Run `dotnet format whitespace --folder --include source/ tests/ samples/` after large edits.
- Files carried over from MediatR keep `// Modified by Steven T. Cramer` (or the XML-comment form
  in project files) as the first line. That line is the Apache-2.0 §4(b) notice that the file was
  changed, so do not remove it. Do not add it to new TimeWarp-authored files.

## Development Workflow

1. Make changes to source code
2. `./bin/dev build`
3. `./bin/dev test`
4. `./bin/dev pack` (or `./bin/dev workflow` for the full CI pipeline)

## Git Workflow

- Default branch: `master`
- **IMPORTANT**: Only use merge commits for PRs - NO squash, NO rebase
- Preserve commit history when merging pull requests
- Work is done in git worktrees (separate working directories for branches)
- Use `git mv` instead of regular `mv` when moving files to preserve git history

## Current Project Status

This repository tracks work on the live `kanban/` board. The legacy MediatR-rename board is archived under `kanban/archived/legacy-mediatr-rename/`.

## Release Process

See `documentation/overview.md` for versioning, CI and releasing (version bump PR, then
`./bin/dev release`). Release notes live in `documentation/release-notes/`. The 15.0 break
list is `documentation/release-notes/15.0.0.md`.
