// Modified by Steven T. Cramer
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator.NotificationPublishers;
using Shouldly;
using Xunit;

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class NotificationPublisherTests
{
    public class MockPublisher : INotificationPublisher
    {
        public int CallCount { get; set; }

        public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handlerExecutor in handlerExecutors)
            {
                await handlerExecutor.HandlerCallback(notification, cancellationToken);
                CallCount++;
            }
        }
    }

    [Fact]
    public void ShouldResolveDefaultPublisher()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
        });

        var provider = services.BuildServiceProvider();
        var med = provider.GetService<IMediator>();

        med.ShouldNotBeNull();

        var publisher = provider.GetService<INotificationPublisher>();

        publisher.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldSubstitutePublisherInstance()
    {
        var publisher = new MockPublisher();
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisher = publisher;
        });

        var provider = services.BuildServiceProvider();
        var med = provider.GetService<IMediator>();

        med.ShouldNotBeNull();

        await med.Publish(new Pinged());
        
        publisher.CallCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSubstitutePublisherServiceType()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisherType = typeof(MockPublisher);
            cfg.Lifetime = ServiceLifetime.Singleton;
        });

        var provider = services.BuildServiceProvider();
        var med = provider.GetService<IMediator>();
        var publisher = provider.GetService<INotificationPublisher>();

        med.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        await med.Publish(new Pinged());

        var mock = publisher.ShouldBeOfType<MockPublisher>();

        mock.CallCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSubstitutePublisherServiceTypeWithWhenAll()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisherType = typeof(TaskWhenAllPublisher);
            cfg.Lifetime = ServiceLifetime.Singleton;
        });

        var provider = services.BuildServiceProvider();
        var med = provider.GetService<IMediator>();
        var publisher = provider.GetService<INotificationPublisher>();

        med.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        await Should.NotThrowAsync(med.Publish(new Pinged()));

        publisher.ShouldBeOfType<TaskWhenAllPublisher>();
    }
}