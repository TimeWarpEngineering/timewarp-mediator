// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.NotificationPublishers;

/// <summary>
/// Uses Task.WhenAll with the list of Handler tasks:
/// <code>
/// var tasks = handlers
///                .Select(handler => handler.Handle(notification, cancellationToken))
///                .ToList();
/// 
/// return Task.WhenAll(tasks);
/// </code>
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
    public Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        Task[] tasks = handlerExecutors
            .Select(handler => handler.HandlerCallback(notification, cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }
}
