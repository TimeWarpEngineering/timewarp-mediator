#region Purpose
// Per-scope pipeline behaviors that must not run on the other pipeline.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.Pipelines;

public sealed class ClientStampBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public static List<string> Events { get; } = new();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Events.Add("client-stamp");
        return await next(cancellationToken).ConfigureAwait(false);
    }
}

public sealed class ServerStampBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public static List<string> Events { get; } = new();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Events.Add("server-stamp");
        return await next(cancellationToken).ConfigureAwait(false);
    }
}
