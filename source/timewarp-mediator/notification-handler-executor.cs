// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

/// <summary>
/// A resolved notification handler paired with the callback that invokes it, passed to an
/// <see cref="INotificationPublisher"/>.
/// </summary>
/// <param name="HandlerInstance">The resolved handler instance, exposed so publishers can inspect or filter it</param>
/// <param name="HandlerCallback">Invokes the handler for the given notification and cancellation token</param>
public record NotificationHandlerExecutor(object HandlerInstance, Func<INotification, CancellationToken, Task> HandlerCallback);