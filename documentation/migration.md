# Migration to 15.0

15.0 is a new major line. A 14.0.0 host does not compile against it until the signatures,
package ids, and removed types below are updated. The shape of the program stays a generated
mediator: `AddGeneratedMediator()`, member assemblies, and named pipelines.

The contract reference is [architecture.md](architecture.md). Diagnostics are
[diagnostics.md](diagnostics.md).

## Checklist

- The file that calls `AddGeneratedMediator()` imports the generated namespace. The default is `using TimeWarp.Mediator.Generated;`. A project that sets `TimeWarpMediatorNamespace` imports that namespace instead. Neither is an implicit using.
- `TimeWarpMediatorAssembly=false` is analyzer membership only. It does not stop emission. A library that must not emit a mediator does not reference `TimeWarp.Mediator.Generators`.
- A host that publishes, including DevTools, references `TimeWarp.Mediator` directly and does not set `PrivateAssets` on that reference. The TWM001–TWM004 analyzer does not flow from a dependency.
- Under `using static TimeWarp.Mediator.Unit`, an expression writes `Unit.ValueTask`. A bare `ValueTask` in an expression is CS0229. `ValueTask` in a signature still binds to `System.Threading.Tasks.ValueTask`.

## Packages

| 14.0.0 reference | 15.0 reference |
|---|---|
| `TimeWarp.Mediator.Contracts` | `TimeWarp.Mediator` |
| `TimeWarp.Mediator.Analyzers` | removed as a package. TWM001–TWM004 ship inside `TimeWarp.Mediator` |
| `TimeWarp.Mediator.Generators` | `TimeWarp.Mediator.Generators` at the same version as `TimeWarp.Mediator` |
| `TimeWarp.Mediator` (the reflection runtime) | removed. There is no `AddMediator` |

15.0 reuses the package id `TimeWarp.Mediator`. Through 14.0.0 that id was the reflection
runtime (`AddMediator`). The 14.0.0 contracts lived in `TimeWarp.Mediator.Contracts`, and
the analyzers were `TimeWarp.Mediator.Analyzers`.

A host references `TimeWarp.Mediator` and `TimeWarp.Mediator.Generators`. A library that
declares messages and does not emit a mediator references `TimeWarp.Mediator` only and keeps
`[assembly: MediatorAssembly]`.

`TimeWarp.Mediator` targets `net10.0` and `net11.0`. The generator is `netstandard2.0` because Roslyn loads
it. The Roslyn floor is `Microsoft.CodeAnalysis.CSharp` 5.0.0.

The host calls `AddGeneratedMediator()` from the generated namespace. That namespace is not an implicit using. Add this line in the file that calls the method:

```csharp
using TimeWarp.Mediator.Generated;
```

`TimeWarpMediatorNamespace` replaces `TimeWarp.Mediator.Generated` when it is set. The extension class is `internal` to the host project. The host supplies an
`IServiceProvider`. Reference `Microsoft.Extensions.DependencyInjection.Abstractions` or
TWM008 reports and nothing is generated.

```xml
<ItemGroup>
  <PackageReference Include="TimeWarp.Mediator" />
  <PackageReference Include="TimeWarp.Mediator.Generators" PrivateAssets="all" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
</ItemGroup>
```

Analyzers do not flow transitively. A project reports TWM001–TWM004 when that project references `TimeWarp.Mediator`. `PrivateAssets="all"` on `TimeWarp.Mediator` is not an analyzer-only switch. It also drops the runtime DLL from a packed nuspec and from `dotnet publish` of a host that received the package only through a dependency. Through 14.0.0 the contracts package flowed as a public dependency and the analyzers package carried `PrivateAssets="all"`. Keeping that flag on the combined 15.0 package hides `TimeWarp.Mediator.dll`.

| Package | Referenced by | PrivateAssets |
|---|---|---|
| `TimeWarp.Mediator` | A host, an app, or a library whose consumers need a runtime type (`ISender`, `Unit`, a published DevTools host) | none. `PrivateAssets="all"` removes the DLL from the nuspec and from the consumer's publish output |
| `TimeWarp.Mediator` | The project that should report TWM001–TWM004 | none, and the reference is direct. The analyzer does not flow from a library that references the package |
| `TimeWarp.Mediator.Generators` | The host that emits the mediator | `all` |
| `TimeWarp.Mediator.Generators` | A library | do not reference the package. A reference emits a mediator in that compilation |

## Handlers, behaviors, and Unit

Handler, notification-handler, behavior, `ISender`, and `IPublisher` members return
`ValueTask` or `ValueTask<T>`. An `async` method may return `ValueTask<T>` and keep its body.

```csharp
public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) =>
    ValueTask.FromResult("Pong");
}
```

`RequestHandlerDelegate<TResponse>` is `MessageHandlerDelegate<TRequest, TResponse>`. The
delegate takes the request. Call `next(request, cancellationToken)`.

```csharp
public ValueTask<TResponse> Handle(
    TRequest request,
    MessageHandlerDelegate<TRequest, TResponse> next,
    CancellationToken cancellationToken) =>
  next(request, cancellationToken);
```

`Unit.Task` is removed. Completed work is `Unit.ValueTask`. `Unit.Value` is unchanged.
`Unit.ValueTask` is a getter with no backing field:

```csharp
public static ValueTask<Unit> ValueTask => new(Unit.Value);
```

A static field of type `ValueTask<Unit>` on the `Unit` struct is a recursive type. The Mono WebAssembly loader throws `TypeLoadException` with the message "Recursive type definition detected" for `System.Threading.Tasks.ValueTask<Unit>` when that field exists. CoreCLR loads it. The getter returns a completed `ValueTask<Unit>` and does not allocate.

Under `using static TimeWarp.Mediator.Unit`, signatures are unaffected. `ValueTask` and
`ValueTask<T>` in type position still bind to `System.Threading.Tasks.ValueTask`. A bare
`ValueTask` in an expression is ambiguous (CS0229). Write `Unit.ValueTask` for the completed
value and `System.Threading.Tasks.ValueTask.FromResult` (or another static member) for the
type. The removed `Unit.Task` property followed the same rule: it did not hide
`System.Threading.Tasks.Task`. In a file that has `using static TimeWarp.Mediator.Unit`,
`ValueTask.FromResult` is the same ambiguous expression. Write
`System.Threading.Tasks.ValueTask.FromResult`.

## ValueTask ergonomics

`ValueTask<T>` has no implicit conversion from `Task<T>`. `ContinueWith` is a method on `Task`, not on `ValueTask`. `Task.WhenAll` takes tasks, so pass `AsTask()`.

```csharp
Task<string> task = Task.FromResult("pong");
ValueTask<string> wrapped = new(task);

Task<string> trimmed = task.ContinueWith(finished => finished.Result.Trim());

ValueTask<string> left = ValueTask.FromResult("a");
ValueTask<string> right = ValueTask.FromResult("b");
string[] both = await Task.WhenAll(left.AsTask(), right.AsTask());
```

## Send failures

Typed `Send` and `Dispatch_*` return the handler `ValueTask` directly.

- A null argument, `NoHandlerException`, and a handler that throws before it returns a
  `ValueTask` leave `Send` as a thrown exception.
- A handler that returns a faulted or canceled `ValueTask` stays faulted. `Send` does not
  throw that fault on the call.

14.x `async` `Send` captured a synchronous throw in the returned task. A caller that projects
`Send` inside `Enumerable.Select` (for example into `Task.WhenAll`) must not assume a
synchronous failure is captured. Catch around the call, or await `Send` directly, when a null
request or a missing handler is possible. `AsTask()` on a faulted `ValueTask` still faults the
task that `WhenAll` observes.

## Removed types

| Removed | Replacement |
|---|---|
| `ICommandHandler<>`, `ICommandHandler<,>`, `IQueryHandler<,>`, `IActionHandler<>`, `IIdempotentCommandHandler<>`, `IIdempotentCommandHandler<,>` | `IRequestHandler<,>` or `IRequestHandler<>`. `ActionHandler<TAction>` remains the base for an `IAction` |
| `NotificationHandler<T>` | `INotificationHandler<T>` |
| `IBaseRequest` | `IRequest` / `IRequest<TResponse>` |
| `IStreamRequest<>`, `IStreamRequestHandler<,>`, `IStreamPipelineBehavior<,>`, `StreamHandlerDelegate<>`, both `ISender.CreateStream` overloads | none. TWM009 is retired |
| `RequestHandlerDelegate<TResponse>` | `MessageHandlerDelegate<TRequest, TResponse>` |
| `MediatorAssembliesAttribute`, `MediatorModuleAttribute` | `[assembly: MediatorAssembly]` and `TimeWarpMediatorAssembly` |
| `TimeWarpMediatorProfile`, Aot and Link emission, `ServiceGen` | one emission. The host supplies `IServiceProvider`. Behaviors are woven |
| `Unit.Task`, `Unit : IComparable` | `Unit.ValueTask` |
| Interface-mapped handler and behavior descriptors | concrete registrations plus `MediatorManifest` |

`IActionHandler<TAction>` returned `Task`. The replacement is `ActionHandler<TAction>`. It implements `IRequestHandler<TAction>` and returns `ValueTask`:

```csharp
public abstract class ActionHandler<TAction> : IRequestHandler<TAction>
    where TAction : IAction
{
  public abstract ValueTask Handle(TAction request, CancellationToken cancellationToken);
}

public sealed class IncrementHandler : ActionHandler<Increment.Action>
{
  public override ValueTask Handle(Increment.Action request, CancellationToken cancellationToken) =>
    Unit.ValueTask;
}
```

`IRequestHandler<TAction>` with that same `ValueTask` method is the same contract when the handler does not use the base class.

`ICommand`, `IQuery<T>`, `IAction`, `IIdempotent`, and `IIdempotentCommand` / `IIdempotentCommand<T>`
stay as markers. Handlers implement `IRequestHandler`.

## Build properties and tooling

`TimeWarpMediatorProfile` is removed. Detection of a generated mediator is the assembly
attribute `[assembly: GeneratedMediator(typeof(Mediator))]`
(`GeneratedMediatorAttribute.MediatorType`).

`MediatorManifest.Version` is 2. The JSON document has no profile field and no `profile` key. A lookup of `profile` does not return `Aot`, `Link`, an empty string, or JSON null. Callers that switched on those names stop branching:

- There is one emission. Behaviors are woven, and the host supplies `IServiceProvider`.
- Detect the generated mediator with `[assembly: GeneratedMediator(typeof(Mediator))]` (`GeneratedMediatorAttribute.MediatorType`).
- `TimeWarpMediatorProfile` is not an MSBuild property.

Per-request kind, response, scope, and ordered behaviors are `MediatorManifest.Requests` /
`MediatorManifest.Request(displayName)`. Kind precedence is `IQuery`, then
`IIdempotentCommand`, then `ICommand`, then `IAction`, otherwise `Request`.

`TimeWarpMediatorNamespace` is the property the generator reads (default
`TimeWarp.Mediator.Generated`). `TimeWarpMediatorAssembly` and `TimeWarpMediatorGenerator`
are analyzer membership and diagnostic ownership. Setting `TimeWarpMediatorAssembly` to
`false` does not stop emission. A library that must not emit does not reference
`TimeWarp.Mediator.Generators`. Details are in [architecture.md](architecture.md).

`MediatorManifest` stays `internal` to the assembly that references Generators. A library cannot name the type. A public manifest would give a compilation that references two hosts two types with the same name (CS0436). The host reads the JSON and passes that string. TimeWarp.State's log and pipeline registry take the string. Each generating assembly has its own manifest, so a server host and a client host each pass their own JSON. There is no cross-assembly accessor.

```csharp
using TimeWarp.Mediator.Generated;

string manifestJson = MediatorManifest.Json;
```

## TimeWarp.State

State is already generator-first. The break is mechanical.

- The host file that calls `AddGeneratedMediator()` imports the generated namespace. It is not an implicit using:

```csharp
using TimeWarp.Mediator.Generated;
```

- `StateActionHandler<T>` keeps deriving from `ActionHandler<T>`. `ActionHandler<T>.Handle`
  returns `ValueTask`. The `Task` shim is gone. The method is `public abstract ValueTask Handle(TAction request, CancellationToken cancellationToken)`.
- These behaviors change
  `Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)`
  to
  `ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)`
  and call `next(request, cancellationToken)`:
  `StateInitializationPreProcessor<,>`, `StateTransactionBehavior<,>`, `TelemetryBehavior<,>`,
  `RenderSubscriptionsPostProcessor<,>`, `ActiveActionBehavior<,>`,
  `MultiTimerPostProcessor<,>`, `PersistentStatePostProcessor<,>`.
- `LogTimeWarpStateMiddleware` and `PipelineBehaviorRegistry` cannot name `MediatorManifest`. The host in the generating assembly passes `MediatorManifest.Json`. The library reads kind, scope, and `Behaviors` in pipeline order from that string. 15.0 does not register `IPipelineBehavior<,>` in
  the container, so an `IServiceCollection` scan for that interface is empty.
- The action-set generator's `await Sender.Send(new X.Action(...))` stays. `ValueTask` awaits
  the same way.
- Hand-written `ISender` fakes drop both `CreateStream` methods. `Send` returns `ValueTask`.
- Package references: Contracts plus Analyzers become one `TimeWarp.Mediator` reference, without `PrivateAssets`. Hosts keep `TimeWarp.Mediator.Generators` with `PrivateAssets="all"`. A published host such as DevTools references `TimeWarp.Mediator` directly. Libraries that must not emit a mediator do not
  reference the generator. See the PrivateAssets table above.
- `IAction`, `ISender<ClientPipeline>`, `IPublisher<ClientPipeline>`, `Send(object)` for
  JavaScript dispatch, and `AddGeneratedMediator<ClientPipeline>()` stay.

## TimeWarp.Architecture

Architecture is already generator-first. Contracts stay `IRequest<OneOf<...>>`. There is no
marker rename.

- The request handlers (about 62 `IRequestHandler<Command, OneOf<...>>` types) return
  `ValueTask<OneOf<...>>`. An `async` body can stay. The signature is the change.
- `FluentValidationBehavior` and `GenericPipelineBehavior` take
  `MessageHandlerDelegate<TRequest, TResponse>` and call `next(request, cancellationToken)`.
  The validation behavior builds its OneOf failure arm with reflective `FromT1`. This signature
  edit is the place to replace that call with a generated or constrained failure-arm helper.
- `BaseFastEndpoint` awaits a `ValueTask`. `await` is unchanged.
- `ScopedSender` implements the three `Send` methods and returns `ValueTask`. Both
  `CreateStream` stubs go away.
- Set `TimeWarpMediatorNamespace` when two hosts would emit `AddGeneratedMediator` into one
  compilation (the CS0121 wrappers around web-application and web-spa).
- Package references follow the table above. `[assembly: MediatorAssembly]`,
  `[MediatorScope(typeof(ClientPipeline))]`, and `[assembly: MediatorBehavior(..., Scope = ...)]`
  stay.

## TimeWarp.Nuru

- Nested endpoint `Handler` classes change `ICommandHandler<X, Unit>`,
  `IQueryHandler<X, Unit>`, and `IIdempotentCommandHandler<X, Unit>` to
  `IRequestHandler<X, Unit>`. The markers `ICommand<T>`, `IQuery<T>`, and
  `IIdempotentCommand<T>` stay. The handler returns `ValueTask<T>`.
- Replace `Unit.Task` with `Unit.ValueTask`. Nuru and the dev CLI use `using static TimeWarp.Mediator.Unit`. Signatures are unaffected. A bare `ValueTask` in an expression is ambiguous (CS0229). Write `Unit.ValueTask` for the value and `System.Threading.Tasks.ValueTask` for the type's statics:

```csharp
using static TimeWarp.Mediator.Unit;

public sealed class PingHandler : IRequestHandler<Ping, Unit>
{
  public ValueTask<Unit> Handle(Ping request, CancellationToken cancellationToken) =>
    Unit.ValueTask;
}
```
- Message-kind detection reads the manifest JSON the app passes (`KindQuery`, `KindIdempotentCommand`,
  `KindCommand`, `KindAction`, `KindRequest`). `MediatorManifest` is internal to the app assembly, so the Nuru library cannot name it. Precedence matches the old string matcher, including idempotent command
  before command. The document has no `profile` key. Do not switch on `Aot` or `Link`.
- The generated mediator always takes `IServiceProvider`, and `AddGeneratedMediator()` is
  always emitted for a graph member. `__GetMediator()` should construct a small
  `IServiceProvider` over Nuru's static graph and pass that provider to the generated
  `Mediator`, instead of building a hidden `ServiceCollection` only to reach MS.DI.
- Detect the generated mediator with `[assembly: GeneratedMediator(typeof(Mediator))]` on the
  app assembly. `TimeWarpMediatorProfile` is removed, so a check for the Aot or Link profile
  no longer says whether `AddGeneratedMediator` exists.
- Do not reference `TimeWarp.Mediator.Generators` from the Nuru library. A Generators
  reference emits a mediator in that compilation. `TimeWarpMediatorAssembly=false` does not
  stop emission. Reference Generators from the app, and call `AddGeneratedMediator()` there.
  The library references `TimeWarp.Mediator` and uses `[assembly: MediatorAssembly]` when its
  messages belong in the app graph.
- Package references: `TimeWarp.Mediator` plus `TimeWarp.Mediator.Generators`. The separate
  analyzers package id is gone.

## From MediatR

Change `using MediatR;` to `using TimeWarp.Mediator;`. The host references the two 15.0
packages and calls `AddGeneratedMediator()`. There is no reflection `AddMediator`.
