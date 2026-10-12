// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Wrappers;

internal abstract class StreamRequestHandlerBase
{
    public abstract IAsyncEnumerable<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal abstract class StreamRequestHandlerWrapper<TResponse> : StreamRequestHandlerBase
{
    public abstract IAsyncEnumerable<TResponse> Handle(
        IStreamRequest<TResponse> request, 
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class StreamRequestHandlerWrapperImpl<TRequest, TResponse> 
    : StreamRequestHandlerWrapper<TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public override IAsyncEnumerable<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => Box(Handle((IStreamRequest<TResponse>) request, serviceProvider, cancellationToken), cancellationToken);

    public override IAsyncEnumerable<TResponse> Handle(IStreamRequest<TResponse> request, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken)
    {
        // Resolve the handler eagerly so a missing registration surfaces from CreateStream, not the first MoveNextAsync.
        IStreamRequestHandler<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

        IAsyncEnumerable<TResponse> Handler() => handler.Handle((TRequest) request, cancellationToken);

        IAsyncEnumerable<TResponse> items = serviceProvider
            .GetServices<IStreamPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate(
                (StreamHandlerDelegate<TResponse>) Handler, 
                (next, pipeline) => () => pipeline.Handle(
                    (TRequest) request, 
                    () => NextWrapper(next(), cancellationToken),
                    cancellationToken
                )
            )();

        return NextWrapper(items, cancellationToken);
    }

    private static async IAsyncEnumerable<object?> Box(
        IAsyncEnumerable<TResponse> items,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (TResponse? item in items.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> NextWrapper<T>(
        IAsyncEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ConfiguredCancelableAsyncEnumerable<T> cancellable = items
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);
        await foreach (T? item in cancellable)
        {
            yield return item;
        }
    }

}
