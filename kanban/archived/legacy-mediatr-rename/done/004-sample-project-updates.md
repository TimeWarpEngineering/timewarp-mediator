# Task 004: Sample Project Updates

## Description
Update all sample project files to use TimeWarp.Mediator namespaces and ensure all samples build and run successfully. This phase focuses on the 10 sample projects that demonstrate various usage patterns and DI container integrations.

## Parent
000_MediatR-to-TimeWarp-Mediator-Rename-Epic

## Dependencies
1. 001_Project-Files-Assembly-Rename must be completed and validated
2. 002_Core-Library-Namespace-Changes must be completed and validated
3. 003_Test-Project-Updates must be completed and validated (all 108 tests passing)

## Requirements
- All 10 sample projects build successfully
- All sample projects run without errors
- All sample projects demonstrate TimeWarp.Mediator usage correctly
- DI container integrations work with new namespaces
- No build script changes (reserved for Task 005)

## Checklist

### Design
- [x] Identify all sample files requiring namespace updates
- [x] Plan DI container integration validation
- [x] Map sample project dependencies

### Implementation

#### Core Sample Project Updates (samples/TimeWarp.Mediator.Examples/)
- [ ] Update main sample namespace files:
  - `ConstrainedRequestPostProcessor.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericPipelineBehavior.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericRequestPostProcessor.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `GenericRequestPreProcessor.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Jing.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `JingHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Ping.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Pinged.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `PingedHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `PingHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Pong.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Ponged.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Runner.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

- [ ] Update exception handler files:
  - `ExceptionHandler/Exceptions.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionHandler/ExceptionsHandlers.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionHandler/ExceptionsHandlersOverrides.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionHandler/Handlers.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionHandler/LogExceptionAction.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionHandler/Requests.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ExceptionHandler/RequestsOverrides.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

- [ ] Update streams files:
  - `Streams/GenericStreamPipelineBehavior.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Streams/Sing.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Streams/SingHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Streams/Song.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

#### AspNetCore Sample Updates (samples/TimeWarp.Mediator.Examples.AspNetCore/)
- [ ] Update namespace files:
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

#### DI Container Integration Samples
- [ ] **Autofac Sample** (samples/TimeWarp.Mediator.Examples.Autofac/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

- [ ] **DryIoc Sample** (samples/TimeWarp.Mediator.Examples.DryIoc/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

- [ ] **Lamar Sample** (samples/TimeWarp.Mediator.Examples.Lamar/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

- [ ] **LightInject Sample** (samples/TimeWarp.Mediator.Examples.LightInject/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

- [ ] **SimpleInjector Sample** (samples/TimeWarp.Mediator.Examples.SimpleInjector/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

- [ ] **Stashbox Sample** (samples/TimeWarp.Mediator.Examples.Stashbox/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

- [ ] **Windsor Sample** (samples/TimeWarp.Mediator.Examples.Windsor/):
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `ContravariantFilter.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - Update using statements: `using MediatR;` → `using TimeWarp.Mediator;`

#### Publish Strategies Sample (samples/TimeWarp.Mediator.Examples.PublishStrategies/)
- [ ] Update namespace files:
  - `AsyncPingedHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `CustomMediator.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Program.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `Publisher.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `PublishStrategy.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`
  - `SyncPingedHandler.cs`: `namespace MediatR` → `namespace TimeWarp.Mediator`

#### Update Using Statements in All Samples
- [ ] Update using statements across all sample files:
  - Change `using MediatR;` → `using TimeWarp.Mediator;`
  - Change `using MediatR.Pipeline;` → `using TimeWarp.Mediator.Pipeline;`
  - Change other MediatR namespace usings as needed

#### Verify Sample Project References
- [ ] Verify all 10 sample projects reference:
  - ProjectReference to `src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
  - PackageReference to `TimeWarp.Mediator.Contracts` (if needed)

### Documentation
- [ ] Update task completion status
- [ ] Document any sample-specific issues
- [ ] Record DI container integration results

## Notes
- **Focus only on sample projects** (samples/TimeWarp.Mediator.Examples.*)
- Each sample should build and run successfully
- Pay special attention to DI container integrations - they need to work with new assembly names
- Some samples may have specific namespace requirements for their demos

## Implementation Notes

### Critical Areas to Validate
- **DI Container Integration**: Each container should properly register and resolve TimeWarp.Mediator services
- **Request/Response Flow**: Samples should demonstrate proper request handling
- **Notification Publishing**: Notification samples should work correctly
- **Pipeline Behaviors**: Pipeline and exception handling samples should work
- **Streaming**: Stream-based samples should function correctly

### Validation Steps
1. [ ] **Build all sample projects**:
   - `dotnet build samples/TimeWarp.Mediator.Examples/TimeWarp.Mediator.Examples.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.AspNetCore/TimeWarp.Mediator.Examples.AspNetCore.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.Autofac/TimeWarp.Mediator.Examples.Autofac.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.DryIoc/TimeWarp.Mediator.Examples.DryIoc.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.Lamar/TimeWarp.Mediator.Examples.Lamar.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.LightInject/TimeWarp.Mediator.Examples.LightInject.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.PublishStrategies/TimeWarp.Mediator.Examples.PublishStrategies.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.SimpleInjector/TimeWarp.Mediator.Examples.SimpleInjector.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.Stashbox/TimeWarp.Mediator.Examples.Stashbox.csproj`
   - `dotnet build samples/TimeWarp.Mediator.Examples.Windsor/TimeWarp.Mediator.Examples.Windsor.csproj`

2. [ ] **Run all sample projects**:
   - `dotnet run --project samples/TimeWarp.Mediator.Examples/TimeWarp.Mediator.Examples.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.AspNetCore/TimeWarp.Mediator.Examples.AspNetCore.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.Autofac/TimeWarp.Mediator.Examples.Autofac.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.DryIoc/TimeWarp.Mediator.Examples.DryIoc.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.Lamar/TimeWarp.Mediator.Examples.Lamar.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.LightInject/TimeWarp.Mediator.Examples.LightInject.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.PublishStrategies/TimeWarp.Mediator.Examples.PublishStrategies.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.SimpleInjector/TimeWarp.Mediator.Examples.SimpleInjector.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.Stashbox/TimeWarp.Mediator.Examples.Stashbox.csproj`
   - `dotnet run --project samples/TimeWarp.Mediator.Examples.Windsor/TimeWarp.Mediator.Examples.Windsor.csproj`

3. [ ] **Verify sample functionality**:
   - Samples should produce expected output
   - No runtime errors or exceptions
   - DI container integrations should work correctly

4. [ ] **Full solution build**:
   - `dotnet build` on complete solution should succeed

### Files to Modify (~50 files)
- All .cs files in `samples/TimeWarp.Mediator.Examples/` and subdirectories
- All .cs files in all 10 sample project directories

### DI Container Specific Considerations
- Some containers may need assembly scanning updates
- Registration code may need updates for new assembly names
- Extension methods may need namespace updates
- Container-specific documentation may reference old names

### Risk Mitigation
- Work on git branch created in Task 001
- Test each sample project individually
- If any sample fails, investigate whether it's due to namespace changes or DI container issues
- Keep backup of working Task 003 state

### Success Criteria
- All 10 sample projects build successfully
- All sample projects run without errors
- All sample projects demonstrate TimeWarp.Mediator usage correctly
- DI container integrations work with new namespaces
- Ready for Task 005 (Build Scripts & Final Validation)