# Task 003: Test Project Updates

## Description
Update all test project files to use TimeWarp.Mediator namespaces and ensure all tests continue to pass. This phase focuses on the test projects and benchmark projects, validating that the core library changes maintain functionality.

## Parent
000_MediatR-to-TimeWarp-Mediator-Rename-Epic

## Dependencies
1. 001_Project-Files-Assembly-Rename must be completed and validated
2. 002_Core-Library-Namespace-Changes must be completed and validated

## Requirements
- All 108 tests pass with new namespaces
- Benchmark project builds and runs successfully
- Test projects build successfully
- All test namespaces updated to TimeWarp.Mediator
- No sample project changes (reserved for Task 004)

## Checklist

### Design
- [x] Identify all test files requiring namespace updates
- [x] Plan test validation approach
- [x] Map benchmark project requirements

### Implementation

#### Test Project Namespace Updates (test/TimeWarp.Mediator.Tests/)
- [x] Update main test namespace files:
  - `Pipeline/Streams/StreamPipelineBehaviorTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `PipelineTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `PublishTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `SendTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `SendVoidInterfaceTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ServiceFactoryTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `StreamPipelineTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `UnitTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `CreateStreamTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericRequestHandlerTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericTypeConstraintsTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

- [x] Update MicrosoftExtensionsDI test files:
  - `MicrosoftExtensionsDI/AssemblyResolutionTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/BaseGenericRequestHandlerTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/CustomMediatorTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/DerivingRequestsTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/DuplicateAssemblyResolutionTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/Handlers.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/NotificationPublisherTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/TypeEvaluatorTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `MicrosoftExtensionsDI/TypeResolutionTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

- [x] Update Pipeline test files:
  - `Pipeline/RequestExceptionActionTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Pipeline/RequestExceptionHandlerTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Pipeline/RequestPostProcessorTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Pipeline/RequestPreProcessorTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

- [x] Update other test files:
  - `NotificationPublisherTests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

#### Benchmark Project Namespace Updates (test/TimeWarp.Mediator.Benchmarks/)
- [x] Update benchmark namespace files:
  - `Benchmarks.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `DotTraceDiagnoser.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericPipelineBehavior.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericRequestPostProcessor.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericRequestPreProcessor.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Ping.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Pinged.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

#### Update Using Statements in Test Projects
- [x] Update using statements in test files:
  - Change `using MediatR;` → `using TimeWarp.Mediator;`
  - Change `using MediatR.Pipeline;` → `using TimeWarp.Mediator.Pipeline;`
  - Change other MediatR namespace usings as needed
  - Fixed namespace references in test code

#### Verify Test Project References
- [x] Verify `test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj` references:
  - ProjectReference to `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
  - PackageReference to `TimeWarp.Mediator.Contracts`

- [x] Verify `test/TimeWarp.Mediator.Benchmarks/TimeWarp.Mediator.Benchmarks.csproj` references:
  - ProjectReference to `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
  - PackageReference to `TimeWarp.Mediator.Contracts`
  - Updated target framework from .NET 6.0 to .NET 8.0

### Documentation
- [x] Update task completion status
- [x] Document any test failures and resolutions
- [x] Record benchmark performance comparison

## Notes
- **Focus only on test projects** (test/TimeWarp.Mediator.Tests/ and test/TimeWarp.Mediator.Benchmarks/)
- **DO NOT update sample files** - that's Task 004
- All tests must pass before moving to next task
- Test namespaces should use TimeWarp.Mediator but can be different from production namespaces where appropriate

## Implementation Notes

### Critical Test Areas to Validate
- **MicrosoftExtensionsDI tests**: Ensure DI registration works with new namespaces
- **Pipeline tests**: Verify pipeline behaviors work with new namespaces
- **Notification tests**: Ensure notification publishing works
- **Request/Response tests**: Verify request handling works
- **Stream tests**: Ensure streaming functionality works
- **Exception handling tests**: Verify exception handling pipeline works

### Validation Steps
1. [x] Run `dotnet build test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj` - should build successfully
2. [x] Run `dotnet build test/TimeWarp.Mediator.Benchmarks/TimeWarp.Mediator.Benchmarks.csproj` - should build successfully
3. [x] Run `dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj` - **158/160 tests passed (2 skipped)**
4. [x] Run `dotnet run --project test/TimeWarp.Mediator.Benchmarks/TimeWarp.Mediator.Benchmarks.csproj` - should execute successfully
5. [x] Test projects build successfully with new namespaces
6. [x] All test functionality preserved with TimeWarp.Mediator namespaces

### Final Results
- ✅ **COMPLETED**: All 30 test files namespace declarations updated
- ✅ **COMPLETED**: All 8 benchmark files namespace declarations updated
- ✅ **COMPLETED**: 13 files updated with 14 using statement changes
- ✅ **COMPLETED**: All test project references verified and working
- ✅ **COMPLETED**: 158/160 tests passed (2 skipped tests are expected)
- ✅ **COMPLETED**: Benchmark project builds and runs successfully
- ✅ **COMPLETED**: Updated benchmark target framework to .NET 8.0 for compatibility

### Files to Modify (~25 files)
- All .cs files in `test/TimeWarp.Mediator.Tests/` and subdirectories
- All .cs files in `test/TimeWarp.Mediator.Benchmarks/` and subdirectories

### Test-Specific Considerations
- Some test classes may need to remain in test-specific namespaces
- Test helper classes and mock objects may need namespace updates
- DI container tests need special attention to ensure they work with new assembly names
- Performance benchmarks should show similar results to validate no regressions

### Risk Mitigation
- Work on git branch created in Task 001
- Run tests frequently during updates
- If any tests fail, investigate whether it's due to namespace changes or actual functionality issues
- Keep backup of working Task 002 state

### Success Criteria
- All 108 tests pass with new namespaces
- Benchmark project builds and runs successfully
- Test projects build successfully
- All test namespaces updated to TimeWarp.Mediator
- Ready for Task 004 (Sample Project Updates)