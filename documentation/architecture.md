# Architecture

15.0 has one generated dispatcher and two packages. The host supplies an
`IServiceProvider`. Pipeline behaviors are woven. There is no reflection dispatcher and no
Host, Aot, or Link profile.

## Packages

| Package | Project | Target | What a reference does |
|---|---|---|---|
| `TimeWarp.Mediator` | `source/timewarp-mediator` | `net10.0;net11.0` | Contracts, `Unit`, membership attributes, `GeneratedMediatorAttribute`, and the TWM001–TWM004 analyzer DLL at `analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll` |
| `TimeWarp.Mediator.Generators` | `source/timewarp-mediator-generators` | `netstandard2.0` | Source generator. Emits the dispatcher, `MediatorManifest`, and `AddGeneratedMediator`. Reports TWM007, TWM008, TWM010, and TWM011, and TWM001–TWM003 in a generator host |

The analyzer project `source/timewarp-mediator-analyzers` stays in the tree because the analyzer
is `netstandard2.0` and the contracts library is `net10.0` and `net11.0`. It is not a NuGet package.
`TimeWarp.Mediator.Generators` does not bundle that analyzer DLL. A library that only declares
messages references `TimeWarp.Mediator` and gets TWM001–TWM004. A host references both packages
at the same version.

Generators and the analyzer load inside Roslyn. The floor is `Microsoft.CodeAnalysis.CSharp`
5.0.0, the compiler that ships with the .NET 10 SDK.

## Contracts

Everything a caller awaits is a `ValueTask` or a `ValueTask<T>`.

| Kind | Types |
|---|---|
| Messages | `IRequest`, `IRequest<TResponse>`. Markers: `IAction : IRequest`, `ICommand : IRequest`, `ICommand<T> : IRequest<T>`, `IQuery<T> : IRequest<T>, IIdempotent`, `IIdempotentCommand : ICommand, IIdempotent`, `IIdempotentCommand<T> : ICommand<T>, IIdempotent`, `INotification` |
| Handlers | `IRequestHandler<TRequest, TResponse>`, `IRequestHandler<TRequest>`, `INotificationHandler<TNotification>`. Base `ActionHandler<TAction>` for a void `IAction` |
| Pipeline | `MessageHandlerDelegate<TRequest, TResponse>`, `IPipelineBehavior<TRequest, TResponse>` |
| Dispatch | `ISender`, `IPublisher`, `IMediator : ISender, IPublisher`, and `ISender<TScope>` / `IPublisher<TScope>` |
| Membership | `[assembly: MediatorAssembly]`, `[MediatorScope(typeof(TScope))]`, `[assembly: MediatorBehavior(typeof(B<,>), order)]` with optional `Scope` |
| Tooling | `[assembly: GeneratedMediator(typeof(Mediator))]` emitted on the host. `Unit` (`Unit.Value`, `Unit.ValueTask`). `NoHandlerException` |

Markers do not change dispatch. A command, a query, an action, and an idempotent command all
go through `IRequestHandler`. Idempotency is a declared contract: there is no idempotency key
and no dedup store. The message kind is data on `MediatorManifest` for tooling.

`IRequestHandler<TRequest, TResponse>.Handle` returns `ValueTask<TResponse>`.
`IRequestHandler<TRequest>.Handle`, `ActionHandler<TAction>.Handle`, and
`INotificationHandler<T>.Handle` return `ValueTask`. A behavior's `Handle` returns
`ValueTask<TResponse>` and calls `next(request, cancellationToken)`.

`Unit.Value` is the only `Unit` value. `Unit.ValueTask` is a cached completed
`ValueTask<Unit>`. `Unit` does not implement `IComparable`.

## Generated dispatcher

The generator emits one `internal sealed` `Mediator` per host in the generated namespace
(default `TimeWarp.Mediator.Generated`). It implements `IMediator` and takes
`IServiceProvider`. A named pipeline emits `Sender_{Scope}` and `Publisher_{Scope}` in that
same namespace. Every generated type is `internal`.

`Send<TResponse>(IRequest<TResponse>)` and `Send<TRequest>(TRequest)` where
`TRequest : IRequest` are type switches. Each arm returns the `ValueTask` from `Dispatch_*`
with no box. `Send(object)` and `Publish(object)` are the boxing paths.

A response arm reinterprets `ValueTask<TConcrete>` as `ValueTask<TResponse>` only when those
types are the same. A covariant send, or a request that implements two `IRequest<>` closures,
awaits the concrete task and casts the result. A mismatch throws `InvalidCastException`.

Typed `Send` and `Dispatch_*` return the handler `ValueTask` directly:

- A null argument, `NoHandlerException`, and a handler that throws before it returns a
  `ValueTask` leave `Send` as a thrown exception.
- A handler that returns a faulted or canceled `ValueTask` stays faulted. `Send` does not
  throw that fault synchronously.

A request with behaviors resolves one scoped chain and invokes it. The chain builds its
`MessageHandlerDelegate<TRequest, TResponse>` steps once per scope and resolves each behavior
and the handler from that scope. A behavior may await before `next` and may call `next` again.
A send does not allocate a closure for the chain. Two scopes can send the same request at the
same time.

A request with no behaviors uses a scoped `HandlerCache` on the dispatcher. Each handler slot
stays empty until that request is sent, then `GetRequiredService` fills it once for the scope.
A handler that cannot be constructed fails only its own send. An unsent request does not
construct its handler. A transient registration is still one instance per scope. Disposing the
scope disposes the cache, and the next send throws `ObjectDisposedException`.

`Publish<TNotification>` stays typed and awaits every assignable handler sequentially, most
derived notification type first, then each type's handlers in display-name order. A handler of
a base notification or of `INotification` runs for a derived notification. There is no
pluggable notification publisher.

`Send(object)` orders request cases most-derived first. A runtime type with no handler of its
own dispatches through the first bound supertype in that order.

Handler discovery:

- A class that implements several handler interfaces is bound once per interface. The concrete
  type is registered once. When a class implements both `IRequestHandler<T, TResponse>` and
  `IRequestHandler<T>` for the same `T`, the two-arity binding wins.
- A concrete non-generic base that already handles an interface is not bound again on a derived
  class. Abstract and open-generic bases are not bound, so each closed derived class binds.
- Member assemblies are walked in ordinal order of assembly name. Handler lists are sorted by
  display name. That order is the notification order and the order in a TWM002 message.

The AOT sample (`samples/timewarp-mediator-examples-aot`) publishes with `PublishAot`,
`IsAotCompatible`, and trim and AOT analyzers as errors. It calls `AddGeneratedMediator()`,
resolves `IMediator` from a scope, and runs a pipeline behavior. Behaviors are woven on that
path.

## Registration

`AddGeneratedMediator()` and `AddGeneratedMediator<TScope>()` are extension methods on an
`internal` class in the host's generated namespace. The default class name is
`TimeWarpMediatorGeneratedServiceCollectionExtensions`. The host file that calls them needs
`using TimeWarp.Mediator.Generated` (or the namespace set by `TimeWarpMediatorNamespace`).

`AddGeneratedMediator()` registers:

- the dispatcher as transient, also as `IMediator`, `ISender`, and `IPublisher`;
- each concrete handler type and each closed behavior type as scoped;
- a scoped chain type for each request that has behaviors;
- a scoped `HandlerCache` when any request has no behaviors.

It does not register `IRequestHandler<>`, `IPipelineBehavior<,>`, or `INotificationHandler<>`.
`AddGeneratedMediator<TScope>()` does the same for `Sender_{Scope}` / `Publisher_{Scope}` and
`ISender<TScope>` / `IPublisher<TScope>`. An unknown `TScope` throws `ArgumentException`.

Resolve `ISender` from an `IServiceScope`. Handlers and the cache are scoped. The dispatcher
uses the provider it was resolved from.

## MediatorManifest

The same generated namespace contains `internal static class MediatorManifest`.

- `Version` is `2`. The document has no profile field. A reader that still expects a profile
  sees version 2 instead of a silent version 1.
- `Json` is the embedded document (a source generator cannot write a loose
  `mediator.manifest.json`). It lists `requests`, `notifications`, and `membership`.
- `Requests` is an `IReadOnlyList<RequestEntry>`. `Request(string requestType)` looks up one
  entry by ordinal display name and throws `ArgumentException` when the name is missing.
- `RequestEntry` carries `Request`, `Response`, `Kind`, `Scope` (null for the unscoped
  pipeline), and `Behaviors` (closed behavior display names, pipeline order). `Requests` and
  each entry's `Behaviors` are read-only. A cast does not expose a mutable array.
- Kind constants: `KindQuery`, `KindIdempotentCommand`, `KindCommand`, `KindAction`,
  `KindRequest`. Precedence is query, then idempotent command, then command, then action,
  otherwise request. `IIdempotentCommand` also implements `ICommand` and is reported as
  `IdempotentCommand`.

Notification handlers are in the JSON `notifications` array. They are not `RequestEntry`
values.

The host assembly is marked
`[assembly: GeneratedMediator(typeof(global::….Mediator))]`. `GeneratedMediatorAttribute.MediatorType`
is the generated mediator type. Tooling reads that attribute. It does not read an MSBuild
profile.

## Membership

A project that references `TimeWarp.Mediator.Generators` emits a mediator for that
compilation. The generator includes the host and emits unless TWM008 stops it because a
runtime assembly is missing. It does not read `TimeWarpMediatorAssembly`. A library avoids
emission by not referencing Generators. It opts into a host's graph with
`[assembly: MediatorAssembly]`.

The generator package sets `TimeWarpMediatorAssembly` to `true` unless the project sets it
to `false`. The analyzer reads that property. `true`, or `[assembly: MediatorAssembly]`, puts
the compilation in the analyzer's member set. `false` does not stop the generator from
emitting.

`[MediatorScope(typeof(TScope))]` on a type, a containing type, or the assembly assigns that
pipeline. A type-level marker wins. Types with no marker join the unscoped default pipeline.
`AddGeneratedMediator()` dispatches only that default. Register
`AddGeneratedMediator<ClientPipeline>()` for the named one. Worked example:
[m2-named-pipelines.md](m2-named-pipelines.md).

`[assembly: MediatorBehavior(typeof(Behavior<,>), order)]` weaves a behavior. A lower `Order`
is outermost. Declaration order breaks ties: among equal `Order` values, the first attribute
is outermost. Omitted `Scope` means the unscoped pipeline only. A scoped pipeline does not
run unscoped behaviors.
Open generics close per request when the constraints match. A type that is not an
`IPipelineBehavior<,>` is TWM010 and is dropped. A behavior that closes over no request in its
pipeline is TWM011.

Build properties:

| Property | Read by | Meaning |
|---|---|---|
| `TimeWarpMediatorNamespace` | generator | Generated namespace. Default `TimeWarp.Mediator.Generated`. Set a distinct value on each host that shares a compilation |
| `TimeWarpMediatorAssembly` | analyzer | Analyzer membership. Default `true` on a generator reference. Does not control emission |
| `TimeWarpMediatorGenerator` | analyzer | `true` on a generator reference. The analyzer then skips TWM001–TWM003 so the generator reports them once. TWM004 stays with the analyzer |

`TimeWarpMediatorProfile` is removed. The host assembly name and public key come from the
compilation, so a strong-named `InternalsVisibleTo` grant still matches the host.

## Diagnostics

Ids and ownership: [diagnostics.md](diagnostics.md).

| Id | Reported by | Meaning |
|---|---|---|
| TWM001 | generator in a host, analyzer in a library | Request has no handler |
| TWM002 | generator in a host, analyzer in a library | Duplicate handler |
| TWM003 | generator in a host, analyzer in a library | Handler and message name different pipelines |
| TWM004 | analyzer | `Send` targets a request from another pipeline |
| TWM007 | generator | Member-assembly type is inaccessible |
| TWM008 | generator | `TimeWarp.Mediator` or `Microsoft.Extensions.DependencyInjection.Abstractions` is missing. Nothing is generated |
| TWM010 | generator | `[MediatorBehavior]` type is not a pipeline behavior |
| TWM011 | generator | Behavior matches no request in its pipeline |

TWM005 and TWM006 described the removed Aot and Link profiles. TWM009 described stream,
processor, and exception-handler kinds. Those three ids are retired.

## Benchmarks

The comparison baseline re-recorded on task 021-003 is
[benchmarks/comparison-baseline.md](benchmarks/comparison-baseline.md) (commit
`c5e4a3a6a6155b6f5ed322379a59559be58fc69a`). The recorded Ratio column still uses the removed
reflection row `TimeWarpLegacyMakeGenericType` as 1.00.

| Row | Mean | Allocated |
|---|---|---|
| `TimeWarpGeneratedInterfaceSend` | 8.243 ns | 24 B |
| `TimeWarpGeneratedInterfaceSendUnit` | 9.360 ns | 0 B |
| `TimeWarpGeneratedStatePipeline` | 131.117 ns | 0 B |
| `MartinothamarGenerated` | 8.314 ns | 24 B |

`./bin/dev benchmark --project comparison --gate` fails when a `TimeWarp*` row exceeds that
file. The gate is not part of `./bin/dev workflow`.
