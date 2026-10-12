// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class PipelineMultiCallToConstructorTests
{
  public class ConstructorTestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
  {
    private readonly Logger Logger;

    public ConstructorTestBehavior(Logger output) => Logger = output;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("ConstructorTestBehavior before");
      TResponse? response = await next();
      Logger.Messages.Add("ConstructorTestBehavior after");

      return response;
    }
  }

  public class ConstructorTestRequest : IRequest<ConstructorTestResponse>
  {
    public string? Message { get; set; }
  }

  public class ConstructorTestResponse
  {
    public string? Message { get; set; }
  }

  public class ConstructorCallCounter
  {
    private int CallCount;

    public int Count => Volatile.Read(ref CallCount);

    public void Increment() => Interlocked.Increment(ref CallCount);
  }

  public class ConstructorTestHandler : IRequestHandler<ConstructorTestRequest, ConstructorTestResponse>
  {
    private readonly Logger Logger;

    public ConstructorTestHandler(Logger logger, ConstructorCallCounter counter)
    {
      Logger = logger;
      counter.Increment();
    }

    public Task<ConstructorTestResponse> Handle(ConstructorTestRequest request, CancellationToken cancellationToken)
    {
      Logger.Messages.Add("Handler");
      return Task.FromResult(new ConstructorTestResponse { Message = request.Message + " ConstructorPong" });
    }
  }

  [Fact]
  public async Task Should_not_call_constructor_multiple_times_when_using_a_pipeline()
  {
    ConstructorCallCounter counter = new();
    Logger output = new();
    IServiceCollection services = new ServiceCollection();

    services.AddSingleton(output);
    services.AddSingleton(counter);
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ConstructorTestBehavior<,>));
    services.AddMediator(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
      cfg.AddOpenBehavior(typeof(ConstructorTestBehavior<,>));
    });
    ServiceProvider provider = services.BuildServiceProvider();

    IMediator med = provider.GetRequiredService<IMediator>();

    ConstructorTestResponse response = await med.Send(new ConstructorTestRequest { Message = "ConstructorPing" });

    response.Message.ShouldBe("ConstructorPing ConstructorPong");

    output.Messages.ShouldBe(new[]
    {
            "ConstructorTestBehavior before",
            "Handler",
            "ConstructorTestBehavior after"
        });
    counter.Count.ShouldBe(1);
  }
}