// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class PublishTests
{
    public class Ping : INotification
    {
        public string? Message { get; set; }
    }

    public class PongHandler : INotificationHandler<Ping>
    {
        private readonly TextWriter _writer;

        public PongHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync(notification.Message + " Pong");
        }
    }

    public class PungHandler : INotificationHandler<Ping>
    {
        private readonly TextWriter _writer;

        public PungHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync(notification.Message + " Pung");
        }
    }

    [Fact]
    public async Task Should_resolve_main_handler()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof (INotificationHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator med = container.GetInstance<IMediator>();

        await med.Publish(new Ping { Message = "Ping" });

        string[] result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
        result.ShouldContain("Ping Pong");
        result.ShouldContain("Ping Pung");
    }

    [Fact]
    public async Task Should_resolve_main_handler_when_object_is_passed()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof (INotificationHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator med = container.GetInstance<IMediator>();

        object message = new Ping { Message = "Ping" };
        await med.Publish(message);

        string[] result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
        result.ShouldContain("Ping Pong");
        result.ShouldContain("Ping Pung");
    }

    public class SequentialMediator : Mediator
    {
        public SequentialMediator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override async Task PublishCore(IEnumerable<NotificationHandlerExecutor> allHandlers, INotification notification, CancellationToken cancellationToken)
        {
            foreach (NotificationHandlerExecutor handler in allHandlers)
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public class SequentialPublisher : INotificationPublisher
    {
        public int CallCount { get; set; }

        public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            foreach (NotificationHandlerExecutor handler in handlerExecutors)
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
                CallCount++;
            }
        }
    }

    [Fact]
    public async Task Should_override_with_sequential_firing()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(INotificationHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IMediator>().Use<SequentialMediator>();
        });

        IMediator med = container.GetInstance<IMediator>();

        await med.Publish(new Ping { Message = "Ping" });

        string[] result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        result.ShouldContain("Ping Pong");
        result.ShouldContain("Ping Pung");
    }

    [Fact]
    public async Task Should_override_with_sequential_firing_through_injection()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);
        SequentialPublisher publisher = new();

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(INotificationHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<INotificationPublisher>().Use(publisher);
            cfg.For<IMediator>().Use<Mediator>();
        });

        IMediator med = container.GetInstance<IMediator>();

        await med.Publish(new Ping { Message = "Ping" });

        string[] result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        result.ShouldContain("Ping Pong");
        result.ShouldContain("Ping Pung");
        publisher.CallCount.ShouldBe(2);
    }

    [Fact]
    public async Task Should_resolve_handlers_given_interface()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(INotificationHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IMediator>().Use<SequentialMediator>();
        });

        IMediator med = container.GetInstance<IMediator>();

        // wrap notifications in an array, so this test won't break on a 'replace with var' refactoring
        INotification[] notifications = new INotification[] { new Ping { Message = "Ping" } };
        await med.Publish(notifications[0]);

        string[] result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        result.ShouldContain("Ping Pong");
        result.ShouldContain("Ping Pung");
    }

    [Fact]
    public async Task Should_resolve_main_handler_by_specific_interface()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof (INotificationHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IPublisher>().Use<Mediator>();
        });

        IPublisher med = container.GetInstance<IPublisher>();

        await med.Publish(new Ping { Message = "Ping" });

        string[] result = builder.ToString().Split(new [] {Environment.NewLine}, StringSplitOptions.None);
        result.ShouldContain("Ping Pong");
        result.ShouldContain("Ping Pung");
    }
}
