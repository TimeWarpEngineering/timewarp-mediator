// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class NotificationPublisherTests
{
    private readonly ITestOutputHelper _output;

    public NotificationPublisherTests(ITestOutputHelper output) => _output = output;

    public class Notification : INotification
    {
    }

    public class FirstHandler : INotificationHandler<Notification>
    {
        public async Task Handle(Notification notification, CancellationToken cancellationToken) 
            => await Task.Delay(500, cancellationToken);
    }
    public class SecondHandler : INotificationHandler<Notification>
    {
        public async Task Handle(Notification notification, CancellationToken cancellationToken) 
            => await Task.Delay(250, cancellationToken);
    }

    [Fact]
    public async Task Should_handle_sequentially_by_default()
    {
        ServiceCollection services = new();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Notification>();
        });
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IMediator med = serviceProvider.GetRequiredService<IMediator>();

        Stopwatch timer = new();
        timer.Start();

        await med.Publish(new Notification());

        timer.Stop();
        
        long sequentialElapsed = timer.ElapsedMilliseconds;

        services = new ServiceCollection();
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Notification>();
            cfg.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });
        serviceProvider = services.BuildServiceProvider();

        med = serviceProvider.GetRequiredService<IMediator>();

        timer.Restart();

        await med.Publish(new Notification());

        timer.Stop();
        
        long parallelElapsed = timer.ElapsedMilliseconds;

        sequentialElapsed.ShouldBeGreaterThan(parallelElapsed);
    }

    [Fact]
    public async Task Task_when_all_publisher_should_run_remaining_handlers_when_a_handler_throws_synchronously()
    {
        bool secondHandlerRan = false;
        NotificationHandlerExecutor[] handlerExecutors =
        {
            new(new object(), (_, _) => throw new InvalidOperationException("first")),
            new(new object(), (_, _) =>
            {
                secondHandlerRan = true;
                return Task.CompletedTask;
            }),
            new(new object(), (_, _) => throw new InvalidOperationException("third")),
        };

        Task publishTask = new TaskWhenAllPublisher().Publish(handlerExecutors, new Notification(), CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(publishTask);
        secondHandlerRan.ShouldBeTrue();
        publishTask.Exception!.InnerExceptions.Select(static exception => exception.Message).ShouldBe(new[] { "first", "third" });
    }
}
