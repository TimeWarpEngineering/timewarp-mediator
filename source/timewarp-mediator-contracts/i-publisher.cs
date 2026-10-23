// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// Publish a notification or event through the mediator pipeline to be handled by multiple handlers.
/// </summary>
public interface IPublisher
{
  /// <summary>
  /// Sends a notification to multiple handlers. The notification is type-erased.
  /// </summary>
  /// <param name="notification">Notification object.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask"/> that completes when publishing finishes.</returns>
  ValueTask Publish(object notification, CancellationToken cancellationToken = default);

  /// <summary>
  /// Sends a notification to multiple handlers.
  /// </summary>
  /// <typeparam name="TNotification">Notification type.</typeparam>
  /// <param name="notification">Notification object.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask"/> that completes when publishing finishes.</returns>
  ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification;
}

/// <summary>
/// Publish through the named pipeline identified by <typeparamref name="TScope"/>.
/// Unscoped <see cref="IPublisher"/> is the default pipeline (handlers with no
/// <see cref="MediatorScopeAttribute"/>).
/// </summary>
/// <typeparam name="TScope">Marker type that names the pipeline (for example <c>ClientPipeline</c>).</typeparam>
public interface IPublisher<TScope> : IPublisher
{
}
