// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests;

public class Ping : IRequest<Pong>
{
  public string? Message { get; init; }
  public Action<Ping>? ThrowAction { get; init; }
}

public class DerivedPing : Ping
{
}

public class Pong
{
  public string? Message { get; init; }
}

public class Zing : IRequest<Zong>
{
  public string? Message { get; init; }
}

public class Zong
{
  public string? Message { get; init; }
}

public class Ding : IRequest
{
  public string? Message { get; init; }
}

public class Pinged : INotification
{

}

class InternalPing : IRequest { }

public class GenericHandler : INotificationHandler<INotification>
{
  public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
  {
    return default;
  }
}

public class DingAsyncHandler : IRequestHandler<Ding>
{
  public ValueTask Handle(Ding message, CancellationToken cancellationToken) => default;
}

public class PingedHandler : INotificationHandler<Pinged>
{
  public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return default;
  }
}

public class PingedAlsoHandler : INotificationHandler<Pinged>
{
  public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return default;
  }
}

public class PingedAlsoOpenHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : Pinged
{
  public ValueTask Handle(TNotification notification, CancellationToken cancellationToken) => default;
}

public class Logger
{
  public IList<string> Messages { get; } = new List<string>();
}

public class PingHandler : IRequestHandler<Ping, Pong>
{
  private readonly Logger Logger;

  public PingHandler(Logger logger)
  {
    Logger = logger;
  }
  public ValueTask<Pong> Handle(Ping message, CancellationToken cancellationToken)
  {
    Logger.Messages.Add("Handler");

    message.ThrowAction?.Invoke(message);

    return ValueTask.FromResult(new Pong { Message = message.Message + " Pong" });
  }
}

public class DerivedPingHandler : IRequestHandler<DerivedPing, Pong>
{
  private readonly Logger Logger;

  public DerivedPingHandler(Logger logger)
  {
    Logger = logger;
  }
  public ValueTask<Pong> Handle(DerivedPing message, CancellationToken cancellationToken)
  {
    Logger.Messages.Add("Handler");
    return ValueTask.FromResult(new Pong { Message = $"Derived{message.Message} Pong" });
  }
}

public class ZingHandler : IRequestHandler<Zing, Zong>
{
  private readonly Logger Logger;

  public ZingHandler(Logger output)
  {
    Logger = output;
  }
  public ValueTask<Zong> Handle(Zing message, CancellationToken cancellationToken)
  {
    Logger.Messages.Add("Handler");
    return ValueTask.FromResult(new Zong { Message = message.Message + " Zong" });
  }
}

public class DuplicateTest : IRequest<string> { }
public class DuplicateHandler1 : IRequestHandler<DuplicateTest, string>
{
  public ValueTask<string> Handle(DuplicateTest message, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(nameof(DuplicateHandler1));
  }
}

public class DuplicateHandler2 : IRequestHandler<DuplicateTest, string>
{
  public ValueTask<string> Handle(DuplicateTest message, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(nameof(DuplicateHandler2));
  }
}

class InternalPingHandler : IRequestHandler<InternalPing>
{
  public ValueTask Handle(InternalPing request, CancellationToken cancellationToken) => default;
}

class MyCustomMediator : IMediator
{
  public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
      where TNotification : INotification
  {
    throw new NotImplementedException();
  }

  public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
      where TRequest : IRequest
  {
    throw new NotImplementedException();
  }
}

interface ITypeArgument { }
class ConcreteTypeArgument : ITypeArgument { }
class OpenGenericVoidRequest<T> : IRequest
    where T : class, ITypeArgument
{ }
class OpenGenericVoidRequestHandler<T> : IRequestHandler<OpenGenericVoidRequest<T>>
    where T : class, ITypeArgument
{
  public ValueTask Handle(OpenGenericVoidRequest<T> request, CancellationToken cancellationToken) => default;
}
class OpenGenericReturnTypeRequest<T> : IRequest<string>
    where T : class, ITypeArgument
{ }
class OpenGenericReturnTypeRequestHandler<T> : IRequestHandler<OpenGenericReturnTypeRequest<T>, string>
    where T : class, ITypeArgument
{
  public ValueTask<string> Handle(OpenGenericReturnTypeRequest<T> request, CancellationToken cancellationToken) => ValueTask.FromResult(nameof(request));
}

public class GenericPing<T> : IRequest<T>
    where T : Pong
{
  public T? Pong { get; set; }
}

public class GenericPingHandler<T> : IRequestHandler<GenericPing<T>, T>
    where T : Pong
{
  public ValueTask<T> Handle(GenericPing<T> request, CancellationToken cancellationToken) => ValueTask.FromResult(request.Pong!);
}

public class VoidGenericPing<T> : IRequest
    where T : Pong
{ }

public class VoidGenericPingHandler<T> : IRequestHandler<VoidGenericPing<T>>
    where T : Pong
{
  public ValueTask Handle(VoidGenericPing<T> request, CancellationToken cancellationToken) => default;
}