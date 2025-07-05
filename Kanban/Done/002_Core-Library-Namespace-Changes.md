# Task 002: Core Library Namespace Changes

## Description
Update all namespace declarations and using statements in the core MediatR library source code to use TimeWarp.Mediator namespaces. This phase focuses only on the core library files, not tests or samples, ensuring the core packages build with the new namespace structure.

## Parent
000_MediatR-to-TimeWarp-Mediator-Rename-Epic

## Dependencies
001_Project-Files-Assembly-Rename must be completed and validated first

## Requirements
- All core library files build successfully with new namespaces
- Package generation works correctly
- Internal references between core library files work
- Public API maintains same structure under new namespace
- Tests and samples continue to use old namespaces (updated in subsequent tasks)

## Checklist

### Design
- [x] Map all namespace changes required
- [x] Identify files requiring using statement updates
- [x] Plan validation approach

### Implementation

#### Core Library Namespace Updates (src/TimeWarp.Mediator/)
- [x] Update root namespace files:
  - `IMediator.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Mediator.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `INotificationHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IRequestHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ISender.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IPublisher.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `INotificationPublisher.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IPipelineBehavior.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IStreamPipelineBehavior.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IStreamRequestHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `NotificationHandlerExecutor.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `TypeForwardings.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

- [x] Update Internal namespace files:
  - `Internal/HandlersOrderer.cs`: `namespace MediatR.Internal` → `namespace TimeWarp.Mediator.Internal`
  - `Internal/ObjectDetails.cs`: `namespace MediatR.Internal` → `namespace TimeWarp.Mediator.Internal`

- [x] Update Pipeline namespace files:
  - `Pipeline/IRequestExceptionAction.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/IRequestExceptionHandler.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/IRequestPostProcessor.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/IRequestPreProcessor.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/RequestExceptionActionProcessorBehavior.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/RequestExceptionHandlerState.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/RequestExceptionProcessorBehavior.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/RequestPostProcessorBehavior.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`
  - `Pipeline/RequestPreProcessorBehavior.cs`: `namespace MediatR.Pipeline` → `namespace TimeWarp.Mediator.Pipeline`

- [x] Update Registration namespace files:
  - `Registration/ServiceRegistrar.cs`: `namespace MediatR.Registration` → `namespace TimeWarp.Mediator.Registration`

- [x] Update Wrappers namespace files:
  - `Wrappers/NotificationHandlerWrapper.cs`: `namespace MediatR.Wrappers` → `namespace TimeWarp.Mediator.Wrappers`
  - `Wrappers/RequestHandlerWrapper.cs`: `namespace MediatR.Wrappers` → `namespace TimeWarp.Mediator.Wrappers`
  - `Wrappers/StreamRequestHandlerWrapper.cs`: `namespace MediatR.Wrappers` → `namespace TimeWarp.Mediator.Wrappers`

- [x] Update NotificationPublishers namespace files:
  - `NotificationPublishers/ForeachAwaitPublisher.cs`: `namespace MediatR.NotificationPublishers` → `namespace TimeWarp.Mediator.NotificationPublishers`
  - `NotificationPublishers/TaskWhenAllPublisher.cs`: `namespace MediatR.NotificationPublishers` → `namespace TimeWarp.Mediator.NotificationPublishers`

- [x] Update MicrosoftExtensionsDI namespace files:
  - `MicrosoftExtensionsDI/MediatrServiceConfiguration.cs`: Extension methods use Microsoft.Extensions.DependencyInjection namespace (no change needed)
  - `MicrosoftExtensionsDI/ServiceCollectionExtensions.cs`: Extension methods use Microsoft.Extensions.DependencyInjection namespace (no change needed)

- [x] Update Entities namespace files:
  - `Entities/OpenBehavior.cs`: `namespace MediatR.Entities` → `namespace TimeWarp.Mediator.Entities`

#### Contracts Library Namespace Updates (src/TimeWarp.Mediator.Contracts/)
- [x] Update contracts namespace files:
  - `INotification.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IRequest.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `IStreamRequest.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Unit.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

#### Update Using Statements in Core Libraries
- [x] Update using statements in core library files:
  - Change `using MediatR;` → `using TimeWarp.Mediator;`
  - Change `using MediatR.Pipeline;` → `using TimeWarp.Mediator.Pipeline;`
  - Change `using MediatR.Internal;` → `using TimeWarp.Mediator.Internal;`
  - Change `using MediatR.Registration;` → `using TimeWarp.Mediator.Registration;`
  - Change `using MediatR.Wrappers;` → `using TimeWarp.Mediator.Wrappers;`
  - Change `using MediatR.NotificationPublishers;` → `using TimeWarp.Mediator.NotificationPublishers;`
  - Change `using MediatR.MicrosoftExtensionsDI;` → `using TimeWarp.Mediator.MicrosoftExtensionsDI;`
  - Change `using MediatR.Entities;` → `using TimeWarp.Mediator.Entities;`

### Documentation
- [x] Update task completion status
- [x] Document any namespace mapping issues
- [x] Record validation results

## Notes
- **Only update core library files** (src/TimeWarp.Mediator/ and src/TimeWarp.Mediator.Contracts/)
- **DO NOT update test files** - that's Task 003
- **DO NOT update sample files** - that's Task 004
- Tests and samples will continue to use old MediatR namespaces until their respective tasks
- Use find/replace carefully to avoid changing comments or strings

## Implementation Notes

### Namespace Changes Summary
- `MediatR` → `TimeWarp.Mediator`
- `MediatR.Internal` → `TimeWarp.Mediator.Internal`
- `MediatR.Pipeline` → `TimeWarp.Mediator.Pipeline`
- `MediatR.Registration` → `TimeWarp.Mediator.Registration`
- `MediatR.Wrappers` → `TimeWarp.Mediator.Wrappers`
- `MediatR.NotificationPublishers` → `TimeWarp.Mediator.NotificationPublishers`
- `MediatR.MicrosoftExtensionsDI` → `TimeWarp.Mediator.MicrosoftExtensionsDI`
- `MediatR.Entities` → `TimeWarp.Mediator.Entities`

### Validation Steps
1. [x] Run `dotnet build src/TimeWarp.Mediator/TimeWarp.Mediator.csproj` - should build successfully
2. [x] Run `dotnet build src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj` - should build successfully
3. [x] Run `dotnet build` on full solution - expected 317 errors in test/sample projects (they still use MediatR namespaces)
4. [x] Verify both NuGet packages are created with correct namespaces
5. [x] Core libraries successfully use TimeWarp.Mediator namespaces internally
6. [x] Ready for Task 003 to update test project namespaces

### Final Results
- ✅ **COMPLETED**: All core library namespace declarations updated (35+ files)
- ✅ **COMPLETED**: All core library using statements updated
- ✅ **COMPLETED**: TimeWarp.Mediator.Contracts builds successfully
- ✅ **COMPLETED**: TimeWarp.Mediator builds successfully  
- ✅ **COMPLETED**: Both NuGet packages created with new namespaces
- ✅ **EXPECTED**: 317 compile errors in test/sample projects (using old MediatR namespaces - will be fixed in Tasks 003-004)

### Files to Modify (~35 files)
- All .cs files in `src/TimeWarp.Mediator/` and subdirectories
- All .cs files in `src/TimeWarp.Mediator.Contracts/`

### Risk Mitigation
- Work on git branch created in Task 001
- Use find/replace carefully to avoid changing comments or strings
- Test build after each namespace group (e.g., all Pipeline files)
- Keep backup of working Task 001 state

### Success Criteria
- All core library files build successfully with new namespaces
- Package generation works correctly
- Internal references between core library files work
- Public API maintains same structure under new namespace
- Ready for Task 003 (Test Project Updates)