# TimeWarp.Mediator diagnostics

`TimeWarp.Mediator` ships the analyzer (TWM001–TWM004 and TWM012) as an analyzer asset. A library that
references that package and does not run the generator gets TWM001–TWM004, and TWM012 when a
`GeneratedHandlerAttribute` name is not a constant. `TimeWarp.Mediator.Generators`
reports TWM007, TWM008, TWM010, and TWM011, and in a generator host it also reports TWM001–TWM003
and an unreadable TWM012. The analyzer skips TWM001–TWM003 when `TimeWarpMediatorGenerator=true`,
so each is reported once. It still reports TWM012 there when the named handler does not exist
after every generator has run. TWM004 stays with the analyzer in both hosts and libraries.
Each descriptor's help link points to its section here.

There is one emission. The host supplies `IServiceProvider`, and registered pipeline behaviors are
woven. TWM005 (behavior not woven) and TWM006 (handler not constructible with `new()`) described
the removed Aot and Link profiles. TWM009 described stream, processor, and exception-handler kinds
that 15.0 no longer ships. Those three ids are retired.

| Id | Severity | Reported by | Summary |
|----|----------|-------------|---------|
| [TWM001](#twm001) | Error | generator (host), analyzer (library) | Request has no handler |
| [TWM002](#twm002) | Error | generator (host), analyzer (library) | Duplicate handler for request |
| [TWM003](#twm003) | Error | generator (host), analyzer (library) | Handler and message belong to different pipelines |
| [TWM004](#twm004) | Error | analyzer | Request is not a member of this sender pipeline |
| [TWM007](#twm007) | Error | generator | Member assembly type is not accessible from the host |
| [TWM008](#twm008) | Error | generator | Generated mediator dependencies are not referenced |
| [TWM010](#twm010) | Warning | generator | Registered behavior is not a pipeline behavior |
| [TWM011](#twm011) | Warning | generator | Registered behavior applies to no request |
| [TWM012](#twm012) | Error | analyzer (missing type, host); generator (unreadable name); analyzer (unreadable name, library) | Declared handler does not exist |

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
methods named `Send` are ignored. In a generator host (`TimeWarpMediatorGenerator=true` or `TimeWarpMediatorAssembly=true`) two more
cases are reported:

- an unscoped `ISender`/`IMediator` sending a scoped request (the generated `Mediator` only
  dispatches unscoped requests);
- an argument typed as an interface or abstract class (`IRequest<int>`) when every bound request
  it could be belongs to another pipeline.

A library sees only part of the graph, so it skips those two. `Send(object)` is never reported.
It throws `NoHandlerException` at runtime.

## TWM007

**Member assembly type is not accessible from the host.** A handler, request, response,
notification, behavior, or pipeline scope marker in a referenced member assembly is `internal`
(or otherwise inaccessible) and the host has no `InternalsVisibleTo`. Generated code would be
`CS0122`, so the type is not bound and its request is not also TWM001. Make the type public, or add
`[assembly: InternalsVisibleTo("HostAssembly")]` to the member assembly.

## TWM008

**Generated mediator dependencies are not referenced.** The generator emits code against
`TimeWarp.Mediator` (`IMediator`) and `Microsoft.Extensions.DependencyInjection.Abstractions`
(`IServiceCollection`, `ServiceCollectionServiceExtensions`, `ServiceProviderServiceExtensions`).
When any of those types is missing it generates nothing and reports this once, instead of
`CS0234`/`CS0246` inside `.g.cs` files. Reference the `TimeWarp.Mediator` package and
`Microsoft.Extensions.DependencyInjection.Abstractions`.

## TWM010

**Registered behavior is not a pipeline behavior.** `[assembly: MediatorBehavior(typeof(T))]`
names a type that does not implement `IPipelineBehavior<TRequest, TResponse>`. It is dropped
from every pipeline.

## TWM011

**Registered behavior applies to no request.** The behavior is valid, but no request in its
pipeline (the `Scope` named argument, or the unscoped default) satisfies its generic
constraints, so it never runs. Fix the constraints or the `Scope`, or remove the registration.

## TWM012

**Declared handler does not exist.** A request carries a `GeneratedHandlerAttribute` (or a
derived attribute) whose `HandlerTypeName` names the handler another generator emits. The name
is relative to the request's containing type, or to its namespace when the request is top-level.
`Handler` on a request nested in `CounterState` names `CounterState.Handler`.

The name has to be a constant string: a constructor argument (including a default parameter) or
a getter that returns a string literal. Anything else is TWM012 on the attribute, reported by
the generator in a host and by the analyzer in a library. The message names the attribute
property and says it is not a constant string. A relative name that is not a type name says so
in the same way.

When the name is a constant and the type still does not exist after every generator has run,
the analyzer reports TWM012 on the attribute in a host. A library does not: the host is where
the other generator emits the handler, and reporting it in the library would be a false
positive. The generated mediator still names the handler, so a missing type is also `CS0234`
when that parent namespace exists, or `CS0246` when it does not, inside `Mediator.g.cs`.
TWM012 is the diagnostic on the request. Requests without the attribute
are unchanged. A declared name that is the same type as the request's `IRequestHandler` is one
handler. A different type is TWM002.
