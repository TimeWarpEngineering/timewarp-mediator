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

### Contracts-Only Build
```powershell
# Build only the contracts package
.\BuildContracts.ps1
```

### Manual Development Commands
```bash
# Clean and build
dotnet clean -c Release
dotnet build -c Release

# Run all tests
dotnet test -c Release --no-build

# Run tests with detailed output
dotnet test -c Release --no-build -l trx --verbosity=normal

# Create packages
dotnet pack .\src\MediatR\MediatR.csproj -c Release -o .\artifacts --no-build
dotnet pack .\src\MediatR.Contracts\MediatR.Contracts.csproj -c Release -o .\artifacts --no-build

# Run single test class (example)
dotnet test test/MediatR.Tests/MediatR.Tests.csproj --filter "ClassName~SendTests"
```

## Architecture Overview

### Core Projects Structure
- **MediatR.Contracts**: Base interfaces (`IRequest`, `INotification`, `IStreamRequest`) - contract-only package
- **MediatR**: Main implementation with `Mediator` class, pipeline behaviors, and DI integrations

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

- **MediatR.Tests**: Comprehensive unit tests covering all scenarios
- **MediatR.Benchmarks**: Performance testing with BenchmarkDotNet
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
- Strong name signing with `MediatR.snk`

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

## Current Project Status

This repository is currently undergoing a rename from MediatR to TimeWarp.Mediator. The Kanban board in `task_000/Kanban/` tracks the progress of this epic rename project across 5 sequential tasks involving project files, namespaces, tests, samples, and build scripts.