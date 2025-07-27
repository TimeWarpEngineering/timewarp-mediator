// Modified by Steven T. Cramer
using Microsoft.Extensions.DependencyInjection;

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

using System;
using System.Linq;
using Shouldly;
using Xunit;

public class DuplicateAssemblyResolutionTests
{
    private readonly IServiceProvider _provider;

    public DuplicateAssemblyResolutionTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg => cfg.RegisterServicesFromAssemblies(typeof(Ping).Assembly, typeof(Ping).Assembly));
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public void ShouldResolveNotificationHandlersOnlyOnce()
    {
        _provider.GetServices<INotificationHandler<Pinged>>().Count().ShouldBe(4);
    }
}