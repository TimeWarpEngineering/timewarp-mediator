// Modified by Steven T. Cramer

namespace TimeWarp.Mediator;

public record NotificationHandlerExecutor(object HandlerInstance, Func<INotification, CancellationToken, Task> HandlerCallback);
