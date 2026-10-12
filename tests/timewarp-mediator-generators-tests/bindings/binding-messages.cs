#region Purpose
// Handler shapes for binding discovery: one class with several handler interfaces, and derived handlers.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.Bindings;

public sealed class BindingLog
{
    public static List<string> Events { get; } = new();
}

public sealed class OrderPlaced : INotification
{
}

public sealed class OrderShipped : INotification
{
}

public sealed class GetOrderCount : IRequest<int>
{
}

public sealed class GetOrderLabel : IRequest<string>
{
}

public sealed class ArchiveOrder : IRequest
{
}

// One class, five handler interfaces: every one must bind and register.
public sealed class OrderHandler
    : INotificationHandler<OrderPlaced>,
      INotificationHandler<OrderShipped>,
      IRequestHandler<GetOrderCount, int>,
      IRequestHandler<GetOrderLabel, string>,
      IRequestHandler<ArchiveOrder>
{
    public Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        BindingLog.Events.Add("order:placed");
        return Task.CompletedTask;
    }

    public Task Handle(OrderShipped notification, CancellationToken cancellationToken)
    {
        BindingLog.Events.Add("order:shipped");
        return Task.CompletedTask;
    }

    public Task<int> Handle(GetOrderCount request, CancellationToken cancellationToken) => Task.FromResult(3);

    public Task<string> Handle(GetOrderLabel request, CancellationToken cancellationToken) => Task.FromResult("order");

    public Task Handle(ArchiveOrder request, CancellationToken cancellationToken)
    {
        BindingLog.Events.Add("order:archived");
        return Task.CompletedTask;
    }
}

public sealed class InvoiceRaised : INotification
{
}

public sealed class InvoicePaid : INotification
{
}

public sealed class GetInvoiceTotal : IRequest<decimal>
{
}

// Concrete base handler: owns the InvoiceRaised and GetInvoiceTotal bindings.
public class InvoiceHandler : INotificationHandler<InvoiceRaised>, IRequestHandler<GetInvoiceTotal, decimal>
{
    public virtual Task Handle(InvoiceRaised notification, CancellationToken cancellationToken)
    {
        BindingLog.Events.Add("invoice:raised:" + GetType().Name);
        return Task.CompletedTask;
    }

    public Task<decimal> Handle(GetInvoiceTotal request, CancellationToken cancellationToken) => Task.FromResult(42m);
}

// Inherits InvoiceHandler's bindings (no TWM002, no second publish) and introduces InvoicePaid.
public sealed class AuditedInvoiceHandler : InvoiceHandler, INotificationHandler<InvoicePaid>
{
    public Task Handle(InvoicePaid notification, CancellationToken cancellationToken)
    {
        BindingLog.Events.Add("invoice:paid");
        return Task.CompletedTask;
    }
}

public sealed class RefundIssued : INotification
{
}

// Abstract generic base: the closed derived class is the handler and binds normally.
public abstract class RefundHandlerBase<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        BindingLog.Events.Add("refund:" + GetType().Name);
        return Task.CompletedTask;
    }
}

public sealed class RefundIssuedHandler : RefundHandlerBase<RefundIssued>
{
}
