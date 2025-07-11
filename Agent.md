# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TimeWarp Mediator is a fork of the MediatR library that implements the mediator pattern for .NET applications. It provides in-process messaging with no dependencies, supporting request/response, commands, queries, notifications, and events with both synchronous and asynchronous patterns.

## Build Commands

### Primary Build Process
```powershell
# Full build with tests and packaging
.\Build.ps1
```

### Unified Build Process
Both packages are now built together via ProjectReference dependency.

### Manual Development Commands
```bash
# Clean and build
dotnet clean -c Release
dotnet build -c Release

# Run all tests
dotnet test -c Release --no-build

# Run tests with detailed output
dotnet test -c Release --no-build -l trx --verbosity=normal

# Create packages (both built together via ProjectReference)
dotnet pack .\src\TimeWarp.Mediator\TimeWarp.Mediator.csproj -c Release -o .\Artifacts --no-build
dotnet pack .\src\TimeWarp.Mediator.Contracts\TimeWarp.Mediator.Contracts.csproj -c Release -o .\Artifacts --no-build

# Run single test class (example)
dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj --filter "ClassName~SendTests"
```

## Architecture Overview

### Core Projects Structure
- **TimeWarp.Mediator.Contracts**: Base interfaces (`IRequest`, `INotification`, `IStreamRequest`) - contract-only package
- **TimeWarp.Mediator**: Main implementation with `Mediator` class, pipeline behaviors, and DI integrations

### Key Architectural Patterns
1. **Mediator Pattern**: Central `IMediator` interface combining `ISender` and `IPublisher`
2. **Request/Response**: Single handler per request via `ISender.Send()`
3. **Notification/Events**: Multiple handlers per notification via `IPublisher.Publish()`
4. **Pipeline Behaviors**: Cross-cutting concerns using `IPipelineBehavior<,>`
5. **Stream Processing**: `IStreamRequest<>` with `IAsyncEnumerable<>` responses

### Critical Implementation Details
- Service provider pattern for handler resolution
- Wrapper classes for generic handler invocation
- Built-in support for pre/post processors and exception handling
- Multiple DI container integrations in `MicrosoftExtensionsDI/`

## Test Architecture

- **TimeWarp.Mediator.Tests**: Comprehensive unit tests covering all scenarios
- **TimeWarp.Mediator.Benchmarks**: Performance testing with BenchmarkDotNet
- **samples/**: Multiple DI container integration examples (AspNetCore, Autofac, DryIoc, etc.)

Tests are organized by feature area and include extensive coverage of:
- Generic type constraints and handler resolution
- Pipeline behavior execution order
- Exception handling scenarios
- DI container compatibility

## Build Configuration

### Target Frameworks
- .NET Standard 2.0 (broad compatibility)
- .NET 6.0 (modern features)

### Key Dependencies
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0
- Microsoft.Bcl.AsyncInterfaces 8.0.0 (for .NET Standard 2.0)

### Build Properties (Directory.Build.props)
- Treats warnings as errors
- C# 10.0 language version
- Source linking enabled
- Strong name signing with `TimeWarp.Mediator.snk`

## Development Workflow

1. Make changes to source code
2. Run `dotnet build -c Release` to verify compilation
3. Run `dotnet test -c Release --no-build` to ensure all tests pass
4. Use `.\Build.ps1` for full validation including packaging
5. Sample projects in `samples/` directory demonstrate usage patterns

## Git Workflow

- Default branch: `master`
- **IMPORTANT**: Only use merge commits for PRs - NO squash, NO rebase
- Preserve commit history when merging pull requests
- Work is done in git worktrees (separate working directories for branches)
- Use `git mv` instead of regular `mv` when moving files to preserve git history

## Current Project Status

This repository is currently undergoing a rename from MediatR to TimeWarp.Mediator. The Kanban board in `task_000/Kanban/` tracks the progress of this epic rename project across 5 sequential tasks involving project files, namespaces, tests, samples, and build scripts.

## Release Process

See `Documentation/Overview.md` for the release process.