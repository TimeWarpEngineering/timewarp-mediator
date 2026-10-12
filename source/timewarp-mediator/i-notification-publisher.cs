// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// Strategy that controls how the handlers for a published notification are invoked
/// (for example sequentially with <see cref="NotificationPublishers.ForeachAwaitPublisher"/> or
/// concurrently with <see cref="NotificationPublishers.TaskWhenAllPublisher"/>).
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Invokes the notification handlers represented by <paramref name="handlerExecutors"/>.
    /// </summary>
    /// <param name="handlerExecutors">The handlers to invoke, already resolved and ordered</param>
    /// <param name="notification">The notification being published</param>
    /// <param name="cancellationToken">Cancellation token passed to every handler</param>
    /// <returns>A task that completes when the publisher considers publishing finished</returns>
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken cancellationToken);
}
