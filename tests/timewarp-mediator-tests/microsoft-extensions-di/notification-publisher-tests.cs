// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class NotificationPublisherTests
{
    public class MockPublisher : INotificationPublisher
    {
        public int CallCount { get; set; }

        public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            foreach (NotificationHandlerExecutor handlerExecutor in handlerExecutors)
            {
                await handlerExecutor.HandlerCallback(notification, cancellationToken);
                CallCount++;
            }
        }
    }

    [Fact]
    public void ShouldResolveDefaultPublisher()
    {
        ServiceCollection services = new();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
        });

        ServiceProvider provider = services.BuildServiceProvider();
        IMediator? med = provider.GetService<IMediator>();

        med.ShouldNotBeNull();

        INotificationPublisher? publisher = provider.GetService<INotificationPublisher>();

        publisher.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldSubstitutePublisherInstance()
    {
        MockPublisher publisher = new();
        ServiceCollection services = new();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisher = publisher;
        });

        ServiceProvider provider = services.BuildServiceProvider();
        IMediator? med = provider.GetService<IMediator>();

        med.ShouldNotBeNull();

        await med.Publish(new Pinged());
        
        publisher.CallCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSubstitutePublisherServiceType()
    {
        ServiceCollection services = new();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisherType = typeof(MockPublisher);
            cfg.Lifetime = ServiceLifetime.Singleton;
        });

        ServiceProvider provider = services.BuildServiceProvider();
        IMediator? med = provider.GetService<IMediator>();
        INotificationPublisher? publisher = provider.GetService<INotificationPublisher>();

        med.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        await med.Publish(new Pinged());

        MockPublisher mock = publisher.ShouldBeOfType<MockPublisher>();

        mock.CallCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSubstitutePublisherServiceTypeWithWhenAll()
    {
        ServiceCollection services = new();
        services.AddSingleton(new Logger());
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisherType = typeof(TaskWhenAllPublisher);
            cfg.Lifetime = ServiceLifetime.Singleton;
        });

        ServiceProvider provider = services.BuildServiceProvider();
        IMediator? med = provider.GetService<IMediator>();
        INotificationPublisher? publisher = provider.GetService<INotificationPublisher>();

        med.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        await Should.NotThrowAsync(med.Publish(new Pinged()));

        publisher.ShouldBeOfType<TaskWhenAllPublisher>();
    }
}
