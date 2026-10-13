---
name: tw-mediator
description: Add and use TimeWarp.Mediator - in-process request/response and notifications for .NET with a source-generated, trim and NativeAOT safe dispatcher beside the MediatR-fork reflection one. Use when adding TimeWarp.Mediator to a project, writing requests, handlers, notifications or pipeline behaviors, choosing the generated (AddGeneratedMediator) or reflection (AddMediator) dispatcher, setting up named pipelines (ISender<TScope>), or fixing TWM001-TWM011 diagnostics.
---

# TimeWarp.Mediator

In-process messaging for .NET: `ISender.Send` (one handler) and `IPublisher.Publish` (many
handlers). 14.x ships two independent dispatchers. Pick one per host; calling `AddMediator()`
never registers the generated mediator.

**Packages:** `TimeWarp.Mediator.Contracts`, `TimeWarp.Mediator.Generators`,
`TimeWarp.Mediator.Analyzers`, `TimeWarp.Mediator`
**Repository:** https://github.com/TimeWarpEngineering/timewarp-mediator
**Docs this skill links instead of repeating:**
[generated-vs-legacy.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md),
[diagnostics.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md),
[m2-named-pipelines.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m2-named-pipelines.md),
[migration.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).

## 1. Which dispatcher

Default to **generated**. Use reflection only when you need something the generated stack does
not dispatch.

| | Generated (default) | Reflection |
|---|---|---|
| Packages | `TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators` | `TimeWarp.Mediator` (pulls Contracts) |
| Registration | `services.AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` | `services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining<T>())` |
| Handler discovery | compile time, member assemblies only | runtime assembly scan |
| Missing / duplicate handler | build error (TWM001 / TWM002) | runtime exception |
| Behaviors | `[assembly: MediatorBehavior(typeof(B<,>))]` | `cfg.AddBehavior` / `cfg.AddOpenBehavior` |
| Named pipelines (`ISender<TScope>`) | yes | no |
| Streams (`IStreamRequest<>`, `CreateStream`) | no (TWM009, throws `NotSupportedException`) | yes |
| Pre/post processors, exception handlers/actions | no (TWM009) | yes |
| Trimming / NativeAOT | safe | no (IL2026 / IL3050) |

Choose reflection only for **streams**, **pre/post processors or the exception cascade**, or
**runtime assembly scanning** (plugins loaded after build). Full matrix:
[migration.md feature matrix](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#feature-matrix).
Do not suppress IL2026/IL3050 on `AddMediator()`; move the host to the generated stack.

## 2. Minimal host (generated)

### Packages (central package management)

Keep every TimeWarp.Mediator package on the same version. Generators does **not** bring
Contracts with it, so reference both.

```xml
<!-- Directory.Packages.props -->
<ItemGroup>
  <PackageVersion Include="TimeWarp.Mediator.Contracts" Version="14.0.0" />
  <PackageVersion Include="TimeWarp.Mediator.Generators" Version="14.0.0" />
  <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
</ItemGroup>
```

```xml
<!-- host .csproj (net10.0) -->
<ItemGroup>
  <PackageReference Include="TimeWarp.Mediator.Contracts" />
  <PackageReference Include="TimeWarp.Mediator.Generators" PrivateAssets="all" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
</ItemGroup>
```

### Membership

A project that references Generators is a graph member automatically
(`TimeWarpMediatorAssembly=true` comes from the package props). A **library** that holds
handlers, requests or behaviors for a host must opt in:

```csharp
[assembly: MediatorAssembly]
```

Such a library references Contracts plus `TimeWarp.Mediator.Analyzers` (not Generators), so it
still fails the build on a missing handler.

### Request, handler, send

```csharp
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;

ServiceCollection services = new();
services.AddGeneratedMediator();
using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();

ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
string reply = await sender.Send(new Ping("hi"));

public sealed record Ping(string Message) : IRequest<string>;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult(request.Message + " pong");
}
```

`AddGeneratedMediator()` is emitted into the `Microsoft.Extensions.DependencyInjection`
namespace as an `internal` extension, so call it from the host's own code. Handlers and
behaviors are registered **scoped**; resolve from a scope. CQRS markers (`ICommand`,
`ICommand<T>`, `IQuery<T>`, `IAction`, `IIdempotentCommand<T>`) dispatch exactly like
`IRequest`.

### Notifications

```csharp
IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
await publisher.Publish(new OrderPlaced(42));

public sealed record OrderPlaced(int OrderId) : INotification;

public sealed class EmailOnOrderPlaced : INotificationHandler<OrderPlaced>
{
  public Task Handle(OrderPlaced notification, CancellationToken cancellationToken) =>
    Task.CompletedTask;
}
```

Generated `Publish` runs handlers sequentially in a fixed order. There is no pluggable
`INotificationPublisher` on the generated stack.

### Pipeline behaviors

Register at compile time with an assembly attribute, not in DI:

```csharp
[assembly: MediatorBehavior(typeof(LoggingBehavior<,>))]

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    TResponse response = await next(cancellationToken);
    return response;
  }
}
```

Generic constraints select which requests a behavior wraps (`where TRequest : ICommand`,
`where TRequest : IIdempotent`). A constraint that matches nothing is TWM011.

### Named pipelines

Pipelines are empty **marker types**, never strings.

```csharp
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]

services.AddGeneratedMediator();                 // unscoped ISender / IPublisher / IMediator
services.AddGeneratedMediator<ClientPipeline>(); // ISender<ClientPipeline> / IPublisher<ClientPipeline>

ISender<ClientPipeline> client = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
string reply = await client.Send(new ClientPing());

public sealed class ClientPipeline
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPing : IRequest<string>
{
}
```

- `[MediatorScope]` goes on a type, a containing type, or the assembly (default for all its types).
- A behavior without `Scope` applies to the unscoped pipeline only.
- Each pipeline is registered separately; `AddGeneratedMediator()` does not dispatch scoped requests.
- Re-entrant sends stay in a pipeline only if the handler injects `ISender<TScope>`.
- `[MediatorModule("Orders")]` is graph membership, not a pipeline name.

Details: [m2-named-pipelines.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m2-named-pipelines.md).

## 3. Profiles

Set with `<TimeWarpMediatorProfile>` in the host project. Default is `Host`.

- **Host** (default): emits `AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()`,
  resolves handlers and behaviors from `IServiceProvider`, weaves `[MediatorBehavior]`. Needs
  `Microsoft.Extensions.DependencyInjection.Abstractions` (TWM008 otherwise). Use this unless
  you have a reason not to.
- **Aot**: no `IServiceProvider`, no `AddGeneratedMediator()`. Construct `new Mediator()`
  (namespace `TimeWarp.Mediator.Generated`); handlers are `new Handler()` process-wide
  singletons in static fields, so keep them stateless. Behaviors are **not woven** (TWM005);
  handlers need an accessible parameterless constructor (TWM006).
- **Link**: today emits the same as Aot (same limits, same diagnostics).

```xml
<TimeWarpMediatorProfile>Aot</TimeWarpMediatorProfile>
```

```csharp
using TimeWarp.Mediator.Generated;

IMediator mediator = new Mediator();
string reply = await mediator.Send(new Ping("aot"));
```

Aot/Link still emit `Sender_*` / `Publisher_*` for named pipelines, but there is no
`AddGeneratedMediator<TScope>()`. Validation, transaction and logging behaviors need Host.
Limits: [Aot / Link profile limits](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/generated-vs-legacy.md#aot--link-profile-limits).

## 4. Diagnostics TWM001-TWM011

All are errors unless marked warning. Full text:
[diagnostics.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md).

| Id | Meaning | Fix |
|---|---|---|
| TWM001 | Request in a member assembly has no handler | Add one `IRequestHandler`, or delete the request |
| TWM002 | More than one handler for a request | Keep exactly one |
| TWM003 | Handler and message name different `[MediatorScope]` markers | Make them match, or scope only one side |
| TWM004 | `ISender<TScope>.Send` (or unscoped `ISender` in a host) sends a request from another pipeline | Inject the right `ISender<TScope>`, or move the request's scope |
| TWM005 (warning) | Behavior not woven under Aot/Link | Use the Host profile, or drop the behavior |
| TWM006 | Aot/Link handler not constructible with `new()` | Add an accessible parameterless ctor (`[SetsRequiredMembers]` for `required` members), or use Host |
| TWM007 | Member-assembly type is `internal` / inaccessible from the host | Make it public, or `[assembly: InternalsVisibleTo("Host")]` in the member |
| TWM008 | Contracts or, on Host, MS.DI Abstractions not referenced; nothing generated | Add Contracts. For the MS.DI gap, add Abstractions, or set the profile to `Aot` when the host does not use MS.DI |
| TWM009 (warning) | Stream handler, pre/post processor or exception handler/action is never called | Use `AddMediator()`, or move the logic into a handler or `IPipelineBehavior` |
| TWM010 (warning) | `[MediatorBehavior]` type is not an `IPipelineBehavior<,>` | Fix the type or remove the registration |
| TWM011 (warning) | Behavior's constraints match no request in its pipeline | Fix the constraints or `Scope`, or remove it |

In a generator host the generator reports TWM001-TWM003; in a library the Analyzers package does.

## 5. Gotchas (from the beta dogfoods)

- **Generated types are `internal`.** `Mediator`, `Sender_*` / `Publisher_*`, `MediatorManifest`
  and the `AddGeneratedMediator` extension class are internal to the compilation that emits
  them. Other assemblies resolve `IMediator` / `ISender` / `IPublisher` from DI; they cannot call
  the host's `AddGeneratedMediator()`. Register once, in the app.
- **Friend of two hosts.** `InternalsVisibleTo` re-exposes the generated types, so a test
  project that is a friend of two hosts sees two `Mediator` types and two
  `AddGeneratedMediator()` (CS0436 / CS0121). Give one reference an alias and use
  `extern alias`:

  ```xml
  <ProjectReference Include="..\server\server.csproj" Aliases="Server" />
  ```

  ```csharp
  extern alias Server;
  ```

- **Libraries that host no handlers.** A library that references Generators (for example a
  framework such as Nuru that flows the generator to apps) but must not be a graph host sets
  `<TimeWarpMediatorAssembly>false</TimeWarpMediatorAssembly>` and never calls
  `AddGeneratedMediator()`; the app does.
- **`TimeWarpMediatorNamespace`.** Generated types land in `TimeWarp.Mediator.Generated` by
  default. Set `<TimeWarpMediatorNamespace>My.App.Mediation</TimeWarpMediatorNamespace>` to move
  them (useful when two hosts' generated types meet in one compilation).
- **net10.0 floor.** `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` 14.x target `net10.0`
  only. Projects that cannot move stay on 13.x. Generators/Analyzers are netstandard2.0 but need
  Roslyn 4.8+ (CS9057 on older compilers).
- **Streams are reflection-only.** `IStreamRequest<>` / `CreateStream` throw
  `NotSupportedException` on the generated mediator (TWM009). Keep `AddMediator()` for them.
- **Membership is opt-in.** A library without `[assembly: MediatorAssembly]` is left out of the
  host graph. When its request types are outside the graph too, the host build stays clean and
  `Send` throws `NoHandlerException`. TWM001 fires only when the request type is already in a
  member assembly and no member handler binds it.
- **`Send(object)`** of a request from another pipeline compiles and throws
  `NoHandlerException` at runtime; prefer typed `Send`.

## 6. Migrating 13.x to 14.0

Full steps and matrix:
[migration.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated);
per-beta changes:
[release-notes-v14.0.0.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/release-notes-v14.0.0.md).

- 14.0.0 is **not** a drop-in for 13.0.0. Keeping `AddMediator()` on 14.x is legal (needs
  net10.0) but stays reflection: no source gen, AOT or named pipelines.
- Packages: host references Contracts + Generators; drop `TimeWarp.Mediator` once nothing calls
  `AddMediator()`. Handler libraries: Contracts + Analyzers + `[assembly: MediatorAssembly]`.
- Behaviors: `cfg.AddBehavior` / `cfg.AddOpenBehavior` become `[assembly: MediatorBehavior(...)]`.
- Registration: `services.AddMediator(cfg => ...)` becomes `services.AddGeneratedMediator()` in
  the host project.
- Build and clear every TWM009: move processors and exception handlers into handlers or
  behaviors, or keep those hosts on reflection.
- Handler lifetime changes from transient (reflection) to scoped (generated Host).
- `GetPipelineInfo()` is reflection-only; inspect `MediatorManifest.Json` on the generated stack.
- Reflection runtime removed public plumbing in beta.5 (`ServiceFactory`, wrappers,
  `ServiceRegistrar`); see the release notes table.
