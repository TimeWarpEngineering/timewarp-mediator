# Task 007: Migrate to .NET 9

## Description
Migrate the entire TimeWarp.Mediator solution from .NET 8 to .NET 9 to provide users with the latest performance optimizations and modern .NET features. This aligns with our targeting strategy of `netstandard2.0` (legacy compatibility) and `net9.0` (modern performance).

## Dependencies
- Task 006 completion (Strong Name Key & Windsor Fix)
- .NET 9 SDK availability

## Requirements
- All projects targeting .NET 9 instead of .NET 8
- CI/CD updated for .NET 9 
- All tests passing on .NET 9
- All 10 sample projects working on .NET 9
- Package generation successful with .NET 9

## Scope

### Core Libraries (2 projects)
- [ ] **TimeWarp.Mediator**: Update from `netstandard2.0;net6.0` → `netstandard2.0;net9.0`
- [ ] **TimeWarp.Mediator.Contracts**: Keep as `netstandard2.0` (no change needed)

### Sample Projects (10 projects) 
- [ ] **TimeWarp.Mediator.Examples.AspNetCore**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.Autofac**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.DryIoc**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.Lamar**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.LightInject**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.PublishStrategies**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.SimpleInjector**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.Stashbox**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples.Windsor**: `net6.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Examples** (base): Update if needed

### Test Projects (2 projects)
- [ ] **TimeWarp.Mediator.Tests**: `net8.0` → `net9.0`
- [ ] **TimeWarp.Mediator.Benchmarks**: `net8.0` → `net9.0`

### CI/CD Infrastructure
- [ ] **GitHub Actions CI**: Update from .NET 8.0 to .NET 9.0 SDK
- [ ] **Release workflow**: Update to .NET 9.0 (when we enable it)
- [ ] **Build scripts**: Verify work with .NET 9

## Implementation Plan

### Phase 1: Core Library
1. Update main TimeWarp.Mediator project to target `netstandard2.0;net9.0`
2. Verify all dependencies support .NET 9
3. Build and test core library

### Phase 2: Sample Projects  
1. Update all sample projects to target `net9.0`
2. Test each DI container integration works on .NET 9
3. Verify all samples build and run successfully

### Phase 3: Test Projects
1. Update test projects to `net9.0`
2. Run full test suite on .NET 9
3. Ensure all 158 tests pass (2 Lamar skips expected)

### Phase 4: CI/CD
1. Update GitHub Actions to use .NET 9 SDK
2. Test full CI pipeline with .NET 9
3. Verify package generation works correctly

## Validation Criteria
- [ ] **Build**: Zero errors, zero warnings on .NET 9
- [ ] **Tests**: All 158 tests passing on .NET 9
- [ ] **Samples**: All 10 samples working on .NET 9  
- [ ] **Packages**: Both NuGet packages created successfully
- [ ] **CI**: GitHub Actions working with .NET 9
- [ ] **Performance**: Verify .NET 9 performance benefits

## Benefits
- **Modern Performance**: .NET 9 users get latest optimizations
- **Future Ready**: Positions TimeWarp.Mediator as cutting-edge
- **Simplified Targeting**: Clean `netstandard2.0` + `net9.0` strategy
- **Competitive Edge**: Shows commitment to latest .NET

## Notes
- This migration aligns with our "ship modern, fix edge cases later" philosophy
- .NET 9 assemblies are forward compatible with future .NET versions
- Legacy users still supported via netstandard2.0 target