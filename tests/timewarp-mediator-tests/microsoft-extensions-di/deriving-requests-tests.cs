// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class DerivingRequestsTests
{
  private readonly IServiceProvider ServiceProvider;
  private readonly IMediator Mediator;

  public DerivingRequestsTests()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(new Logger());
    services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(Ping)));
    ServiceProvider = services.BuildServiceProvider();
    Mediator = ServiceProvider.GetRequiredService<IMediator>();
  }

  [Fact]
  public async Task ShouldReturnPingPong()
  {
    Pong pong = await Mediator.Send(new Ping() { Message = "Ping" });
    pong.Message.ShouldBe("Ping Pong");
  }

  [Fact]
  public async Task ShouldReturnDerivedPingPong()
  {
    Pong pong = await Mediator.Send(new DerivedPing() { Message = "Ping" });
    pong.Message.ShouldBe("DerivedPing Pong");
  }
}