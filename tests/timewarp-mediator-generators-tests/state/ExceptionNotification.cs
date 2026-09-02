#region Purpose
// State-shaped ExceptionNotification published when StateTransactionBehavior restores on throw.
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace TimeWarp.Mediator.Generators.Tests.State;

public sealed class ExceptionNotification : INotification
{
    public ExceptionNotification(string requestName, Exception exception)
    {
        RequestName = requestName;
        Exception = exception;
    }

    public string RequestName { get; }

    public Exception Exception { get; }
}

public sealed class RecordingExceptionHandler : INotificationHandler<ExceptionNotification>
{
    public List<ExceptionNotification> Notifications { get; } = new();

    public Task Handle(ExceptionNotification notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);
        return Task.CompletedTask;
    }
}
