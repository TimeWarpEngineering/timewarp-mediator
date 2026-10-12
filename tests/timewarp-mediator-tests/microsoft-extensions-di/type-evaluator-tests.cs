// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class TypeEvaluatorTests
{
  private readonly IServiceProvider ServiceProvider;
  private readonly IServiceCollection ServiceCollection;

  public TypeEvaluatorTests()
  {
    ServiceCollection = new ServiceCollection();
    ServiceCollection.AddSingleton(new Logger());
    ServiceCollection.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssemblyContaining(typeof(Ping));
      cfg.TypeEvaluator = t => t.Namespace == "TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests.Included";
    });
    ServiceProvider = ServiceCollection.BuildServiceProvider();
  }

  [Fact]
  public void ShouldResolveMediator()
  {
    ServiceProvider.GetService<IMediator>().ShouldNotBeNull();
  }

  [Fact]
  public void ShouldOnlyResolveIncludedRequestHandlers()
  {
    ServiceProvider.GetService<IRequestHandler<Foo, Bar>>().ShouldNotBeNull();
    ServiceProvider.GetService<IRequestHandler<Ping, Pong>>().ShouldBeNull();
  }

  [Fact]
  public void ShouldNotRegisterUnNeededBehaviors()
  {
    ServiceCollection.Any(service => service.ImplementationType == typeof(RequestPreProcessorBehavior<,>))
        .ShouldBeFalse();
    ServiceCollection.Any(service => service.ImplementationType == typeof(RequestPostProcessorBehavior<,>))
        .ShouldBeFalse();
    ServiceCollection.Any(service => service.ImplementationType == typeof(RequestExceptionActionProcessorBehavior<,>))
        .ShouldBeFalse();
    ServiceCollection.Any(service => service.ImplementationType == typeof(RequestExceptionProcessorBehavior<,>))
        .ShouldBeFalse();
  }
}