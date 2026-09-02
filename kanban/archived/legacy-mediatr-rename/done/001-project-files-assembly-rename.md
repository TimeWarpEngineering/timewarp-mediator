# Task 001: Project Files & Assembly Rename

## Description
Rename all project files, directories, and assembly configurations from MediatR to TimeWarp.Mediator. This phase focuses on the project structure without touching source code namespaces, ensuring the solution remains buildable throughout the process.

## Parent
000_MediatR-to-TimeWarp-Mediator-Rename-Epic

## Dependencies
None - This is the first task in the rename sequence

## Requirements
- All 14 projects build successfully
- Solution loads correctly in Visual Studio
- Package references between projects work
- Strong name signing works with new key
- No source code namespace changes (reserved for Task 002)

## Checklist

### Design
- [x] Identify all project files and directories requiring rename
- [x] Plan directory structure changes
- [x] Design new strong name key strategy
- [x] Plan solution file updates

### Implementation

#### Directory Structure Rename
- [x] Rename `src/MediatR/` → `src/TimeWarp.Mediator/`
- [x] Rename `src/MediatR.Contracts/` → `src/TimeWarp.Mediator.Contracts/`
- [x] Rename `test/MediatR.Tests/` → `test/TimeWarp.Mediator.Tests/`
- [x] Rename `test/MediatR.Benchmarks/` → `test/TimeWarp.Mediator.Benchmarks/`
- [x] Rename `samples/MediatR.Examples/` → `samples/TimeWarp.Mediator.Examples/`
- [x] Rename `samples/MediatR.Examples.AspNetCore/` → `samples/TimeWarp.Mediator.Examples.AspNetCore/`
- [x] Rename `samples/MediatR.Examples.Autofac/` → `samples/TimeWarp.Mediator.Examples.Autofac/`
- [x] Rename `samples/MediatR.Examples.DryIoc/` → `samples/TimeWarp.Mediator.Examples.DryIoc/`
- [x] Rename `samples/MediatR.Examples.Lamar/` → `samples/TimeWarp.Mediator.Examples.Lamar/`
- [x] Rename `samples/MediatR.Examples.LightInject/` → `samples/TimeWarp.Mediator.Examples.LightInject/`
- [x] Rename `samples/MediatR.Examples.PublishStrategies/` → `samples/TimeWarp.Mediator.Examples.PublishStrategies/`
- [x] Rename `samples/MediatR.Examples.SimpleInjector/` → `samples/TimeWarp.Mediator.Examples.SimpleInjector/`
- [x] Rename `samples/MediatR.Examples.Stashbox/` → `samples/TimeWarp.Mediator.Examples.Stashbox/`
- [x] Rename `samples/MediatR.Examples.Windsor/` → `samples/TimeWarp.Mediator.Examples.Windsor/`

#### Project Files Rename
- [x] Rename `src/TimeWarp.Mediator/MediatR.csproj` → `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
- [x] Rename `src/TimeWarp.Mediator.Contracts/MediatR.Contracts.csproj` → `src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`
- [x] Rename `test/TimeWarp.Mediator.Tests/MediatR.Tests.csproj` → `test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj`
- [x] Rename `test/TimeWarp.Mediator.Benchmarks/MediatR.Benchmarks.csproj` → `test/TimeWarp.Mediator.Benchmarks/TimeWarp.Mediator.Benchmarks.csproj`
- [x] Rename all 10 sample project files to match new directory names

#### Solution File Update
- [x] Rename `MediatR.sln` → `TimeWarp.Mediator.sln`
- [x] Update all 14 project paths in solution file
- [x] Update all project names in solution file
- [x] Added missing Windsor project to solution file
- [x] Test solution loads correctly

#### Assembly Configuration Updates
- [x] Update `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`:
  - Change `<RootNamespace>MediatR</RootNamespace>` → `<RootNamespace>TimeWarp.Mediator</RootNamespace>`
  - Add `<AssemblyName>TimeWarp.Mediator</AssemblyName>`
- [x] Update `src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`:
  - Change `<RootNamespace>MediatR</RootNamespace>` → `<RootNamespace>TimeWarp.Mediator</RootNamespace>`
  - Add `<AssemblyName>TimeWarp.Mediator.Contracts</AssemblyName>`

#### Package References Update
- [x] Update test projects to reference `TimeWarp.Mediator.Contracts` instead of `MediatR.Contracts`
- [x] Update all sample projects to reference `TimeWarp.Mediator.Contracts` instead of `MediatR.Contracts`
- [x] Update all ProjectReference paths in project files

#### Strong Name Key
- [x] Create new strong name key: `TimeWarp.Mediator.snk`
- [x] Update `src/Directory.Build.props`: Change `<AssemblyOriginatorKeyFile>..\..\MediatR.snk</AssemblyOriginatorKeyFile>` → `<AssemblyOriginatorKeyFile>..\..\TimeWarp.Mediator.snk</AssemblyOriginatorKeyFile>`

### Documentation
- [x] Update task completion status
- [x] Document any issues encountered
- [x] Record validation results

## Notes
- **IMPORTANT**: DO NOT change namespaces in source code - that's Task 002
- **IMPORTANT**: DO NOT change using statements - that's Task 002
- This task only renames files, directories, and updates project configurations
- Source code should compile with original MediatR namespaces but new assembly names
- Use `git mv` for all renames to preserve history

## Implementation Notes

### Validation Steps
1. [x] Run `dotnet build` on solution - Core projects build successfully
2. [x] Verify both NuGet packages are created (TimeWarp.Mediator and TimeWarp.Mediator.Contracts)
3. [x] Check package metadata shows correct TimeWarp branding
4. [x] Verify Logo.png is included in packages
5. [x] Test solution loads correctly

### Final Results
- ✅ **COMPLETED**: All 14 projects renamed successfully
- ✅ **COMPLETED**: Core projects (TimeWarp.Mediator, TimeWarp.Mediator.Contracts) build without errors
- ✅ **COMPLETED**: Test projects build successfully 
- ✅ **COMPLETED**: 9/10 sample projects build successfully
- ⚠️ **EXPECTED**: 1 error in Windsor sample (ServiceFactory namespace issue - will be resolved in Task 002)
- ✅ **COMPLETED**: NuGet packages generated with TimeWarp.Mediator branding
- ✅ **COMPLETED**: Git commit created (95e4806)

### Risk Mitigation
- Create git branch before starting
- Use `git mv` for all renames to preserve history
- Test build after each major step
- Keep backup of original MediatR.snk until validation complete

### Files to Modify
1. Directory renames (14 directories)
2. Project file renames (14 files)  
3. `TimeWarp.Mediator.sln`
4. `src/Directory.Build.props`
5. `TimeWarp.Mediator.snk` (new file)
6. All project files for assembly name and package reference updates

### Success Criteria
- All 14 projects build successfully
- Solution loads correctly in Visual Studio
- Package references between projects work
- Strong name signing works with new key
- Ready for Task 002 (Core Library Namespace Changes)