// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Benchmarks;

public class Pinged : INotification
{
}

public class PingedHandler : INotificationHandler<Pinged>
{
  public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return default;
  }
}