# M1 generated Mediator (004-001)

Source-generated dispatcher, analyzer, and State-shaped golden file for epic **004**. Design SSOT: `analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §14.

**Do not call `AddMediator()` for this stack.** That method is the 13.x reflection fork.
Generated registration is `AddGeneratedMediator()` (and `AddGeneratedMediator<TScope>()` in M2).
Comparison: [generated-vs-legacy.md](./generated-vs-legacy.md).

**14.0.0-beta is not a drop-in for 13.0.0.** As of `14.0.0-beta.1`, this stack is proven only
against the M1/M2 golden files in this repo (generator tests, State-shaped `IncrementActionSet`,
AOT sample, named-pipelines sample). GitHub issue
[#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52) stays **open** until
a **stable 14.0.0**.

## Packages for a generated host

| Package | Required on a host? | Notes |
|---------|---------------------|-------|
| `TimeWarp.Mediator.Contracts` | yes | Runtime + compile. Not pulled by Generators (development dependency, no transitive packages). |
| `TimeWarp.Mediator.Generators` | yes | Emits `sealed TimeWarp.Mediator.Generated.Mediator` and `AddGeneratedMediator()`. Packs analyzer DLLs in the nupkg. |
| `TimeWarp.Mediator.Analyzers` | if Generators is not referenced | TWM001/TWM002 (and TWM003/TWM004 from M2). Safe on a library that does not run the generator. |
| `TimeWarp.Mediator` | no (generated-only host) | Reflection `AddMediator()` runtime. |

## What shipped

- `TimeWarp.Mediator.Analyzers` — TWM001 (request with no handler), TWM002 (duplicate handler). Safe on a library that does **not** run the generator. The library must opt in with `[assembly: MediatorAssembly]`.
- `TimeWarp.Mediator.Generators` — emits `sealed TimeWarp.Mediator.Generated.Mediator : IMediator`, monomorphic `Send`, `Send(object)` switch, `MediatorManifest` v1 (embedded JSON const; source generators cannot write a loose `mediator.manifest.json`), and `AddGeneratedMediator()` (Host profile). Profile `Aot` uses ServiceGen static fields and does **not** weave `[assembly: MediatorBehavior]` into `Dispatch_*` (Host/State is the scoped pipeline path).
- Contracts: `IAction` / `IActionHandler` / `ActionHandler` (`ValueTask`), `ICommand` / `IQuery` + handlers, membership attributes, `NoHandlerException`.
- `ISender`, `IPublisher`, `IMediator`, handler and pipeline interfaces live in `TimeWarp.Mediator.Contracts` (type-forwarded from `TimeWarp.Mediator`) so the AOT sample does not reference the reflection assembly.
- State golden file: `IncrementActionSet` + `StateTransactionBehavior` (scoped) matching `Reverse().Aggregate` (short-circuit, clone/restore, `ExceptionNotification`).
- AOT sample: `samples/timewarp-mediator-examples-aot` with `EnableTrimAnalyzer`, `EnableAotAnalyzer`, `IsAotCompatible`, **no** `NoWarn` on IL2026/IL3050. Profile `Aot` uses ServiceGen static fields.
- Benchmarks: `tests/timewarp-mediator-benchmarks-comparison` vs legacy MakeGenericType, generated `IMediator.Send`, CallSiteInlining `Dispatch_*`, and martinothamar.

The reflection `TimeWarp.Mediator.Mediator` remains the `AddMediator()` implementation (Phase A coexistence). It is a different dispatcher.

## Membership

See spec §9.

```csharp
[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(StateTransactionBehavior<,>))]
```

| Attribute | Role |
|-----------|------|
| `[assembly: MediatorAssembly]` | Opt this assembly into the compile-time graph. |
| `[MediatorScope(typeof(TScope))]` | Named pipeline membership (M2; marker type, not a string). |
| `[assembly: MediatorBehavior(typeof(MyBehavior<,>))]` | Compile-time pipeline. Optional `Scope = typeof(TScope)` selects one named pipeline. |

Hosts that reference the generator package get `TimeWarpMediatorAssembly=true` from `buildTransitive/TimeWarp.Mediator.Generators.props`. Libraries that only need diagnostics reference `TimeWarp.Mediator.Analyzers` and add `[assembly: MediatorAssembly]`.

## Deferred

| Item | Where |
|------|--------|
| `ISender<TScope>` / `IPublisher<TScope>` named pipelines | **004-002** — see [m2-named-pipelines.md](./m2-named-pipelines.md) |
| Call-site interceptors as default dispatch | after M2; CallSiteInlining is measured only |
| Call-graph pruning / WASM-mini `Send(object)` strip | later profile |
| Streams (`CreateStream`) | later; generated Mediator throws `NotSupportedException` |
| Typed exception cascade (`IRequestExceptionHandler`) | later |
| TimeWarp.State NuGet switch | file after this milestone merges |
| TimeWarp.ServiceGen (Nuru 444) | not on this epic's critical path |
| `AddMediator()` auto-preferring the generated type | later coexistence polish; M1 uses `AddGeneratedMediator()` |

GitHub issue [#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52) stays **open** until a stable 14.0.0 (not epic **005**).
