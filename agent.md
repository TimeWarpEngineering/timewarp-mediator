# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimeWarp Mediator is a fork of the MediatR library that implements the mediator pattern for .NET applications. It provides in-process messaging, supporting request/response, commands, queries, notifications, and events with both synchronous and asynchronous patterns.

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
| `./bin/dev pack [--no-build]` | Pack Contracts, Analyzers, and Generators to `artifacts/packages`, assert the nupkg layout (analyzer DLLs, buildTransitive props, a single `lib/net10.0` folder on Contracts, `notice`, `unlicense`, and the package's own `readme.md` with absolute links only), then build and run the package smoke consumer against those nupkgs |
| `./bin/dev verify-samples [--no-build]` | Build every `samples/**/*.csproj`, then run each Exe sample and require exit code 0 (aot and named-pipelines also check their output) |
| `./bin/dev benchmark [--project comparison] [--filter <glob>] [--record] [--gate]` | Run BenchmarkDotNet on the comparison project. `--record` rewrites `documentation/benchmarks/comparison-baseline.md` with commit SHA, date, tolerance, and the 15.0 target table. `--gate` re-runs `comparison` and fails when a `TimeWarp*` row exceeds that file. Not part of `workflow`. |
| `./bin/dev workflow [--mode pr\|merge\|release]` | CI pipeline: clean → build → test → verify-samples → pack. Release mode: clean → build → pack → check-version → push |
| `./bin/dev check-version [--strict]` | Fail if `<Version>` is already published on nuget.org |
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
| `timewarp-mediator-contracts` | `TimeWarp.Mediator.Contracts` | `net10.0` | `IRequest`, `ICommand`, `IQuery`, `IAction`, `INotification`, `IStreamRequest`, handler and pipeline-behavior interfaces, `ISender` / `IPublisher` / `IMediator` (and the `<TScope>` forms), membership attributes (`MediatorAssembly`, `MediatorScope`, `MediatorBehavior`, `MediatorModule`), `NoHandlerException`, `Unit` |
| `timewarp-mediator-generators` | `TimeWarp.Mediator.Generators` | `netstandard2.0` | Source generator: compile-time message graph, `internal sealed Mediator`, named pipelines, `AddGeneratedMediator()`; bundles the Analyzers DLL; reports TWM005–TWM011 |
| `timewarp-mediator-analyzers` | `TimeWarp.Mediator.Analyzers` | `netstandard2.0` | TWM001–TWM004 for libraries that do not run the generator |

### Dispatcher

The dispatcher is source-generated. A host references Contracts and Generators and calls
`AddGeneratedMediator()`. Trim and NativeAOT safe; the `Aot` profile dispatches through static
fields. See `documentation/m1-generated-mediator.md`.

Request/response is `ISender.Send()` (one handler). Notifications are `IPublisher.Publish()`
(many handlers). `IPipelineBehavior<,>` runs on the Host profile. The Aot and Link profiles do
not weave behaviors. `CreateStream` / `IStreamRequest<>` throw `NotSupportedException` and report
TWM009. See `documentation/diagnostics.md`.

## Test Architecture (`tests/`)

- `timewarp-mediator-generators-tests`: generator golden files and dispatch tests
- `timewarp-mediator-generators-compilation-tests`: generator incrementality and compilation tests
- `timewarp-mediator-generators-roslyn-floor-tests`: compiles against Microsoft.CodeAnalysis.CSharp 4.8.0
- `timewarp-mediator-analyzers-tests`: TWM analyzer tests
- `timewarp-mediator-benchmarks-comparison`: BenchmarkDotNet (`./bin/dev benchmark`)
- `timewarp-mediator-package-smoke`: outside-consumer smoke over the packed nupkgs; not in the slnx, run by `dev pack`

`samples/` holds the generated samples (AOT, named pipelines). See `samples/readme.md`.

## Build Configuration

### Target Frameworks
- `TimeWarp.Mediator.Contracts`: `net10.0` only (14.x floor; consumers on older runtimes stay on 13.x)
- `TimeWarp.Mediator.Generators` and `TimeWarp.Mediator.Analyzers`: `netstandard2.0` (Roslyn loads them inside the compiler process, including .NET Framework MSBuild and Visual Studio hosts)
- Tests, benchmarks, samples and the dev CLI: `net10.0`

### Key Dependencies
- Microsoft.Extensions.DependencyInjection.Abstractions (version pinned in `Directory.Packages.props`; central package management)
- Microsoft.CodeAnalysis.CSharp 4.8.0 floor for the shipped analyzers/generators (`VersionOverride`)

### Build Properties
- `Directory.Build.props` (root): nullable, strict features, banned-API analyzer, Source Link,
  and `TreatWarningsAsErrors`. `tests/` and `samples/` set `TreatWarningsAsErrors` back to `false`.
  Assemblies are not strong-named. Analyzer and generator `InternalsVisibleTo` entries stay so
  the generator and the incrementality tests can see internal symbols.
- `source/Directory.Build.props`: the single `<Version>` and the package metadata.
- No repo-wide `LangVersion` pin: `net10.0` projects use the SDK default (C# 14); the netstandard2.0 Roslyn projects set `latest`. Generator-emitted code stays conservative so consumers on older language versions still compile it.

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
`./bin/dev release`). Release notes live in `documentation/release-notes/`.
