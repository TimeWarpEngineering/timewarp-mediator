#region Purpose
// NativeAOT host. The generated mediator takes IServiceProvider and weaves pipeline behaviors.
#endregion

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(StampBehavior))]

internal static class Program
{
  private static async Task<int> Main()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator();
    await using ServiceProvider provider = services.BuildServiceProvider();
    using IServiceScope scope = provider.CreateScope();
    IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    Pong pong = await mediator.Send(new Ping("aot"));
    object? boxed = await mediator.Send((object)new Ping("switch"));
    if (StampBehavior.Runs != 2
        || pong.Message != "aot-pong"
        || boxed is not Pong boxedPong
        || boxedPong.Message != "switch-pong")
    {
      return 1;
    }

    Console.WriteLine(pong.Message);
    return 0;
  }
}

public sealed class StampBehavior : IPipelineBehavior<Ping, Pong>
{
  public static int Runs;

  public ValueTask<Pong> Handle(
      Ping request,
      MessageHandlerDelegate<Ping, Pong> next,
      CancellationToken cancellationToken)
  {
    Runs++;
    return next(request, cancellationToken);
  }
}

public sealed class Ping : IRequest<Pong>
{
  public Ping(string message)
  {
    Message = message;
  }

  public string Message { get; }
}

public sealed class Pong
{
  public Pong(string message)
  {
    Message = message;
  }

  public string Message { get; }
}

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
  public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(new Pong(request.Message + "-pong"));
  }
}
