# TimeWarp.Mediator diagnostics

The analyzer package reports TWM001–TWM004. The generator package also reports TWM005–TWM011,
which describe what the generated mediator can and cannot emit. Each descriptor's help link
points to its section here.

| Id | Severity | Reported by | Summary |
|----|----------|-------------|---------|
| [TWM001](#twm001) | Error | analyzer, generator | Request has no handler |
| [TWM002](#twm002) | Error | analyzer, generator | Duplicate handler for request |
| [TWM003](#twm003) | Error | analyzer, generator | Handler and message belong to different pipelines |
| [TWM004](#twm004) | Error | analyzer | Request is not a member of this sender pipeline |
| [TWM005](#twm005) | Warning | generator (Aot/Link) | Pipeline behavior is not woven under the Aot profile |
| [TWM006](#twm006) | Error | generator (Aot/Link) | Aot handler is not constructible with `new()` |
| [TWM007](#twm007) | Error | generator | Member assembly type is not accessible from the host |
| [TWM008](#twm008) | Error | generator | Generated mediator dependencies are not referenced |
| [TWM009](#twm009) | Warning | generator | Handler kind is not dispatched by the generated mediator |
| [TWM010](#twm010) | Warning | generator | Registered behavior is not a pipeline behavior |
| [TWM011](#twm011) | Warning | generator | Registered behavior applies to no request |

## TWM001

**Request has no handler.** A concrete request in a member assembly has no `IRequestHandler`.
Add one handler, or remove the request.

## TWM002

**Duplicate handler for request.** More than one class handles the same request. The message
lists them in ordinal display-name order and points at the second. Keep exactly one.

## TWM003

**Handler and message belong to different pipelines.** The handler and its request or
notification both resolve a `[MediatorScope(typeof(TScope))]` marker (on the type, a containing
type, or the assembly), and the markers differ. If only one side is scoped, the pair joins that
pipeline. If neither is, it joins the unscoped default pipeline. Make both name the same marker,
or scope only one side.

## TWM004

**Request is not a member of this sender pipeline.** `ISender<TScope>.Send` only dispatches
requests assigned to `TScope`. Detection covers `sender.Send(x)`, `sender?.Send(x)`, and
receivers that implement `ISender<TScope>`. Only calls that bind to `ISender.Send` count; other
methods named `Send` are ignored. In a generator host (`TimeWarpMediatorAssembly=true`) two more
cases are reported:

- an unscoped `ISender`/`IMediator` sending a scoped request (the generated `Mediator` only
  dispatches unscoped requests);
- an argument typed as an interface or abstract class (`IRequest<int>`) when every bound request
  it could be belongs to another pipeline.

A library sees only part of the graph, so it skips those two. `Send(object)` is never reported.
It throws `NoHandlerException` at runtime.

## TWM005

**Pipeline behavior is not woven under the Aot profile.** Aot/Link dispatch calls handlers
directly from static `ServiceGen` fields. `[assembly: MediatorBehavior]` registrations do not run
and the manifest omits them. Use the Host profile for cross-cutting behaviors.

## TWM006

**Aot handler is not constructible with `new()`.** Aot/Link creates each handler once with
`new Handler()`. Give it an accessible constructor with no required arguments. If it has
`required` members, mark that constructor `[SetsRequiredMembers]`.

## TWM007

**Member assembly type is not accessible from the host.** A handler, request, response,
notification, or behavior in a referenced member assembly is `internal` (or otherwise
inaccessible) and the host has no `InternalsVisibleTo`. Generated code would be `CS0122`, so the
type is not bound and its request is not also TWM001. Make the type public, or add
`[assembly: InternalsVisibleTo("HostAssembly")]` to the member assembly.

## TWM008

**Generated mediator dependencies are not referenced.** The generator needs
`TimeWarp.Mediator.IMediator` (TimeWarp.Mediator.Contracts) and, under the Host profile,
`Microsoft.Extensions.DependencyInjection.Abstractions`. When one is missing it generates
nothing and reports this once, instead of `CS0234`/`CS0246` inside `.g.cs` files. Add the
reference, or set `TimeWarpMediatorProfile` to `Aot` when you do not use MS.DI.

## TWM009

**Handler kind is not dispatched by the generated mediator.** A class in a member assembly
implements `IStreamRequestHandler`, `IStreamPipelineBehavior`, `IRequestPreProcessor`,
`IRequestPostProcessor`, `IRequestExceptionHandler`, or `IRequestExceptionAction`. The generated
mediator never calls it, and `CreateStream` throws `NotSupportedException`. Use the
reflection-based `AddMediator()` runtime for these, or move the logic into a request handler or
an `IPipelineBehavior`.

## TWM010

**Registered behavior is not a pipeline behavior.** `[assembly: MediatorBehavior(typeof(T))]`
names a type that does not implement `IPipelineBehavior<TRequest, TResponse>`. It is dropped
from every pipeline.

## TWM011

**Registered behavior applies to no request.** The behavior is valid, but no request in its
pipeline (the `Scope` named argument, or the unscoped default) satisfies its generic
constraints, so it never runs. Fix the constraints or the `Scope`, or remove the registration.
