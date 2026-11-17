# TimeWarp.Mediator diagnostics

`TimeWarp.Mediator` ships the analyzer (TWM001–TWM004, TWM012, and TWM013) as an analyzer asset. A library that
references that package and does not run the generator gets TWM001–TWM004, and TWM012 when a
`GeneratedHandlerAttribute` name is not a constant. `TimeWarp.Mediator.Generators`
reports TWM007, TWM008, TWM010, and TWM011, and in a generator host it also reports TWM001–TWM003
and an unreadable TWM012. The analyzer skips TWM001–TWM003 when `TimeWarpMediatorGenerator=true`,
so each is reported once. It still reports TWM012 there when the named handler does not exist
after every generator has run. TWM013 is split the same way in a host: the generator reports a
`GeneratedRequestPatternAttribute` it cannot read or expand, and the analyzer reports a declared
request or handler that is missing or does not match after every generator has run.
TWM004 stays with the analyzer in both hosts and libraries.
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
| [TWM012](#twm012) | Error | analyzer (missing type or not an `IRequestHandler`, host); generator (unreadable name or not a type name); analyzer (unreadable name, library) | Declared handler does not exist |

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

The name has to be a constant string. In the compilation that declares the attribute, a getter
that returns a string literal is enough. An attribute declared in another assembly is read from
a constructor parameter named `handlerTypeName`. A default value is enough:

```csharp
public sealed class ReducerAttribute(string handlerTypeName = "Handler") : GeneratedHandlerAttribute
{
  public override string HandlerTypeName => handlerTypeName;
}
```

A ProjectReference hands the compiler a reference assembly. That image does not keep the getter
body, so a getter-only attribute is not a constant. Any other string constructor parameter is
ignored. That failure is TWM012 on the attribute, reported by the generator in a host and by the
analyzer in a library. The message says the name is not a constant string and names
`handlerTypeName`. A relative name that is not a type name uses the same id and says it is not
a type name relative to the request's containing type.

When the name is a constant and the type still does not exist after every generator has run,
the analyzer reports TWM012 on the attribute in a host. A library does not: the host is where
the other generator emits the handler, and reporting it in the library would be a false
positive. The generated mediator still names the handler, so a missing type is also `CS0234`
when that parent namespace exists, or `CS0246` when it does not, inside `Mediator.g.cs`.
TWM012 is the diagnostic on the request. Requests without the attribute
are unchanged. A declared name that exists and does not implement the request's
`IRequestHandler` is TWM012. The message names that interface. A declared name that is the same
type as the request's `IRequestHandler` is one handler. A different handler type is TWM002.

## TWM013

**Declared generated request does not match.** A source type carries a
`GeneratedRequestPatternAttribute` (or a derived attribute). It declares, per contract type, a
request and a handler that another generator emits into the host, and the request interface that
request implements. The generated mediator emits a `Send` arm from those names without seeing the
types, so drift between the declaration and the other generator shows up here.

```csharp
[GeneratedRequestPattern(
  "{Host}.{Contract}ActionSet.Action",
  "{Host}.{Contract}ActionSet.Handler",
  "TimeWarp.Mediator.IRequest<{ContractType}.Response>",
  typeof(CreateRole), typeof(UpdateRole))]
public partial class RoleState { }
```

The generator reports TWM013 on the attribute when it cannot read or expand the pattern:

- A template is not a constant. Each template comes from a constructor parameter with the same
  name (`requestTypeName`, `handlerTypeName`, `requestInterfaceName`; a default value is enough)
  or from a `const string` field named `RequestTypeName`, `HandlerTypeName`, or
  `RequestInterfaceName` on the attribute class. The contracts come from a constructor parameter
  named `contracts`. A getter body does not survive in a reference assembly, so properties are
  not read.
- A template uses a placeholder other than `{Host}`, `{Contract}`, or `{ContractType}`, or `{Host}`
  is a generic type.
- A contract is not a non-generic named type.
- The request or handler name does not expand to a dotted type name.
- The request interface does not resolve to an interface in the compilation, or it is not an
  `IRequest` or `IRequest<T>`. The interface and its response must be visible to the generator:
  in source or in a reference. Only the request and the handler may come from another generator.

After every generator has run, the analyzer reports TWM013 on the attribute in a host when:

- the declared request does not exist,
- the declared handler does not exist,
- the request does not implement the declared interface (a different response is this case), or
- the handler does not implement `IRequestHandler` for that request and response. The message
  names the interface.

A missing request or handler is also `CS0234` or `CS0246` inside `Mediator.g.cs`. TWM013 is the
diagnostic that names the declaration. A declared request is never TWM001. A declared handler
that is the same type as a visible `IRequestHandler` for that request is one handler. A
different handler type is TWM002.
