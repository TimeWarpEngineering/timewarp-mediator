# M2 named pipelines (`ISender<TScope>`)

Source-generated per-scope senders and publishers for epic **004**. Design SSOT: `analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §9.1.

Pipelines are **marker types**, not strings. Use empty types such as `ClientPipeline` /
`ServerPipeline`. `[MediatorModule("Orders")]` does not name a pipeline.

**Do not call `AddMediator()` for this stack.** Named pipelines exist only on the generated
dispatcher: `AddGeneratedMediator()` (unscoped) and `AddGeneratedMediator<TScope>()` (one
named pipeline). Comparison: [generated-vs-legacy.md](./generated-vs-legacy.md).

**14.0.0-beta is not a drop-in for 13.0.0.** As of `14.0.0-beta.1`, named pipelines are
proven only against the M2 golden files (`tests/timewarp-mediator-generators-tests` scoped
pipeline tests and `samples/timewarp-mediator-examples-named-pipelines`). GitHub issue
[#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52) stays **open**
until a **stable 14.0.0**.

A generated host still needs **Contracts + Generators** (and **Analyzers** when the generator
is not referenced). See [generated-vs-legacy.md](./generated-vs-legacy.md#packages-for-a-generated-host).

## What shipped

- Contracts: `ISender<TScope>` / `IPublisher<TScope>` (marker-type pipelines) and `[MediatorScope(typeof(TScope))]`.
- `[assembly: MediatorBehavior(typeof(MyBehavior<,>), Scope = typeof(ClientPipeline))]` assigns a behavior to one pipeline. Omitted `Scope` is the unscoped default pipeline only.
- Generator emits `Sender_{TScope}` and `Publisher_{TScope}` with type-switched dispatch disjoint from each other and from unscoped `Mediator`.
- Host registration: `AddGeneratedMediator()` (unscoped) and `AddGeneratedMediator<TScope>()` (one named pipeline). MS.DI resolves each independently.
- Analyzer: **TWM003** handler/request scope mismatch; **TWM004** typed `ISender<TScope>.Send` of a request from another pipeline (compile error). `Send(object)` of a wrong-scope request throws `NoHandlerException`.
- Sample: `samples/timewarp-mediator-examples-named-pipelines` — `ClientPipeline` + `ServerPipeline` in one host.

`[MediatorModule("Orders")]` remains graph membership (the declaring assembly joins the linker). It does **not** name a pipeline.

## Marker types (`ClientPipeline` / `ServerPipeline`)

```csharp
public sealed class ClientPipeline
{
}

public sealed class ServerPipeline
{
}

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ServerStampBehavior<,>), Scope = typeof(ServerPipeline))]

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
services.AddGeneratedMediator();
services.AddGeneratedMediator<ClientPipeline>();
services.AddGeneratedMediator<ServerPipeline>();

ServiceProvider provider = services.BuildServiceProvider();
ISender<ClientPipeline> client = provider.GetRequiredService<ISender<ClientPipeline>>();
ISender<ServerPipeline> server = provider.GetRequiredService<ISender<ServerPipeline>>();
```

Sample: `samples/timewarp-mediator-examples-named-pipelines`.

## TimeWarp.State switch (later task)

When TimeWarp.State moves onto this package, inject `ISender<ClientPipeline>` on the Blazor client and `ISender<ServerPipeline>` on the server. Do not share one `IMediator` and filter inside behaviors. File that switch task after 004-002 merges; do not implement it here.

## Coexistence

A host that only registers the unscoped sender (`AddGeneratedMediator()`) does not dispatch scoped handlers. Register `AddGeneratedMediator<ClientPipeline>()` (and/or server) in the same container when both pipelines live in one process.

Re-entrant `Send` stays in the same pipeline when the handler injects `ISender<TScope>`. Injecting unscoped `ISender` is a different pipeline.
