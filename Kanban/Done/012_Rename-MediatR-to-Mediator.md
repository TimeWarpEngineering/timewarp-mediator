# Task 012: Rename MediatR to Mediator

## Description
Complete the final renaming from MediatR to Mediator for the remaining API surface area elements. This task addresses the last vestiges of "MediatR" branding in class names and method names to fully establish TimeWarp.Mediator's independent identity.

## Dependencies
- Task 011: Add Fork Attribution Comments (Completed)

## Requirements
- Rename all remaining "MediatR" references to "Mediator" in code
- Update all affected tests and samples
- Maintain proper attribution where legally required
- Ensure all tests pass after changes
- Update documentation to reflect new naming

## Breaking Changes
⚠️ **This task introduces breaking API changes:**
- `MediatRServiceConfiguration` → `MediatorServiceConfiguration`
- `AddMediatR()` → `AddMediator()`

## Implementation Checklist

### Core Library Changes
- [x] Rename `MediatRServiceConfiguration` class to `MediatorServiceConfiguration`
  - File: `src/TimeWarp.Mediator/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs`
  - Update all references throughout codebase
- [x] Rename `AddMediatR` extension methods to `AddMediator`
  - File: `src/TimeWarp.Mediator/MicrosoftExtensionsDI/ServiceCollectionExtensions.cs`
  - Update all overloads
- [x] Update `AddMediatRClassesWithTimeout` to `AddMediatorClassesWithTimeout`
  - File: `src/TimeWarp.Mediator/Registration/ServiceRegistrar.cs`

### Test Updates
- [x] Update all test files that reference `AddMediatR`
  - `test/TimeWarp.Mediator.Tests/GenericRequestHandlerTests.cs`
  - `test/TimeWarp.Mediator.Tests/MicrosoftExtensionsDI/*.cs`
  - `test/TimeWarp.Mediator.Benchmarks/Benchmarks.cs`
- [x] Update all test files that reference `MediatRServiceConfiguration`

### Sample Project Updates
- [x] Update all sample projects to use `AddMediator`
  - `samples/TimeWarp.Mediator.Examples.AspNetCore/Program.cs`
  - `samples/TimeWarp.Mediator.Examples.Autofac/Program.cs`
  - All other sample projects

### Documentation Updates
- [x] Update README.md
  - Change installation instructions to show `AddMediator`
  - Update code examples
  - Remove old MediatR package references
  - Update badges to TimeWarp.Mediator equivalents
  - Preserve attribution section for original MediatR

### Additional Updates
- [x] Update XML documentation comments
- [x] Search for any remaining "MediatR" in comments (except attribution)
- [x] Update Directory.Build.props PackageTags if needed

## Validation Criteria
- [x] Solution builds without errors or warnings
- [x] All 158 tests pass
- [x] All sample projects build and run correctly
- [x] No unintended "MediatR" references remain (grep for MediatR)
- [x] API documentation reflects new naming

## Notes
This completes the comprehensive rebranding effort. After this task, the only remaining "MediatR" references should be:
1. Historical/attribution references in documentation
2. Copyright notices for proper attribution
3. File mapping references showing original names

The goal is complete API independence while maintaining ethical attribution to the original project.