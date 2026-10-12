# M1 generated Mediator (004-001)

Source-generated dispatcher, analyzer, and State-shaped golden file for epic **004**. Design SSOT: `analysis/2026-06-17-source-gen-aot-rewrite-spec.md` §14.

**Do not call `AddMediator()` for this stack.** That method is the 13.x reflection fork.
Generated registration is `AddGeneratedMediator()` (and `AddGeneratedMediator<TScope>()` in M2).
Comparison: [generated-vs-legacy.md](./generated-vs-legacy.md).

**14.0.0-beta is not a drop-in for 13.0.0.** As of `14.0.0-beta.4`, this stack is proven only
against the M1/M2 golden files in this repo (generator tests, State-shaped `IncrementActionSet`,
AOT sample, named-pipelines sample). GitHub issue
[#52](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/52) stays **open** until
a **stable 14.0.0**.

## Packages for a generated host

| Package | Required on a host? | Notes |
|---------|---------------------|-------|
| `TimeWarp.Mediator.Contracts` | yes | Runtime + compile. Not pulled by Generators (development dependency, no transitive packages). |
| `TimeWarp.Mediator.Generators` | yes | Emits `internal sealed TimeWarp.Mediator.Generated.Mediator` and an internal `AddGeneratedMediator()`. Packs analyzer DLLs in the nupkg. |
| `TimeWarp.Mediator.Analyzers` | if Generators is not referenced | TWM001/TWM002 (and TWM003/TWM004 from M2). Safe on a library that does not run the generator. |
| `TimeWarp.Mediator` | no (generated-only host) | Reflection `AddMediator()` runtime. |

## What shipped

- `TimeWarp.Mediator.Analyzers` — TWM001 (request with no handler), TWM002 (duplicate handler). Safe on a library that does **not** run the generator. The library must opt in with `[assembly: MediatorAssembly]`.
- `TimeWarp.Mediator.Generators` — emits `internal sealed TimeWarp.Mediator.Generated.Mediator : IMediator`, monomorphic `Send`, `Send(object)` switch, `MediatorManifest` v1 (embedded JSON const; source generators cannot write a loose `mediator.manifest.json`), and `AddGeneratedMediator()` (Host profile). Profile `Aot` uses ServiceGen static fields (handlers are process-wide singletons and need an accessible parameterless constructor, else **TWM006**) and does **not** weave `[assembly: MediatorBehavior]` into `Dispatch_*`: each such behavior is **TWM005** and the Aot manifest lists no behaviors (Host/State is the scoped pipeline path). See [generated-vs-legacy.md](./generated-vs-legacy.md#aot--link-profile-limits).
- Contracts: `IAction` / `IActionHandler` / `ActionHandler` (`ValueTask`), `ICommand` / `IQuery` + handlers, membership attributes, `NoHandlerException`.
- Contracts (14.0.0-beta.2): `IIdempotent` marker, `IIdempotentCommand` / `IIdempotentCommand<T>` (`: ICommand…, IIdempotent`) + `IIdempotentCommandHandler<>` / `IIdempotentCommandHandler<,>`. `IQuery<T>` now implements `IIdempotent`. Dispatch is identical to `ICommand`; idempotency-key enforcement and a dedup store are out of scope.
- Generator (14.0.0-beta.3, [#68](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/68)): `IRequestHandler<T, Unit>` dispatches as a response-returning request (only `IRequestHandler<T>` is void), and array / other non-named responses (`IQuery<T[]>`) are discovered and close pipeline behaviors.
- Generator (14.0.0-beta.4): every generated type (`Mediator`, `Sender_*` / `Publisher_*`, `MediatorManifest`, `GeneratedMediatorServiceCollectionExtensions`) is `internal` in all profiles, so a compilation that references two hosts sees no duplicate types (CS0436). Resolve the mediator through `IMediator` / `ISender` / `IPublisher` from outside the host.
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

## Handler discovery and ordering

- **Every interface binds.** A class that implements several handler interfaces (`INotificationHandler<A>` and `INotificationHandler<B>`, or several `IRequestHandler<,>`) is bound and registered once per interface. When a class implements both `IRequestHandler<T, R>` and `IRequestHandler<T>` for the same `T`, the two-arity binding wins.
- **Inherited bindings do not repeat.** If a concrete, non-generic base class in a member assembly already handles an interface, a derived class does not get a second binding for it. `class B : A` is not a TWM002 duplicate, and a notification is not published to it twice. A derived class still binds the interfaces it adds itself. Abstract or generic bases (`HandlerBase<T>`) are not bound themselves, so their closed derived classes bind normally.
- **Deterministic order.** Member assemblies are walked in ordinal order of assembly name, and each handler list is sorted ordinally by fully qualified display name. Notification handlers run in that order. The TWM002 message lists handlers in that order and points at the second one. The generated source does not depend on reference order or hash-set order.

## Type switch and notification variance

- **`Send(object)` is most-derived first.** Request cases are ordered so a derived request type comes before every base class or interface it implements (ordinal display name breaks ties). A `ZDerived : ABase` request with its own handler dispatches to that handler, and the generated switch never hits CS8120. A runtime request type with no handler of its own dispatches through the first bound supertype in that order.
- **`Publish` is contravariant (MediatR parity).** `Publish(notification)` runs the handlers of **every** bound notification type the runtime object is assignable to: its own type, base classes, and interfaces (`INotificationHandler<IAuditEvent>` receives every notification that implements `IAuditEvent`). Types run most-derived first, then each type's handlers in display-name order. A derived notification with no handler of its own still reaches its bases' handlers. A class that handles both `Base` and `Derived` is called once per interface. Assignability is the base-class chain plus implemented interfaces; generic variance on a notification interface (`IEvent<out T>`) is not modelled.
- **Named pipelines** apply the same rules within one `TScope`: delivery only reaches handlers bound in that scope.
- **Generated names are safe.** Pattern locals are fixed (`r0`, `n0`), so a message named `Request`, `Object`, `Lock`, `Params`, or `Base` compiles. `Dispatch_*`, `Publish_*`, and `ServiceGen` members use the sanitized fully qualified name; when two types sanitize to the same name (`A.B_C` and `A_B.C`), both get a stable hash suffix (`Dispatch_global__A_B_C_1a2b3c4d`). Non-colliding names are unchanged.

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
