#region Purpose
// Golden-file clone of TimeWarp.State StateTransactionBehavior: clone, next, restore + notify on throw.
#endregion

#region Design
// Matches today's Reverse().Aggregate semantics: this behavior is innermost among the State stack
// in the golden file (listed last), so it wraps the handler. Success keeps the cloned state;
// exception restores the original and publishes ExceptionNotification.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.State;

public sealed class StateTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAction
{
    private readonly IStore Store;
    private readonly IPublisher Publisher;

    public StateTransactionBehavior(IStore store, IPublisher publisher)
    {
        Store = store;
        Publisher = publisher;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Type enclosingStateType = typeof(TRequest).GetEnclosingStateType();
        IState originalState = Store.GetState(enclosingStateType);
        IState newState = originalState.Clone();
        newState.Sender = originalState.Sender;
        Store.SetState(newState);

        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Store.SetState(originalState);
            ExceptionNotification exceptionNotification = new(
                requestName: typeof(StateTransactionBehavior<TRequest, TResponse>).Name,
                exception: exception);
            await Publisher.Publish(exceptionNotification, cancellationToken).ConfigureAwait(false);
            return default!;
        }
    }
}
