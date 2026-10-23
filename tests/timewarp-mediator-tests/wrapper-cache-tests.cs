// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class WrapperCacheTests
{
  public class MultiResponsePing : IRequest<string>, IRequest<int> { }

  public class MultiResponsePingStringHandler : IRequestHandler<MultiResponsePing, string>
  {
    public ValueTask<string> Handle(MultiResponsePing request, CancellationToken cancellationToken) => ValueTask.FromResult("pong");
  }

  public class MultiResponsePingIntHandler : IRequestHandler<MultiResponsePing, int>
  {
    public ValueTask<int> Handle(MultiResponsePing request, CancellationToken cancellationToken) => ValueTask.FromResult(42);
  }

  public class VoidAndResponsePing : IRequest, IRequest<string> { }

  public class VoidAndResponsePingVoidHandler : IRequestHandler<VoidAndResponsePing>
  {
    public bool Handled { get; private set; }

    public ValueTask Handle(VoidAndResponsePing request, CancellationToken cancellationToken)
    {
      Handled = true;
      return default;
    }
  }

  public class VoidAndResponsePingStringHandler : IRequestHandler<VoidAndResponsePing, string>
  {
    public ValueTask<string> Handle(VoidAndResponsePing request, CancellationToken cancellationToken) => ValueTask.FromResult("pong");
  }

  [Fact]
  public async Task Should_dispatch_each_response_type_for_a_request_implementing_several_request_interfaces()
  {
    ServiceProvider serviceProvider = new ServiceCollection()
        .AddTransient<IRequestHandler<MultiResponsePing, string>, MultiResponsePingStringHandler>()
        .AddTransient<IRequestHandler<MultiResponsePing, int>, MultiResponsePingIntHandler>()
        .BuildServiceProvider();
    Mediator mediator = new(serviceProvider);
    MultiResponsePing request = new();

    (await mediator.Send((object) request)).ShouldNotBeNull();
    (await mediator.Send<string>(request)).ShouldBe("pong");
    (await mediator.Send<int>(request)).ShouldBe(42);
  }

  [Fact]
  public async Task Should_dispatch_void_send_after_untyped_send_for_a_request_implementing_both_request_interfaces()
  {
    VoidAndResponsePingVoidHandler voidHandler = new();
    ServiceProvider serviceProvider = new ServiceCollection()
        .AddSingleton<IRequestHandler<VoidAndResponsePing>>(voidHandler)
        .AddTransient<IRequestHandler<VoidAndResponsePing, string>, VoidAndResponsePingStringHandler>()
        .BuildServiceProvider();
    Mediator mediator = new(serviceProvider);
    VoidAndResponsePing request = new();

    (await mediator.Send((object) request)).ShouldBe("pong");
    await mediator.Send(request);

    voidHandler.Handled.ShouldBeTrue();
  }
}