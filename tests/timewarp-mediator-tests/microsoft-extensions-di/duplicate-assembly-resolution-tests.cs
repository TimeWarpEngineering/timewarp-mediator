// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class DuplicateAssemblyResolutionTests
{
  private readonly IServiceProvider ServiceProvider;

  public DuplicateAssemblyResolutionTests()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(new Logger());
    services.AddMediator(cfg => cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly, typeof(Ping).Assembly));
    ServiceProvider = services.BuildServiceProvider();
  }

  [Fact]
  public void ShouldResolveNotificationHandlersOnlyOnce()
  {
    ServiceProvider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(4);
  }
}