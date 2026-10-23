// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests.Included;

public class Foo : IRequest<Bar>
{
  public string? Message { get; init; }
  public Action<Foo>? ThrowAction { get; init; }
}

public class Bar
{
  public string? Message { get; init; }
}

public class FooHandler : IRequestHandler<Foo, Bar>
{
  private readonly Logger Logger;

  public FooHandler(Logger logger)
  {
    Logger = logger;
  }
  public ValueTask<Bar> Handle(Foo message, CancellationToken cancellationToken)
  {
    Logger.Messages.Add("Handler");

    message.ThrowAction?.Invoke(message);

    return ValueTask.FromResult(new Bar { Message = message.Message + " Bar" });
  }
}