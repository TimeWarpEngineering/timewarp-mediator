---
name: tw-mediator
description: Add and use TimeWarp.Mediator 15.0 - in-process request/response and notifications for .NET with one source-generated dispatcher. Use when adding TimeWarp.Mediator to a project, writing requests, handlers, notifications or pipeline behaviors, calling AddGeneratedMediator, setting up named pipelines (ISender<TScope>), reading MediatorManifest, or fixing TWM001-TWM004, TWM007, TWM008, TWM010, and TWM011.
---

# TimeWarp.Mediator

In-process messaging for .NET. `ISender.Send` reaches one handler. `IPublisher.Publish`
reaches many handlers, sequentially. The dispatcher is source-generated. A host calls
`AddGeneratedMediator()`. Handlers and behaviors return `ValueTask`.

**Packages:** `TimeWarp.Mediator` (contracts and TWM001–TWM004) and `TimeWarp.Mediator.Generators`.
**Repository:** https://github.com/TimeWarpEngineering/timewarp-mediator
**Docs this skill links instead of repeating:**
[architecture.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/architecture.md),
[diagnostics.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md),
[m2-named-pipelines.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m2-named-pipelines.md),
[migration.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/migration.md).

## 1. Dispatcher

| | 15.0 |
|---|---|
| Packages | `TimeWarp.Mediator` + `TimeWarp.Mediator.Generators` |
| Registration | `services.AddGeneratedMediator()` / `AddGeneratedMediator<TScope>()` from the generated namespace. The dispatcher is transient |
| Handler discovery | compile time, member assemblies only |
| Missing / duplicate handler | build error (TWM001 / TWM002) |
| Behaviors | `[assembly: MediatorBehavior(typeof(B<,>))]`, always woven. The host supplies `IServiceProvider` |
| Named pipelines (`ISender<TScope>`) | yes |
| Streams, pre/post processors, exception handlers | removed with the reflection runtime |
| Trimming / NativeAOT | safe, including woven behaviors |

There is one emission. `TimeWarpMediatorProfile` is removed.

## 2. Minimal host

Keep both packages on the same version. Generators does not bring `TimeWarp.Mediator` with it.

```xml
<!-- Directory.Packages.props -->
<ItemGroup>
  <PackageVersion Include="TimeWarp.Mediator" Version="15.0.0-beta.4" />
  <PackageVersion Include="TimeWarp.Mediator.Generators" Version="15.0.0-beta.4" />
  <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.12" />
</ItemGroup>
```

```xml
<!-- host .csproj (net10.0 or net11.0) -->
<ItemGroup>
  <PackageReference Include="TimeWarp.Mediator" />
  <PackageReference Include="TimeWarp.Mediator.Generators" PrivateAssets="all" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
</ItemGroup>
```

Use the version this repo is packing, currently `15.0.0-beta.4`. 14.0.0 on nuget.org
is the previous surface: the id `TimeWarp.Mediator` was the reflection runtime (`AddMediator`),
contracts were `TimeWarp.Mediator.Contracts`, analyzers were a separate package, and handlers
returned `Task`.

### Membership

A project that references Generators emits a mediator. The generator package sets
`TimeWarpMediatorAssembly=true`, which the analyzer reads as membership. The generator does
not read that property. A library that holds handlers or requests for a host opts in and
does not reference Generators:

```csharp
[assembly: MediatorAssembly]
```

That library references `TimeWarp.Mediator`. The TWM001–TWM004 analyzer ships inside that
package.

### Request, handler, send

```csharp
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;
using TimeWarp.Mediator.Generated;

ServiceCollection services = new();
services.AddGeneratedMediator();
using ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();

ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
string reply = await sender.Send(new Ping("hi"));

public sealed record Ping(string Message) : IRequest<string>;

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
    ValueTask.FromResult(request.Message + " pong");
}
```

`AddGeneratedMediator()` is an `internal` extension in the generated namespace
(default `TimeWarp.Mediator.Generated`, class
`TimeWarpMediatorGeneratedServiceCollectionExtensions`). Call it from the host. The
dispatcher is transient. Handlers, behaviors, behavior chains, and `HandlerCache` are
scoped. A singleton dispatcher was not faster than a reused transient instance on the
net10.0 send benchmark, so the method does not take a `ServiceLifetime`
(why: task 020 measurement 2026-10-28). Resolve `ISender` from a scope.

`ICommand`, `ICommand<T>`, `IQuery<T>`, `IAction`, and `IIdempotentCommand` /
`IIdempotentCommand<T>` are markers. Handlers implement `IRequestHandler`. Kind is on
`MediatorManifest`.

### Notifications

```csharp
IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
await publisher.Publish(new OrderPlaced(42));

public sealed record OrderPlaced(int OrderId) : INotification;

public sealed class EmailOnOrderPlaced : INotificationHandler<OrderPlaced>
{
  public ValueTask Handle(OrderPlaced notification, CancellationToken cancellationToken) =>
    ValueTask.CompletedTask;
}
```

`Publish` awaits handlers sequentially in display-name order, most-derived notification type
first, including handlers of base types and `INotification`.

### Pipeline behaviors

```csharp
[assembly: MediatorBehavior(typeof(LoggingBehavior<,>))]

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public async ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken)
  {
    TResponse response = await next(request, cancellationToken);
    return response;
  }
}
```

Constraints select the requests a behavior wraps (`where TRequest : ICommand`,
`where TRequest : IIdempotent`). A constraint that matches nothing is TWM011.

### Named pipelines

Pipelines are empty marker types.

```csharp
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]

services.AddGeneratedMediator();
services.AddGeneratedMediator<ClientPipeline>();

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

- `[MediatorScope]` goes on a type, a containing type, or the assembly.
- A behavior without `Scope` runs on the unscoped pipeline only.
- `AddGeneratedMediator()` does not dispatch scoped requests.
- Re-entrant sends stay in a pipeline when the handler injects `ISender<TScope>`.

Details: [m2-named-pipelines.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/m2-named-pipelines.md).

### Manifest

`MediatorManifest` is `internal` in the generated namespace. `Version` is 2. There is no
profile field. `Request(displayName)` returns kind, response type, scope, and the ordered
behavior list. The host assembly carries
`[assembly: GeneratedMediator(typeof(Mediator))]`.

## 3. Diagnostics

Full text:
[diagnostics.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/diagnostics.md).

| Id | Meaning | Fix |
|---|---|---|
| TWM001 | Request in a member assembly has no handler | Add one `IRequestHandler`, or delete the request |
| TWM002 | More than one handler for a request | Keep exactly one |
| TWM003 | Handler and message name different `[MediatorScope]` markers | Make them match, or scope only one side |
| TWM004 | `ISender<TScope>.Send` (or unscoped `ISender` in a host) sends a request from another pipeline | Inject the matching sender, or move the request |
| TWM007 | Member-assembly type is inaccessible from the host | Make it public, or `[assembly: InternalsVisibleTo("Host")]` |
| TWM008 | `TimeWarp.Mediator` or MS.DI Abstractions is missing. Nothing is generated | Reference `TimeWarp.Mediator` and `Microsoft.Extensions.DependencyInjection.Abstractions` |
| TWM010 (warning) | `[MediatorBehavior]` type is not an `IPipelineBehavior<,>` | Fix the type or remove the attribute |
| TWM011 (warning) | Behavior constraints match no request in its pipeline | Fix the constraints or `Scope`, or remove it |

In a generator host the generator reports TWM001–TWM003. The analyzer reports TWM004 there,
and TWM001–TWM004 in a library. TWM005, TWM006, and TWM009 are retired.

## 4. Gotchas

- **Generated types are `internal`.** `Mediator`, `Sender_*`, `Publisher_*`,
  `MediatorManifest`, and the `AddGeneratedMediator` class are internal to the compilation
  that emits them. Other assemblies take `ISender` / `IPublisher` / `IMediator` from DI.
- **Friend of two hosts.** `InternalsVisibleTo` re-exposes those types, so one test project
  that is a friend of two hosts sees two `Mediator` types (CS0436 / CS0121). Alias one
  reference and use `extern alias`, or give each host its own `TimeWarpMediatorNamespace`.
- **Libraries do not reference Generators.** A reference to `TimeWarp.Mediator.Generators`
  always emits a mediator in that compilation. `TimeWarpMediatorAssembly=false` does not
  stop emission. A library such as Nuru references `TimeWarp.Mediator`, opts in with
  `[assembly: MediatorAssembly]`, and leaves the generator reference on the app. The app
  calls `AddGeneratedMediator()`.
- **`TimeWarpMediatorNamespace`.** Default `TimeWarp.Mediator.Generated`. Set it when two
  hosts' generated types meet in one compilation.
- **SDK compiler.** The packages need a .NET SDK whose compiler is at least the Roslyn version they reference (11.0.100 or later; a 10.0.x band only if its compiler is at least that version); an older SDK reports CS8032/CS9057 and the generator emits nothing.
- **`using static TimeWarp.Mediator.Unit`.** Signatures are unaffected. A bare `ValueTask`
  in an expression is ambiguous (CS0229). Write `Unit.ValueTask` for the value and
  `System.Threading.Tasks.ValueTask` for the type's statics. The removed `Unit.Task`
  property did not hide `Task`.
- **Synchronous `Send` failures are thrown.** A null request, `NoHandlerException`, and a
  handler that throws before returning a `ValueTask` escape `Send`. A faulted `ValueTask`
  stays faulted until awaited. `Enumerable.Select` into `Task.WhenAll` does not capture the
  synchronous throw.
- **Membership is opt-in for libraries.** Without `[assembly: MediatorAssembly]`, a library's
  handlers are left out. `Send` then throws `NoHandlerException`. TWM001 fires when the
  request type is in a member assembly and no handler binds it.
- **`Send(object)`** of a request from another pipeline compiles and throws
  `NoHandlerException` at runtime.

## 5. Migrating

[migration.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/migration.md)
has the package table and the TimeWarp.State, TimeWarp.Architecture, and TimeWarp.Nuru
sections. Breaking changes with finding numbers:
[15.0.0.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/documentation/release-notes/15.0.0.md).
