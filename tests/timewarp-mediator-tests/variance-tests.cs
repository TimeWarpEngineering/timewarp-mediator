namespace TimeWarp.Mediator.Tests;

/// <summary>
/// Dispatch through C# generic variance on the reflection runtime: <c>INotificationHandler&lt;in T&gt;</c>
/// and <c>IRequestHandler&lt;in TRequest, TResponse&gt;</c> are contravariant, so a handler for a base
/// message also handles a derived one.
/// </summary>
public class VarianceTests
{
    public interface IAnimalEvent : INotification
    {
        string Name { get; }
    }

    public class AnimalEvent : IAnimalEvent
    {
        public string Name { get; init; } = "";
    }

    public class DogBarked : AnimalEvent
    {
    }

    public class EventLog
    {
        public List<string> Entries { get; } = new();
    }

    public class AnimalEventInterfaceHandler : INotificationHandler<IAnimalEvent>
    {
        private readonly EventLog _log;

        public AnimalEventInterfaceHandler(EventLog log) => _log = log;

        public Task Handle(IAnimalEvent notification, CancellationToken cancellationToken)
        {
            _log.Entries.Add($"IAnimalEvent:{notification.Name}");
            return Task.CompletedTask;
        }
    }

    public class AnimalEventHandler : INotificationHandler<AnimalEvent>
    {
        private readonly EventLog _log;

        public AnimalEventHandler(EventLog log) => _log = log;

        public Task Handle(AnimalEvent notification, CancellationToken cancellationToken)
        {
            _log.Entries.Add($"AnimalEvent:{notification.Name}");
            return Task.CompletedTask;
        }
    }

    public class DogBarkedHandler : INotificationHandler<DogBarked>
    {
        private readonly EventLog _log;

        public DogBarkedHandler(EventLog log) => _log = log;

        public Task Handle(DogBarked notification, CancellationToken cancellationToken)
        {
            _log.Entries.Add($"DogBarked:{notification.Name}");
            return Task.CompletedTask;
        }
    }

    public class Greeting : IRequest<string>
    {
        public string Name { get; init; } = "";
    }

    public class FormalGreeting : Greeting
    {
    }

    public class GreetingHandler : IRequestHandler<Greeting, string>
    {
        public Task<string> Handle(Greeting request, CancellationToken cancellationToken)
            => Task.FromResult($"Hello {request.Name} from {request.GetType().Name}");
    }

    private static readonly HashSet<Type> ScannedTypes = new()
    {
        typeof(AnimalEventInterfaceHandler),
        typeof(AnimalEventHandler),
        typeof(DogBarkedHandler),
        typeof(GreetingHandler),
    };

    private static (IMediator Mediator, EventLog Log) BuildMediator()
    {
        EventLog log = new();
        ServiceCollection services = new();
        services.AddSingleton(log);
        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<VarianceTests>();
            cfg.TypeEvaluator = ScannedTypes.Contains;
        });

        // Contravariance: the base-request handler satisfies the derived request's handler interface.
        services.AddTransient<IRequestHandler<FormalGreeting, string>, GreetingHandler>();

        return (services.BuildServiceProvider().GetRequiredService<IMediator>(), log);
    }

    [Fact]
    public async Task Publish_derived_notification_should_reach_base_class_and_interface_handlers()
    {
        (IMediator mediator, EventLog log) = BuildMediator();

        await mediator.Publish(new DogBarked { Name = "Rex" });

        log.Entries.OrderBy(static entry => entry, StringComparer.Ordinal).ShouldBe(new[]
        {
            "AnimalEvent:Rex",
            "DogBarked:Rex",
            "IAnimalEvent:Rex",
        });
    }

    [Fact]
    public async Task Publish_base_notification_should_not_reach_derived_handlers()
    {
        (IMediator mediator, EventLog log) = BuildMediator();

        await mediator.Publish(new AnimalEvent { Name = "Cat" });

        log.Entries.OrderBy(static entry => entry, StringComparer.Ordinal).ShouldBe(new[]
        {
            "AnimalEvent:Cat",
            "IAnimalEvent:Cat",
        });
    }

    [Fact]
    public async Task Publish_through_object_overload_should_dispatch_on_runtime_type()
    {
        (IMediator mediator, EventLog log) = BuildMediator();

        object notification = new DogBarked { Name = "Fido" };
        await mediator.Publish(notification);

        log.Entries.Count.ShouldBe(3);
        log.Entries.ShouldContain("DogBarked:Fido");
    }

    [Fact]
    public async Task Send_base_request_should_use_its_handler()
    {
        (IMediator mediator, _) = BuildMediator();

        string response = await mediator.Send(new Greeting { Name = "Ada" });

        response.ShouldBe("Hello Ada from Greeting");
    }

    [Fact]
    public async Task Send_derived_request_should_use_contravariant_base_handler()
    {
        (IMediator mediator, _) = BuildMediator();

        string response = await mediator.Send(new FormalGreeting { Name = "Ada" });

        response.ShouldBe("Hello Ada from FormalGreeting");
    }
}
