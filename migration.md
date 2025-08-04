# Migration Guide: MediatR to TimeWarp.Mediator

This guide helps you migrate from MediatR to TimeWarp.Mediator. The migration is straightforward as TimeWarp.Mediator is a fork of MediatR with minimal breaking changes.

## Quick Migration Steps

### 1. Update Package References

Replace your MediatR package references:

```xml
<!-- Before -->
<PackageReference Include="MediatR" Version="12.x.x" />
<PackageReference Include="MediatR.Contracts" Version="2.x.x" />

<!-- After -->
<PackageReference Include="TimeWarp.Mediator" Version="13.0.0" />
<PackageReference Include="TimeWarp.Mediator.Contracts" Version="13.0.0" />
```

### 2. Update Namespaces

Perform a global search and replace across your codebase:

- Find: `using MediatR;`
- Replace: `using TimeWarp.Mediator;`

This single change should handle most of your code migration since all types remain in the same relative namespace structure.

### 3. Update Dependency Injection

If you're using the built-in DI extensions:

```csharp
// Before
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));

// After
services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));
```

Note: The method name changed from `AddMediatR` to `AddMediator` (correcting the spelling).

## Detailed Changes

### Namespace Changes

All namespaces have been updated from `MediatR` to `TimeWarp.Mediator`:

| Before | After |
|--------|-------|
| `MediatR.IMediator` | `TimeWarp.Mediator.IMediator` |
| `MediatR.IRequest` | `TimeWarp.Mediator.IRequest` |
| `MediatR.INotification` | `TimeWarp.Mediator.INotification` |
| `MediatR.IPipelineBehavior` | `TimeWarp.Mediator.IPipelineBehavior` |

### Package Names

| Before | After |
|--------|-------|
| `MediatR` | `TimeWarp.Mediator` |
| `MediatR.Contracts` | `TimeWarp.Mediator.Contracts` |

### API Compatibility

TimeWarp.Mediator maintains full API compatibility with MediatR. All your existing:
- Request/Response handlers
- Notification handlers
- Pipeline behaviors
- Stream handlers
- Exception handlers

...will continue to work without modification after updating the namespaces.

## New Features

### GetPipelineInfo Extension Method

TimeWarp.Mediator adds a new extension method for inspecting registered pipeline components:

```csharp
// Get information about registered pipeline components
string pipelineInfo = services.GetPipelineInfo();
Console.WriteLine(pipelineInfo);

// Output:
// TimeWarp.Mediator Pipeline Information:
// =====================================
// 
// Registered Pre-Processors:
//   - MyApp.PingPreProcessor
// 
// Registered Behaviors:
//   - MyApp.LoggingBehavior`2
// 
// Registered Post-Processors:
//   - MyApp.PingPostProcessor
```

This is useful for debugging and verifying your pipeline configuration.

## License Change

TimeWarp.Mediator is released under The Unlicense, making it freely available for any use without attribution requirements. The original MediatR code by Jimmy Bogard was licensed under Apache 2.0. See the NOTICE file for details.

## Migration Script Example

For large codebases, you might want to use a script to automate the migration:

### PowerShell
```powershell
# Update .csproj files
Get-ChildItem -Recurse -Filter "*.csproj" | ForEach-Object {
    (Get-Content $_.FullName) `
        -replace 'Include="MediatR"', 'Include="TimeWarp.Mediator"' `
        -replace 'Include="MediatR.Contracts"', 'Include="TimeWarp.Mediator.Contracts"' |
    Set-Content $_.FullName
}

# Update C# files
Get-ChildItem -Recurse -Filter "*.cs" | ForEach-Object {
    (Get-Content $_.FullName) `
        -replace 'using MediatR;', 'using TimeWarp.Mediator;' `
        -replace '\.AddMediatR\(', '.AddMediator(' |
    Set-Content $_.FullName
}
```

### Bash
```bash
# Update .csproj files
find . -name "*.csproj" -type f -exec sed -i 's/Include="MediatR"/Include="TimeWarp.Mediator"/g' {} +
find . -name "*.csproj" -type f -exec sed -i 's/Include="MediatR.Contracts"/Include="TimeWarp.Mediator.Contracts"/g' {} +

# Update C# files
find . -name "*.cs" -type f -exec sed -i 's/using MediatR;/using TimeWarp.Mediator;/g' {} +
find . -name "*.cs" -type f -exec sed -i 's/\.AddMediatR(/\.AddMediator(/g' {} +
```

## Troubleshooting

### Build Errors After Migration

If you encounter build errors after migration:

1. Ensure all projects have been updated with the new package references
2. Clean your solution and rebuild: `dotnet clean && dotnet build`
3. Clear NuGet cache if needed: `dotnet nuget locals all --clear`

### Runtime Errors

If you get runtime errors about missing types:

1. Verify all `using` statements have been updated
2. Check that dependency injection registration uses `AddMediator` not `AddMediatR`
3. Ensure you're not mixing MediatR and TimeWarp.Mediator packages in the same solution

## Support

For issues or questions about the migration:
- GitHub Issues: https://github.com/TimeWarpEngineering/timewarp-mediator/issues
- Original MediatR documentation still applies: https://github.com/jbogard/MediatR/wiki