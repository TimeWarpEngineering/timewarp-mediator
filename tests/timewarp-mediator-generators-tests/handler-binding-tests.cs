#region Purpose
// Discovery binds every handler interface on a class and skips interfaces a concrete base handler already binds.
#endregion

public class HandlerBindingTests
{
  [Fact]
  public async Task MultiInterfaceHandler_HandlesEveryNotification()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
    BindingLog.Events.Clear();

    await publisher.Publish(new OrderPlaced());
    await publisher.Publish(new OrderShipped());

    BindingLog.Events.ShouldBe(new[] { "order:placed", "order:shipped" });
  }

  [Fact]
  public async Task MultiInterfaceHandler_HandlesEveryRequest()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    BindingLog.Events.Clear();

    (await sender.Send(new GetOrderCount())).ShouldBe(3);
    (await sender.Send(new GetOrderLabel())).ShouldBe("order");
    await sender.Send(new ArchiveOrder());

    BindingLog.Events.ShouldBe(new[] { "order:archived" });
  }

  [Fact]
  public void MultiInterfaceHandler_RegistersEveryInterface()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();

    scope.ServiceProvider.GetServices<INotificationHandler<OrderPlaced>>().Single().ShouldBeOfType<OrderHandler>();
    scope.ServiceProvider.GetServices<INotificationHandler<OrderShipped>>().Single().ShouldBeOfType<OrderHandler>();
    scope.ServiceProvider.GetService<IRequestHandler<GetOrderCount, int>>().ShouldBeOfType<OrderHandler>();
    scope.ServiceProvider.GetService<IRequestHandler<GetOrderLabel, string>>().ShouldBeOfType<OrderHandler>();
    scope.ServiceProvider.GetService<IRequestHandler<ArchiveOrder>>().ShouldBeOfType<OrderHandler>();
  }

  [Fact]
  public async Task DerivedHandler_DoesNotRebindInheritedInterfaces()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    BindingLog.Events.Clear();

    await mediator.Publish(new InvoiceRaised());
    await mediator.Publish(new InvoicePaid());
    decimal total = await mediator.Send(new GetInvoiceTotal());

    total.ShouldBe(42m);
    BindingLog.Events.ShouldBe(new[] { "invoice:raised:InvoiceHandler", "invoice:paid" });
    scope.ServiceProvider.GetServices<INotificationHandler<InvoiceRaised>>().Single().ShouldBeOfType<InvoiceHandler>();
    scope.ServiceProvider.GetServices<INotificationHandler<InvoicePaid>>().Single().ShouldBeOfType<AuditedInvoiceHandler>();
  }

  [Fact]
  public async Task AbstractGenericBase_DerivedHandlerBinds()
  {
    using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
    BindingLog.Events.Clear();

    await publisher.Publish(new RefundIssued());

    BindingLog.Events.ShouldBe(new[] { "refund:RefundIssuedHandler" });
  }

  [Fact]
  public void Manifest_ListsEachBinding()
  {
    const string bindings = "TimeWarp.Mediator.Generators.Tests.Bindings.";
    ManifestDocument manifest = ManifestDocument.Parse(MediatorManifest.Json);
    manifest.Request(bindings + "GetOrderCount").Handler.ShouldBe(bindings + "OrderHandler");
    manifest.Request(bindings + "GetOrderLabel").Handler.ShouldBe(bindings + "OrderHandler");
    manifest.Request(bindings + "ArchiveOrder").Handler.ShouldBe(bindings + "OrderHandler");
    manifest.Notification(bindings + "OrderShipped").Handlers.ShouldBe(new[] { bindings + "OrderHandler" });
    manifest.Notification(bindings + "InvoicePaid").Handlers.ShouldBe(new[] { bindings + "AuditedInvoiceHandler" });
    manifest.Requests.Select(r => r.Handler).ShouldNotContain(bindings + "AuditedInvoiceHandler");
  }

  private static ServiceProvider CreateProvider()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator();
    return services.BuildServiceProvider();
  }
}