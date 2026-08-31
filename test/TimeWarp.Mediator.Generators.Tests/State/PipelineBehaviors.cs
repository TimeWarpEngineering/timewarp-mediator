#region Purpose
// Tracking and short-circuit behaviors that prove generated order matches Reverse().Aggregate.
#endregion

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace TimeWarp.Mediator.Generators.Tests.State;

public static class PipelineLog
{
    public static List<string> Events { get; } = new();

    public static void Clear()
    {
        Events.Clear();
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
