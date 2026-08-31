# M2 ISender TScope named pipelines

## Description

Named pipelines: `ISender<TScope>` / `IPublisher<TScope>` as separate generated classes. Marker types name the pipeline (`ClientPipeline` vs `ServerPipeline`). Each scope gets its own dispatch table and behavior chain. No runtime "is this my message?" filtering.

Parent: **004**. This is the TimeWarp.State client vs server split.

## Depends on

- 004-001

## Requirements

- `ISender<TScope>` and `IPublisher<TScope>` plus unscoped `ISender` / `IPublisher` (unscoped = default pipeline)
- Generator emits a concrete Sender and Publisher **per** `TScope` with type-switched dispatch
- Handlers and behaviors belong to a scope via membership (`[MediatorModule]`, options, or equivalent — pick one and document it)
- `ISender<ClientPipeline>` never dispatches server handlers; `ISender<ServerPipeline>` never dispatches client handlers
- Behaviors registered for one scope do not run on the other
- Re-entrant `Send` stays in the same scope unless the caller injects a different `ISender<T>`
- MS.DI can resolve `ISender<TScope>` independently (two mediators in one host)

## Checklist

### Design
- [x] Membership rule: how a handler/behavior is assigned to a scope (do not leave this implicit)
- [x] Unscoped vs scoped coexistence (what happens if a host only registers the unscoped sender)

### Implementation
- [x] Interfaces in Contracts
- [x] Per-scope Sender/Publisher emit
- [x] Sample: `ClientPipeline` + `ServerPipeline` in one host with disjoint handler sets
- [x] Tests: wrong-scope send is a compile error or a hard runtime miss (prefer compile error)
- [x] Re-entrancy within a scope still works (M1 golden-file behaviors)

### Documentation
- [x] Spec § scoped-sender fold-in
- [x] Note for the future TimeWarp.State switch task: inject `ISender<ClientPipeline>` on the Blazor client, `ISender<ServerPipeline>` on the server

### Review
- [x] Effort-1 general round 1 under `review/`
- [x] Disposition clean (0 open) on this task id

## Out of scope

- TimeWarp.State package switch (file after 004-001, implement after this task)
- Call-site interceptors / pruning
- TimeWarp.ServiceGen

## Notes

- This is **not** string-named pipelines. `TScope` is a marker type.
- Impossible to retrofit onto martinothamar; this is the product reason to own Mediator.
- Nuru 443 may use scoped senders after this ships; it must not implement them.
- Review kitchen: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: 2438044 (2026-08-31)
- Implementer: grok session 01a0591d-acec-71b0-9b3e-30fcebf1e991 (2026-09-01)
- Review oracle: grok (2026-09-01) round-1 general 01a0593a-45e7-7de2-8dad-6f08314b5d28; disposition clean

## Results

Named pipelines (`ISender<TScope>` / `IPublisher<TScope>`) are marker-type dispatch tables, not string names and not runtime "is this my message?" filters.

**Membership:** `[MediatorScope(typeof(TScope))]` on handler, request, containing type, or assembly (closest type wins; assembly is the default). `[assembly: MediatorBehavior(typeof(B<,>), Scope = typeof(TScope))]` assigns a behavior to that pipeline. Omitted `Scope` is the unscoped default pipeline only. `[MediatorModule("Orders")]` stays graph membership and does not name a pipeline.

**Unscoped vs scoped:** `AddGeneratedMediator()` registers only unscoped `ISender` / `IPublisher` / `IMediator`. `AddGeneratedMediator<TScope>()` registers that pipeline independently. A host that only registers the unscoped sender cannot dispatch scoped handlers (`NoHandlerException` on `Send(object)`).

**Emit:** generator writes `Sender_{TScope}` (`ISender<TScope>`) and `Publisher_{TScope}` (`IPublisher<TScope>`) with type-switched dispatch disjoint from each other and from unscoped `Mediator`.

**Wrong-scope send:** TWM004 is a compile error on typed `ISender<TScope>.Send` of a request from another pipeline. `Send(object)` throws `NoHandlerException`. TWM003 is a compile error when handler and request specify different scopes.

**TimeWarp.State switch (later task):** inject `ISender<ClientPipeline>` on the Blazor client and `ISender<ServerPipeline>` on the server. Do not share one `IMediator` and filter inside behaviors.

**Tests:** analyzer 6 passed (includes TWM003/TWM004); generator 19 passed (M1 golden-file + 9 scoped tests); reflection suite 163 passed / 2 skipped with `DOTNET_ROLL_FORWARD=LatestMajor`. Named-pipelines sample printed `client:hello` / `server:hello` and exited 0.

**Key files:** `src/TimeWarp.Mediator.Contracts/{ISender,IPublisher,MediatorScopeAttribute,MediatorBehaviorAttribute}.cs`; `src/TimeWarp.Mediator.Analyzers/{MessageGraph,MessageGraphBuilder,RequestHandlerAnalyzer,DiagnosticDescriptors}.cs`; `src/TimeWarp.Mediator.Generators/{MediatorEmitter,ManifestEmitter}.cs`; `test/TimeWarp.Mediator.Generators.Tests/ScopedPipelineTests.cs`; `samples/TimeWarp.Mediator.Examples.NamedPipelines/`; `Analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §9.1; `Documentation/m2-named-pipelines.md`.

### How to validate

**Smoke**

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-004-002-m2-isender-tscope-named-pipelines
dotnet test test/TimeWarp.Mediator.Generators.Tests/TimeWarp.Mediator.Generators.Tests.csproj -c Release --filter FullyQualifiedName~ScopedPipelineTests
dotnet test test/TimeWarp.Mediator.Analyzers.Tests/TimeWarp.Mediator.Analyzers.Tests.csproj -c Release --filter "FullyQualifiedName~Twm003|FullyQualifiedName~Twm004"
dotnet run --project samples/TimeWarp.Mediator.Examples.NamedPipelines/TimeWarp.Mediator.Examples.NamedPipelines.csproj -c Release
```

**Expect**

- ScopedPipelineTests: all passed (client/server dispatch, wrong-scope `NoHandlerException`, monomorphic Send surface, re-entrancy, DI independence, manifest scopes).
- Twm003 / Twm004: both passed.
- Sample stdout is `client:hello` then `server:hello`, process exit 0.

**Automated gate**

```bash
dotnet test test/TimeWarp.Mediator.Generators.Tests/TimeWarp.Mediator.Generators.Tests.csproj -c Release
dotnet test test/TimeWarp.Mediator.Analyzers.Tests/TimeWarp.Mediator.Analyzers.Tests.csproj -c Release
DOTNET_ROLL_FORWARD=LatestMajor dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj -c Release
```

Expect: generator 19 passed; analyzer 6 passed; reflection 163 passed, 2 skipped. M1 `IncrementActionSetTests` (re-entrancy, `Reverse().Aggregate` order) stay in the generator suite.

**Not in scope:** TimeWarp.State NuGet switch, call-site interceptors, streams.

### Review

- **Effort:** 1 (general only). **Rounds:** 1.
- **Roster:** general (round 1: 01a0593a-45e7-7de2-8dad-6f08314b5d28).
- **Final counts:** bug 0; suggestion 0; nit 0. Open = 0.
- **Disposition:** **clean** (`review/disposition.md`).
- **Paths:** `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- No findings, no wontfix, no escalation. Re-check: generator 19 passed; analyzer 6 passed; NamedPipelines sample printed `client:hello` / `server:hello` and exited 0.
