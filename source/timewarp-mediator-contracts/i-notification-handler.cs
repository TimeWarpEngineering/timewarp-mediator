// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// Defines a handler for a notification.
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
  /// <summary>
  /// Handles a notification.
  /// </summary>
  /// <param name="notification">The notification.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask"/> that completes when the handler finishes.</returns>
  ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
}
