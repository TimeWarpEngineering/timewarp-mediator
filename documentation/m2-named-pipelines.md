# Named pipelines (`ISender<TScope>`)

Per-scope senders and publishers. The contract page is [architecture.md](architecture.md).
Host packages are in [migration.md](migration.md).

Pipelines are marker types, not strings. Use empty types such as `ClientPipeline` and
`ServerPipeline`.

## What you register

- `ISender<TScope>` and `IPublisher<TScope>` dispatch only the members of that marker.
- `[MediatorScope(typeof(TScope))]` on a request, a handler, a containing type, or the assembly
  assigns the pipeline. A type-level marker wins. No marker means the unscoped default.
- `[assembly: MediatorBehavior(typeof(MyBehavior<,>), Scope = typeof(ClientPipeline))]` assigns
  a behavior to one pipeline. Omitted `Scope` is the unscoped default only. A scoped pipeline
  does not run unscoped behaviors.
- The generator emits `Sender_{TScope}` and `Publisher_{TScope}` with type-switched dispatch
  disjoint from each other and from unscoped `Mediator`.
- `AddGeneratedMediator()` registers the unscoped sender. `AddGeneratedMediator<TScope>()`
  registers one named pipeline. Both extensions are `internal` in the host's generated
  namespace. Call them from the host.
- **TWM003** is a handler/request scope mismatch. **TWM004** is a typed `Send` of a request
  from another pipeline. `Send(object)` of a wrong-scope request throws `NoHandlerException`.

Sample: `samples/timewarp-mediator-examples-named-pipelines`.

## Marker types

```csharp
using TimeWarp.Mediator.Generated;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ServerStampBehavior<,>), Scope = typeof(ServerPipeline))]

public sealed class ClientPipeline
{
}

public sealed class ServerPipeline
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPing : IRequest<string>
{
}

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerPing : IRequest<string>
{
}
```

```csharp
ServiceCollection services = new();
services.AddGeneratedMediator();
services.AddGeneratedMediator<ClientPipeline>();
services.AddGeneratedMediator<ServerPipeline>();

ServiceProvider provider = services.BuildServiceProvider();
using IServiceScope scope = provider.CreateScope();
ISender<ClientPipeline> client = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
ISender<ServerPipeline> server = scope.ServiceProvider.GetRequiredService<ISender<ServerPipeline>>();
```

Behaviors return `ValueTask<TResponse>` and call `next(request, cancellationToken)`. See
[migration.md](migration.md).

## TimeWarp.State

TimeWarp.State injects `ISender<ClientPipeline>` on the Blazor client and can inject
`ISender<ServerPipeline>` on the server. The two pipelines do not share one `IMediator`.
The 15.0 signature changes for State are in [migration.md](migration.md#timewarpstate).

## Coexistence

A host that only calls `AddGeneratedMediator()` does not dispatch scoped handlers. Register
each named pipeline in the same container when both live in one process.

Re-entrant `Send` stays in the same pipeline when the handler injects `ISender<TScope>`.
Injecting unscoped `ISender` is a different pipeline.
