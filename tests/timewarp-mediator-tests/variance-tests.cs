namespace TimeWarp.Mediator.Tests;

/// <summary>
/// Reflection-runtime variance. For notifications, the registrar adds every scanned handler
/// assignable to a closed <c>INotificationHandler&lt;T&gt;</c> that some scanned handler
/// implements. A derived notification reaches base and interface handlers when a handler for
/// that derived type is scanned with them, and is not delivered when only base handlers are
/// scanned. For requests, the registrar keeps a single exact match and does not bind a base
/// handler to a derived request. The derived-request test registers that closed interface
/// itself, which compiles because <c>TRequest</c> is contravariant.
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
    private readonly EventLog EventLog;

    public AnimalEventInterfaceHandler(EventLog log) => EventLog = log;

    public Task Handle(IAnimalEvent notification, CancellationToken cancellationToken)
    {
      EventLog.Entries.Add($"IAnimalEvent:{notification.Name}");
      return Task.CompletedTask;
    }
  }

  public class AnimalEventHandler : INotificationHandler<AnimalEvent>
  {
    private readonly EventLog EventLog;

    public AnimalEventHandler(EventLog log) => EventLog = log;

    public Task Handle(AnimalEvent notification, CancellationToken cancellationToken)
    {
      EventLog.Entries.Add($"AnimalEvent:{notification.Name}");
      return Task.CompletedTask;
    }
  }

  public class DogBarkedHandler : INotificationHandler<DogBarked>
  {
    private readonly EventLog EventLog;

    public DogBarkedHandler(EventLog log) => EventLog = log;

    public Task Handle(DogBarked notification, CancellationToken cancellationToken)
    {
      EventLog.Entries.Add($"DogBarked:{notification.Name}");
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

    // The registrar does not map IRequestHandler<Greeting, string> onto FormalGreeting.
    // Registering the closed interface is legal because TRequest is contravariant.
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
  public async Task Send_explicitly_registered_derived_request_handler_should_see_runtime_type()
  {
    (IMediator mediator, _) = BuildMediator();

    string response = await mediator.Send(new FormalGreeting { Name = "Ada" });

    response.ShouldBe("Hello Ada from FormalGreeting");
  }
}