#region Purpose
// One IAction and three open-generic behaviors for the State-shaped pipeline row.
#endregion

#region Design
// Behaviors are constrained to IAction and scoped to ClientPipeline, so they do not wrap
// unscoped Ping/Pong or the scoped Ping that has no behaviors. Each behavior only awaits
// next: the row measures mediator weave, not a state clone. The handler returns a completed
// ValueTask, so a 15.0 allocation-free weave would show 0 B on this row.
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

[MediatorScope(typeof(ClientPipeline))]
public sealed class Increment : IAction
{
  public int Amount { get; set; } = 1;
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class IncrementHandler : ActionHandler<Increment>
{
  public override ValueTask Handle(Increment request, CancellationToken cancellationToken)
  {
    return default;
  }
}

public sealed class RenderBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAction
{
  public async ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken)
  {
    return await next(request, cancellationToken).ConfigureAwait(false);
  }
}

public sealed class ExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAction
{
  public async ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken)
  {
    return await next(request, cancellationToken).ConfigureAwait(false);
  }
}

public sealed class StateTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAction
{
  public async ValueTask<TResponse> Handle(
      TRequest request,
      MessageHandlerDelegate<TRequest, TResponse> next,
      CancellationToken cancellationToken)
  {
    return await next(request, cancellationToken).ConfigureAwait(false);
  }
}
