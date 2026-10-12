#region Purpose
// Tracking and short-circuit behaviors that prove generated order matches Reverse().Aggregate.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.State;

// Per async flow, not process-wide: every unscoped send runs the global tracking behaviors, so a
// static list would couple test classes by execution order. Clear() (synchronous) starts a fresh
// log for the calling test; behaviors awaited from that test then write into it.
public static class PipelineLog
{
    private static readonly AsyncLocal<List<string>?> Current = new();

    public static List<string> Events => Current.Value ??= new List<string>();

    public static void Clear()
    {
        Current.Value = new List<string>();
    }
}

public sealed class OuterTrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        PipelineLog.Events.Add("outer-before");
        TResponse response = await next(cancellationToken).ConfigureAwait(false);
        PipelineLog.Events.Add("outer-after");
        return response;
    }
}

public sealed class InnerTrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        PipelineLog.Events.Add("inner-before");
        TResponse response = await next(cancellationToken).ConfigureAwait(false);
        PipelineLog.Events.Add("inner-after");
        return response;
    }
}

public sealed class ShortCircuitBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAction
{
    public static bool ShortCircuit { get; set; }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (ShortCircuit)
        {
            PipelineLog.Events.Add("short-circuit");
            return Task.FromResult(default(TResponse)!);
        }

        return next(cancellationToken);
    }
}

public sealed class UnitOnlyBehavior<TRequest> : IPipelineBehavior<TRequest, Unit>
    where TRequest : notnull
{
    public Task<Unit> Handle(
        TRequest request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        return next(cancellationToken);
    }
}

public sealed class Ping : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        PipelineLog.Events.Add("handler");
        return Task.FromResult(request.Message + "-pong");
    }
}

public sealed class ReentrantAction : IRequest<int>
{
    public int Value { get; set; }
}

public sealed class ReentrantHandler : IRequestHandler<ReentrantAction, int>
{
    private readonly ISender Sender;

    public ReentrantHandler(ISender sender)
    {
        Sender = sender;
    }

    public async Task<int> Handle(ReentrantAction request, CancellationToken cancellationToken)
    {
        if (request.Value == 0)
        {
            int inner = await Sender.Send(new ReentrantAction { Value = 1 }, cancellationToken).ConfigureAwait(false);
            return inner + 1;
        }

        return request.Value;
    }
}
