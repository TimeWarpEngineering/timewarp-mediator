// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class TypeResolutionTests
{
  private readonly IServiceProvider ServiceProvider;

  public TypeResolutionTests()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(new Logger());
    services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(Ping)));
    ServiceProvider = services.BuildServiceProvider();
  }

  [Fact]
  public void ShouldResolveMediator()
  {
    ServiceProvider.GetService<IMediator>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveSender()
  {
    ServiceProvider.GetService<ISender>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolvePublisher()
  {
    ServiceProvider.GetService<IPublisher>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<Ping, Pong>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveVoidRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<Ding>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveNotificationHandlers()
  {
    ServiceProvider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(4);
  }

  [Fact]
  public void ShouldNotThrowWithMissingEnumerables()
  {
    Should.NotThrow(() => ServiceProvider.GetRequiredService<IEnumerable<IRequestExceptionAction<int, Exception>>>());
  }

  [Fact]
  public void ShouldResolveFirstDuplicateHandler()
  {
    ServiceProvider.GetService<IRequestHandler<DuplicateTest, string>>().ShouldNotBeNull();
    ServiceProvider.GetService<IRequestHandler<DuplicateTest, string>>()
        .ShouldBeAssignableTo<DuplicateHandler1>();
  }

  [Fact]
  public void ShouldResolveIgnoreSecondDuplicateHandler()
  {
    ServiceProvider.GetServices<IRequestHandler<DuplicateTest, string>>().Count().ShouldBe(1);
  }

  [Fact]
  public void ShouldHandleKeyedServices()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(new Logger());
    services.AddKeyedSingleton<string>("Foo", "Foo");
    services.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(Ping)));
    ServiceProvider serviceProvider = services.BuildServiceProvider();

    IMediator med = serviceProvider.GetRequiredService<IMediator>();

    med.ShouldNotBeNull();
  }
}