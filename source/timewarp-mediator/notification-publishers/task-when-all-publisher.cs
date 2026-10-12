// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.NotificationPublishers;

/// <summary>
/// Uses Task.WhenAll with the list of Handler tasks. A handler that throws synchronously is converted
/// to a faulted task so the remaining handlers still run and every failure is observed:
/// <code>
/// Task[] tasks = handlerExecutors
///     .Select(handler => InvokeHandler(handler, notification, cancellationToken))
///     .ToArray();
///
/// return Task.WhenAll(tasks);
/// </code>
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
  /// <inheritdoc/>
  public Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
  {
    Task[] tasks = handlerExecutors
        .Select(handler => InvokeHandler(handler, notification, cancellationToken))
        .ToArray();

    return Task.WhenAll(tasks);
  }

  private static Task InvokeHandler(NotificationHandlerExecutor handler, INotification notification, CancellationToken cancellationToken)
  {
    try
    {
      return handler.HandlerCallback(notification, cancellationToken);
    }
    catch (Exception exception)
    {
      return Task.FromException(exception);
    }
  }
}