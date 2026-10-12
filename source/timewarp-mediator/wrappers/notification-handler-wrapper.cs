// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Wrappers;

internal abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(INotification notification, IServiceProvider serviceProvider,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken);
}

internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override Task Handle(INotification notification, IServiceProvider serviceProvider,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        IEnumerable<NotificationHandlerExecutor> handlers = serviceProvider
            .GetServices<INotificationHandler<TNotification>>()
            .Select(static x => new NotificationHandlerExecutor(x, (theNotification, theToken) => x.Handle((TNotification)theNotification, theToken)));

        return publish(handlers, notification, cancellationToken);
    }
}
