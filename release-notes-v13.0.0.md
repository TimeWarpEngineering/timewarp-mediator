# TimeWarp.Mediator v13.0.0 Release Notes

## Overview

TimeWarp.Mediator v13.0.0 is the first stable release of our fork of the popular MediatR library. This release marks our transition from MediatR to TimeWarp.Mediator, featuring a corrected spelling, a more permissive license, and maintaining full API compatibility.

## Major Changes

### 🔄 Fork from MediatR
- TimeWarp.Mediator is a fork of MediatR by Jimmy Bogard
- Full API compatibility maintained - existing code works with minimal changes
- All original functionality preserved

### 📝 License Change
- **From**: Apache License 2.0 (original MediatR)
- **To**: The Unlicense (TimeWarp.Mediator)
- New contributions are under The Unlicense, providing maximum freedom for users
- Original MediatR code acknowledgment maintained in NOTICE file

### 📦 Package Rename
- **From**: `MediatR` / `MediatR.Contracts`
- **To**: `TimeWarp.Mediator` / `TimeWarp.Mediator.Contracts`
- Corrects the spelling of "Mediator" in package names

### 🏷️ Namespace Change
- **From**: `MediatR`
- **To**: `TimeWarp.Mediator`
- Simple find/replace migration path

## New Features

### GetPipelineInfo Extension Method
A new diagnostic tool for inspecting registered pipeline components:

```csharp
string pipelineInfo = services.GetPipelineInfo();
// Returns formatted information about registered pre-processors, behaviors, and post-processors
```

## Migration

Migrating from MediatR is straightforward:

1. Update package references in your `.csproj` files
2. Replace `using MediatR;` with `using TimeWarp.Mediator;`
3. Change `services.AddMediatR(...)` to `services.AddMediator(...)`

See the [migration guide](./migration.md) for detailed instructions and automation scripts.

## Breaking Changes

- Package name change (requires updating package references)
- Namespace change (requires updating using statements)
- DI extension method renamed from `AddMediatR` to `AddMediator`

## Acknowledgments

This project is based on the excellent work of Jimmy Bogard and all contributors to the original MediatR project. We are grateful for their creation of this elegant implementation of the mediator pattern.

## Getting Started

```bash
dotnet add package TimeWarp.Mediator
dotnet add package TimeWarp.Mediator.Contracts
```

## Links

- [GitHub Repository](https://github.com/TimeWarpEngineering/timewarp-mediator)
- [Migration Guide](./migration.md)
- [Original MediatR](https://github.com/jbogard/MediatR)

## Future Plans

We plan to maintain API compatibility with MediatR while potentially adding non-breaking enhancements. The focus remains on simplicity and reliability.