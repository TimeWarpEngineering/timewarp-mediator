# M2 named pipelines (`ISender<TScope>`)

Source-generated per-scope senders and publishers for epic **004**. Design SSOT: `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §9.1.

## What shipped

- Contracts: `ISender<TScope>` / `IPublisher<TScope>` (marker-type pipelines) and `[MediatorScope(typeof(TScope))]`.
- `[assembly: MediatorBehavior(typeof(MyBehavior<,>), Scope = typeof(ClientPipeline))]` assigns a behavior to one pipeline. Omitted `Scope` is the unscoped default pipeline only.
- Generator emits `Sender_{TScope}` and `Publisher_{TScope}` with type-switched dispatch disjoint from each other and from unscoped `Mediator`.
- Host registration: `AddGeneratedMediator()` (unscoped) and `AddGeneratedMediator<TScope>()` (one named pipeline). MS.DI resolves each independently.
- Analyzer: **TWM003** handler/request scope mismatch; **TWM004** typed `ISender<TScope>.Send` of a request from another pipeline (compile error). `Send(object)` of a wrong-scope request throws `NoHandlerException`.
- Sample: `samples/TimeWarp.Mediator.Examples.NamedPipelines` — `ClientPipeline` + `ServerPipeline` in one host.

`[MediatorModule("Orders")]` remains graph membership (the declaring assembly joins the linker). It does **not** name a pipeline.

## TimeWarp.State switch (later task)

When TimeWarp.State moves onto this package, inject `ISender<ClientPipeline>` on the Blazor client and `ISender<ServerPipeline>` on the server. Do not share one `IMediator` and filter inside behaviors. File that switch task after 004-002 merges; do not implement it here.

## Coexistence

A host that only registers the unscoped sender (`AddGeneratedMediator()`) does not dispatch scoped handlers. Register `AddGeneratedMediator<ClientPipeline>()` (and/or server) in the same container when both pipelines live in one process.

Re-entrant `Send` stays in the same pipeline when the handler injects `ISender<TScope>`. Injecting unscoped `ISender` is a different pipeline.
