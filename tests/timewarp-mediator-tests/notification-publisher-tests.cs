// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class NotificationPublisherTests
{
  private static readonly TimeSpan GateTimeout = TimeSpan.FromSeconds(10);

  public class Notification : INotification
  {
  }

  /// <summary>
  /// Shared by the gated handlers of one test: each handler records its start, then waits on
  /// <see cref="Release"/> before recording its end.
  /// </summary>
  public class HandlerGate
  {
    private readonly object Lock = new();
    private readonly List<string> RecordedEvents = new();
    private int StartedCount;

    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource FirstStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource BothStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Started => Volatile.Read(ref StartedCount);

    public IReadOnlyList<string> Events
    {
      get
      {
        lock (Lock)
        {
          return RecordedEvents.ToArray();
        }
      }
    }

    public async Task Run(string name)
    {
      Record($"{name} start");
      int started = Interlocked.Increment(ref StartedCount);
      if (started == 1)
      {
        FirstStarted.TrySetResult();
      }

      if (started == 2)
      {
        BothStarted.TrySetResult();
      }

      await Release.Task;
      Record($"{name} end");
    }

    private void Record(string entry)
    {
      lock (Lock)
      {
        RecordedEvents.Add(entry);
      }
    }
  }

  public class FirstHandler : INotificationHandler<Notification>
  {
    private readonly HandlerGate HandlerGate;

    public FirstHandler(HandlerGate gate) => HandlerGate = gate;

    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) => new(HandlerGate.Run("first"));
  }

  public class SecondHandler : INotificationHandler<Notification>
  {
    private readonly HandlerGate HandlerGate;

    public SecondHandler(HandlerGate gate) => HandlerGate = gate;

    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) => new(HandlerGate.Run("second"));
  }

  private static (IMediator Mediator, HandlerGate Gate) BuildMediator(Type? notificationPublisherType)
  {
    HandlerGate gate = new();
    ServiceCollection services = new();
    services.AddSingleton(gate);
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssemblyContaining<Notification>();
      cfg.TypeEvaluator = static type => type == typeof(FirstHandler) || type == typeof(SecondHandler);
      if (notificationPublisherType is not null)
      {
        cfg.NotificationPublisherType = notificationPublisherType;
      }
    });

    return (services.BuildServiceProvider().GetRequiredService<IMediator>(), gate);
  }

  private static NotificationHandlerExecutor[] GatedExecutors(HandlerGate gate) =>
  [
      new(new object(), (_, _) => gate.Run("first")),
        new(new object(), (_, _) => gate.Run("second")),
    ];

  private static async Task ShouldRunSequentially(Task publishTask, HandlerGate gate)
  {
    await gate.FirstStarted.Task.WaitAsync(GateTimeout);

    // The first handler is parked on the gate; a sequential publisher must not start the next one.
    gate.Started.ShouldBe(1);
    publishTask.IsCompleted.ShouldBeFalse();

    gate.Release.SetResult();
    await publishTask.WaitAsync(GateTimeout);

    IReadOnlyList<string> events = gate.Events;
    events.Count.ShouldBe(4);
    string firstName = events[0].Split(' ')[0];
    string secondName = events[2].Split(' ')[0];
    firstName.ShouldNotBe(secondName);
    events.ShouldBe(new[] { $"{firstName} start", $"{firstName} end", $"{secondName} start", $"{secondName} end" });
  }

  private static async Task ShouldRunConcurrently(Task publishTask, HandlerGate gate)
  {
    // Both handlers start while the gate is still closed: only possible when they run concurrently.
    await gate.BothStarted.Task.WaitAsync(GateTimeout);
    publishTask.IsCompleted.ShouldBeFalse();
    gate.Events.ShouldBe(new[] { "first start", "second start" }, ignoreOrder: true);

    gate.Release.SetResult();
    await publishTask.WaitAsync(GateTimeout);

    gate.Events.Count.ShouldBe(4);
    gate.Events.Skip(2).ShouldBe(new[] { "first end", "second end" }, ignoreOrder: true);
  }

  [Fact]
  public async Task Foreach_await_publisher_should_run_handlers_in_order_one_at_a_time()
  {
    HandlerGate gate = new();

    Task publishTask = new ForeachAwaitPublisher().Publish(GatedExecutors(gate), new Notification(), CancellationToken.None);

    await ShouldRunSequentially(publishTask, gate);
    gate.Events.ShouldBe(new[] { "first start", "first end", "second start", "second end" });
  }

  [Fact]
  public async Task Task_when_all_publisher_should_run_handlers_concurrently()
  {
    HandlerGate gate = new();

    Task publishTask = new TaskWhenAllPublisher().Publish(GatedExecutors(gate), new Notification(), CancellationToken.None);

    await ShouldRunConcurrently(publishTask, gate);
  }

  [Fact]
  public async Task Should_handle_sequentially_by_default()
  {
    (IMediator mediator, HandlerGate gate) = BuildMediator(notificationPublisherType: null);

    await ShouldRunSequentially(mediator.Publish(new Notification()).AsTask(), gate);
  }

  [Fact]
  public async Task Should_handle_concurrently_with_task_when_all_publisher()
  {
    (IMediator mediator, HandlerGate gate) = BuildMediator(typeof(TaskWhenAllPublisher));

    await ShouldRunConcurrently(mediator.Publish(new Notification()).AsTask(), gate);
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