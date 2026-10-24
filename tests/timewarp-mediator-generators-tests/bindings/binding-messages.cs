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

public sealed class ScopedToken : IRequest<Guid>
{
}

public sealed class ScopedTokenHandler : IRequestHandler<ScopedToken, Guid>
{
  public Guid Token { get; } = Guid.NewGuid();

  public ValueTask<Guid> Handle(ScopedToken request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(Token);
  }
}

// One class, five handler interfaces: every one must bind and register.
public sealed class OrderHandler
    : INotificationHandler<OrderPlaced>,
      INotificationHandler<OrderShipped>,
      IRequestHandler<GetOrderCount, int>,
      IRequestHandler<GetOrderLabel, string>,
      IRequestHandler<ArchiveOrder>
{
  public ValueTask Handle(OrderPlaced notification, CancellationToken cancellationToken)
  {
    BindingLog.Events.Add("order:placed");
    return default;
  }

  public ValueTask Handle(OrderShipped notification, CancellationToken cancellationToken)
  {
    BindingLog.Events.Add("order:shipped");
    return default;
  }

  public ValueTask<int> Handle(GetOrderCount request, CancellationToken cancellationToken) => ValueTask.FromResult(3);

  public ValueTask<string> Handle(GetOrderLabel request, CancellationToken cancellationToken) => ValueTask.FromResult("order");

  public ValueTask Handle(ArchiveOrder request, CancellationToken cancellationToken)
  {
    BindingLog.Events.Add("order:archived");
    return default;
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
  public virtual ValueTask Handle(InvoiceRaised notification, CancellationToken cancellationToken)
  {
    BindingLog.Events.Add("invoice:raised:" + GetType().Name);
    return default;
  }

  public ValueTask<decimal> Handle(GetInvoiceTotal request, CancellationToken cancellationToken) => ValueTask.FromResult(42m);
}

// Inherits InvoiceHandler's bindings (no TWM002, no second publish) and introduces InvoicePaid.
public sealed class AuditedInvoiceHandler : InvoiceHandler, INotificationHandler<InvoicePaid>
{
  public ValueTask Handle(InvoicePaid notification, CancellationToken cancellationToken)
  {
    BindingLog.Events.Add("invoice:paid");
    return default;
  }
}

public sealed class RefundIssued : INotification
{
}

// Abstract generic base: the closed derived class is the handler and binds normally.
public abstract class RefundHandlerBase<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
  public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
  {
    BindingLog.Events.Add("refund:" + GetType().Name);
    return default;
  }
}

public sealed class RefundIssuedHandler : RefundHandlerBase<RefundIssued>
{
}