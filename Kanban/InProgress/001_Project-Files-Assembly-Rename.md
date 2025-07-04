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
- [ ] Rename `src/MediatR/` → `src/TimeWarp.Mediator/`
- [ ] Rename `src/MediatR.Contracts/` → `src/TimeWarp.Mediator.Contracts/`
- [ ] Rename `test/MediatR.Tests/` → `test/TimeWarp.Mediator.Tests/`
- [ ] Rename `test/MediatR.Benchmarks/` → `test/TimeWarp.Mediator.Benchmarks/`
- [ ] Rename `samples/MediatR.Examples/` → `samples/TimeWarp.Mediator.Examples/`
- [ ] Rename `samples/MediatR.Examples.AspNetCore/` → `samples/TimeWarp.Mediator.Examples.AspNetCore/`
- [ ] Rename `samples/MediatR.Examples.Autofac/` → `samples/TimeWarp.Mediator.Examples.Autofac/`
- [ ] Rename `samples/MediatR.Examples.DryIoc/` → `samples/TimeWarp.Mediator.Examples.DryIoc/`
- [ ] Rename `samples/MediatR.Examples.Lamar/` → `samples/TimeWarp.Mediator.Examples.Lamar/`
- [ ] Rename `samples/MediatR.Examples.LightInject/` → `samples/TimeWarp.Mediator.Examples.LightInject/`
- [ ] Rename `samples/MediatR.Examples.PublishStrategies/` → `samples/TimeWarp.Mediator.Examples.PublishStrategies/`
- [ ] Rename `samples/MediatR.Examples.SimpleInjector/` → `samples/TimeWarp.Mediator.Examples.SimpleInjector/`
- [ ] Rename `samples/MediatR.Examples.Stashbox/` → `samples/TimeWarp.Mediator.Examples.Stashbox/`
- [ ] Rename `samples/MediatR.Examples.Windsor/` → `samples/TimeWarp.Mediator.Examples.Windsor/`

#### Project Files Rename
- [ ] Rename `src/TimeWarp.Mediator/MediatR.csproj` → `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
- [ ] Rename `src/TimeWarp.Mediator.Contracts/MediatR.Contracts.csproj` → `src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`
- [ ] Rename `test/TimeWarp.Mediator.Tests/MediatR.Tests.csproj` → `test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj`
- [ ] Rename `test/TimeWarp.Mediator.Benchmarks/MediatR.Benchmarks.csproj` → `test/TimeWarp.Mediator.Benchmarks/TimeWarp.Mediator.Benchmarks.csproj`
- [ ] Rename all 10 sample project files to match new directory names

#### Solution File Update
- [ ] Rename `MediatR.sln` → `TimeWarp.Mediator.sln`
- [ ] Update all 14 project paths in solution file
- [ ] Update all project names in solution file
- [ ] Test solution loads correctly

#### Assembly Configuration Updates
- [ ] Update `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`:
  - Change `<RootNamespace>MediatR</RootNamespace>` → `<RootNamespace>TimeWarp.Mediator</RootNamespace>`
  - Add `<AssemblyName>TimeWarp.Mediator</AssemblyName>`
- [ ] Update `src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`:
  - Change `<RootNamespace>MediatR</RootNamespace>` → `<RootNamespace>TimeWarp.Mediator</RootNamespace>`
  - Add `<AssemblyName>TimeWarp.Mediator.Contracts</AssemblyName>`

#### Package References Update
- [ ] Update test projects to reference `TimeWarp.Mediator.Contracts` instead of `MediatR.Contracts`
- [ ] Update all sample projects to reference `TimeWarp.Mediator.Contracts` instead of `MediatR.Contracts`
- [ ] Update all ProjectReference paths in project files

#### Strong Name Key
- [ ] Create new strong name key: `TimeWarp.Mediator.snk`
- [ ] Update `src/Directory.Build.props`: Change `<AssemblyOriginatorKeyFile>..\..\MediatR.snk</AssemblyOriginatorKeyFile>` → `<AssemblyOriginatorKeyFile>..\..\TimeWarp.Mediator.snk</AssemblyOriginatorKeyFile>`

### Documentation
- [ ] Update task completion status
- [ ] Document any issues encountered
- [ ] Record validation results

## Notes
- **IMPORTANT**: DO NOT change namespaces in source code - that's Task 002
- **IMPORTANT**: DO NOT change using statements - that's Task 002
- This task only renames files, directories, and updates project configurations
- Source code should compile with original MediatR namespaces but new assembly names
- Use `git mv` for all renames to preserve history

## Implementation Notes

### Validation Steps
1. [ ] Run `dotnet build` on solution - should build successfully
2. [ ] Verify both NuGet packages are created (TimeWarp.Mediator and TimeWarp.Mediator.Contracts)
3. [ ] Check package metadata shows correct TimeWarp branding
4. [ ] Verify Logo.png is included in packages
5. [ ] Test solution loads in Visual Studio without errors

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