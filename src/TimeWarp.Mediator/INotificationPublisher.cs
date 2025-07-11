// Modified by Steven T. Cramer
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace TimeWarp.Mediator;

public interface INotificationPublisher
{
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken cancellationToken);
}