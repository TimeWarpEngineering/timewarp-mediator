# Migration Guide

Two migrations are covered here:

1. [MediatR to TimeWarp.Mediator 13.x](#mediatr-to-timewarpmediator-13x): the reflection
   rename (`AddMediatR` → `AddMediator`).
2. [13.x reflection to 14.x generated](#13x-reflection-to-14x-generated): moving a host onto
   `AddGeneratedMediator()`.

The two stacks are compared in
[documentation/generated-vs-legacy.md](./documentation/generated-vs-legacy.md).

## MediatR to TimeWarp.Mediator 13.x

TimeWarp.Mediator 13.x is a fork of MediatR with minimal breaking changes, so this migration is
mostly a rename.

### Quick Migration Steps

#### 1. Update Package References

Replace your MediatR package references:

```xml
<!-- Before -->
<PackageReference Include="MediatR" Version="12.x.x" />
<PackageReference Include="MediatR.Contracts" Version="2.x.x" />

<!-- After -->
<PackageReference Include="TimeWarp.Mediator" Version="13.0.0" />
<PackageReference Include="TimeWarp.Mediator.Contracts" Version="13.0.0" />
```

#### 2. Update Namespaces

Perform a global search and replace across your codebase:

- Find: `using MediatR;`
- Replace: `using TimeWarp.Mediator;`

This single change should handle most of your code migration since all types remain in the same relative namespace structure.

#### 3. Update Dependency Injection

If you're using the built-in DI extensions:

```csharp
// Before
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));

// After
services.AddMediator(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));
```

Note: The method name changed from `AddMediatR` to `AddMediator` (correcting the spelling).

### Detailed Changes

#### Namespace Changes

All namespaces have been updated from `MediatR` to `TimeWarp.Mediator`:

| Before | After |
|--------|-------|
| `MediatR.IMediator` | `TimeWarp.Mediator.IMediator` |
| `MediatR.IRequest` | `TimeWarp.Mediator.IRequest` |
| `MediatR.INotification` | `TimeWarp.Mediator.INotification` |
| `MediatR.IPipelineBehavior` | `TimeWarp.Mediator.IPipelineBehavior` |

#### Package Names

| Before | After |
|--------|-------|
| `MediatR` | `TimeWarp.Mediator` |
| `MediatR.Contracts` | `TimeWarp.Mediator.Contracts` |

#### API Compatibility

TimeWarp.Mediator 13.x keeps MediatR's API apart from the namespace and the `AddMediatR`
rename. All your existing:
- Request/Response handlers
- Notification handlers
- Pipeline behaviors
- Stream handlers
- Exception handlers

...will continue to work without modification after updating the namespaces. This holds for
the 13.x reflection line only; the 14.x generated stack is a different API surface (next
section).

### New Features

#### GetPipelineInfo Extension Method

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

### License Change

TimeWarp.Mediator 13.x is released under The Unlicense. Code derived from MediatR by Jimmy Bogard
stays under Apache 2.0, so redistributions still carry the NOTICE file.

### Migration Script Example

For large codebases, you might want to use a script to automate the migration:

#### PowerShell
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

#### Bash
```bash
# Update .csproj files
find . -name "*.csproj" -type f -exec sed -i 's/Include="MediatR"/Include="TimeWarp.Mediator"/g' {} +
find . -name "*.csproj" -type f -exec sed -i 's/Include="MediatR.Contracts"/Include="TimeWarp.Mediator.Contracts"/g' {} +

# Update C# files
find . -name "*.cs" -type f -exec sed -i 's/using MediatR;/using TimeWarp.Mediator;/g' {} +
find . -name "*.cs" -type f -exec sed -i 's/\.AddMediatR(/\.AddMediator(/g' {} +
```

### Troubleshooting

#### Build Errors After Migration

If you encounter build errors after migration:

1. Ensure all projects have been updated with the new package references
2. Clean your solution and rebuild: `dotnet clean && dotnet build`
3. Clear NuGet cache if needed: `dotnet nuget locals all --clear`

#### Runtime Errors

If you get runtime errors about missing types:

1. Verify all `using` statements have been updated
2. Check that dependency injection registration uses `AddMediator` not `AddMediatR`
3. Ensure you're not mixing MediatR and TimeWarp.Mediator packages in the same solution

## 13.x reflection to 14.x generated

14.x keeps the reflection runtime, so a 13.x host can move to 14.x and keep `AddMediator()`
(it then needs net10.0). That is **not** a move to the generated stack: `AddMediator()` never
registers the generated mediator. To get compile-time dispatch, trimming/NativeAOT safety, or
named pipelines, switch the host to `AddGeneratedMediator()`.

### Feature matrix

| Feature | 13.x / 14.x reflection (`AddMediator`) | 14.x generated, Host profile (`AddGeneratedMediator`) | 14.x generated, Aot / Link profile |
|---------|----------------------------------------|------------------------------------------------------|-----------------------------------|
| Request/response, commands, queries, `Send(object)` | yes | yes | yes |
| Notifications (`Publish`) | yes, with pluggable `INotificationPublisher` | yes, handlers run sequentially in a fixed order | yes, sequential |
| `IPipelineBehavior<,>` | registered with `AddBehavior` / `AddOpenBehavior` | `[assembly: MediatorBehavior(typeof(...))]` | not woven (**TWM005**) |
| Streams (`CreateStream`, `IStreamRequestHandler`, `IStreamPipelineBehavior`) | yes | no: throws `NotSupportedException`, handlers are **TWM009** | no |
| Pre/post processors (`IRequestPreProcessor`, `IRequestPostProcessor`) | yes | no (**TWM009**) | no |
| Exception cascade (`IRequestExceptionHandler`, `IRequestExceptionAction`) | yes | no (**TWM009**) | no |
| Named pipelines (`ISender<TScope>` / `IPublisher<TScope>`) | no | yes, `AddGeneratedMediator<TScope>()` | `Sender_*` / `Publisher_*` are emitted, but there is no `AddGeneratedMediator<TScope>()` |
| Handler discovery | runtime assembly scan | compile time, member assemblies only (`[assembly: MediatorAssembly]`) | compile time |
| Missing or duplicate handler | runtime exception | build error (**TWM001** / **TWM002**) | build error |
| Container support | `Microsoft.Extensions.DependencyInjection`; other containers through their `IServiceCollection` / `IServiceProvider` adapters (see `samples/`) | registers into `IServiceCollection`; any container that consumes it | none: handlers are `new Handler()` singletons in static fields |
| Handler lifetime | transient | scoped | process-wide singleton |
| Trimming / NativeAOT | no (IL2026 / IL3050) | trim and NativeAOT safe | trim and NativeAOT safe, no `IServiceProvider` |
| `GetPipelineInfo()` | yes | no; inspect `MediatorManifest.Json` | no |
| Target | net10.0 (14.x); 13.0.0 also covers netstandard2.0 / net6.0 | net10.0 | net10.0 |

On the Host profile, `AddGeneratedMediator()` registers the mediator, sender, and publisher as
transient and registers handlers and pipeline behaviors as scoped. Reflection scanned handlers
are transient.

If you need streams, pre/post processors, or the exception cascade, stay on `AddMediator()`, or
move that logic into a request handler or an `IPipelineBehavior` first.

### Steps

1. **Packages.** Reference `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators` on
   the host, at the same version. Drop `TimeWarp.Mediator` once nothing calls `AddMediator()`.
   Libraries that hold handlers but do not run the generator reference Contracts and
   `TimeWarp.Mediator.Analyzers`. See
   [Packages for a generated host](./documentation/generated-vs-legacy.md#packages-for-a-generated-host).
2. **Membership.** Hosts that reference Generators are members automatically. Add
   `[assembly: MediatorAssembly]` to every library whose handlers, requests, or behaviors the
   host should dispatch. See [Membership](./documentation/generated-vs-legacy.md#membership).
3. **Behaviors.** Replace `cfg.AddBehavior` / `cfg.AddOpenBehavior` with
   `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]`. The optional `Scope` argument is under
   [Membership](./documentation/generated-vs-legacy.md#membership).
4. **Registration.** Replace `services.AddMediator(cfg => ...)` with
   `services.AddGeneratedMediator()` in the host project. The generated extension is `internal`,
   so call it from the host itself, not from a shared library.
5. **Unsupported kinds.** Build and fix every **TWM009** warning: move stream handlers,
   processors, and exception handlers/actions into request handlers or pipeline behaviors.
6. **Named pipelines (optional).** Mark requests with `[MediatorScope(typeof(ClientPipeline))]`
   and call `AddGeneratedMediator<ClientPipeline>()`. See
   [Named pipelines](./documentation/generated-vs-legacy.md#named-pipelines-marker-types-not-strings).
7. **AOT (optional).** Set `<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>` and read
   [Aot / Link profile limits](./documentation/generated-vs-legacy.md#aot--link-profile-limits).

Every diagnostic is described in [documentation/diagnostics.md](./documentation/diagnostics.md).

## Support

For issues or questions about the migration:
- GitHub Issues: https://github.com/TimeWarpEngineering/timewarp-mediator/issues
- Original MediatR documentation still applies: https://github.com/jbogard/MediatR/wiki