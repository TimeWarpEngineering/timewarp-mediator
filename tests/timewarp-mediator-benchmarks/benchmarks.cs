// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Benchmarks;

[DotTraceDiagnoser]
public class Benchmarks
{
  private IMediator Mediator;
  private readonly Ping Ping = new() { Message = "Hello World" };
  private readonly Pinged Pinged = new();

  [GlobalSetup]
  public void GlobalSetup()
  {
    ServiceCollection services = new();

    services.AddSingleton(TextWriter.Null);

    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssemblyContaining(typeof(Ping));
      cfg.AddOpenBehavior(typeof(GenericPipelineBehavior<,>));
    });

    ServiceProvider provider = services.BuildServiceProvider();

    Mediator = provider.GetRequiredService<IMediator>();
  }

  [Benchmark]
  public Task SendingRequests()
  {
    return Mediator.Send(Ping);
  }

  [Benchmark]
  public Task PublishingNotifications()
  {
    return Mediator.Publish(Pinged);
  }
}