// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class AssemblyResolutionTests
{
  private readonly IServiceProvider ServiceProvider;

  public AssemblyResolutionTests()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddSingleton(new Logger());
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      cfg.RegisterGenericHandlers = true;
    });
    ServiceProvider = services.BuildServiceProvider();
  }

  [Fact]
  public void ShouldResolveMediator()
  {
    ServiceProvider.GetService<IMediator>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<Ping, Pong>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveInternalHandler()
  {
    ServiceProvider.GetService<IRequestHandler<InternalPing>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveNotificationHandlers()
  {
    ServiceProvider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(4);
  }

  [Fact]
  public void ShouldRequireAtLeastOneAssembly()
  {
    ServiceCollection services = new();

    Action registration = () => services.AddMediator(_ => { });

    registration.ShouldThrow<ArgumentException>();
  }

  [Fact]
  public void ShouldResolveGenericVoidRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<OpenGenericVoidRequest<ConcreteTypeArgument>>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveGenericReturnTypeRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<OpenGenericReturnTypeRequest<ConcreteTypeArgument>, string>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveGenericPingRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<GenericPing<Pong>, Pong>>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldResolveVoidGenericPingRequestHandler()
  {
    ServiceProvider.GetService<IRequestHandler<VoidGenericPing<Pong>>>().ShouldNotBeNull();
  }
}