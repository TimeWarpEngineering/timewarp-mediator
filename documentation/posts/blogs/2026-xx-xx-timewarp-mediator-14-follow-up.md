# One behavior, across the Mediator 14.0 break

Unpublished draft. Post this 2–5 days after the launch post, only after Steven says so.
Intended URL, once the month and day replace `xx`:

https://thefreezeteam.com/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-mediator-14-follow-up

Launch post:
https://thefreezeteam.com/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-mediator-14-release

Same card: `documentation/posts/assets/timewarp-mediator-14-card.png`.

The launch post said `AddMediator()` does not become `AddGeneratedMediator()` by itself. Here
is the same logging behavior on each side. Both snippets are the Host profile. The Aot profile
does not weave `[assembly: MediatorBehavior]` (TWM005). If the handler needs constructor
injection, stay on Host.

## Before: 13.x / 14.x reflection

```csharp
public sealed class Ping : IRequest<string>
{
  public string Message { get; set; } = "";
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult(request.Message);
}

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    return next(cancellationToken);
  }
}

services.AddMediator(cfg =>
{
  cfg.RegisterServicesFromAssemblyContaining<PingHandler>();
  cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
```

The assembly scan finds `PingHandler`. The open behavior is closed over each request at
runtime. This is the path that warns under trimming and NativeAOT.

## After: 14.0 generated

```csharp
[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(LoggingBehavior<,>))]

public sealed class Ping : IRequest<string>
{
  public string Message { get; set; } = "";
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
  public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
    Task.FromResult(request.Message);
}

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    return next(cancellationToken);
  }
}

services.AddGeneratedMediator();
```

The behavior class did not change. Registration did. The host references
`TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators` at 14.0.0. Drop
`TimeWarp.Mediator` once this is the only call. A missing `PingHandler` is now TWM001 at
build time.

Two things this snippet does not do:

- It does not open a stream, run a pre/post processor, or run an exception handler. Those stay
  on `AddMediator()`, or you fold them into the request handler or the behavior first.
  Notifications are fine: `Publish` runs on the generated mediator too.
- It does not outrun martinothamar's generator. The 2026-10-12 baseline still has that `Send`
  at 8.8 ns and 24 B, against our generated `Send` at 29.9 ns and 96 B. The win against our
  own reflection `Send` (69.0 ns, 224 B) is the one the launch card quotes.

Matrix:
[migration.md](https://github.com/TimeWarpEngineering/timewarp-mediator/blob/master/migration.md#13x-reflection-to-14x-generated).
