#region Purpose
// Unscoped and ClientPipeline notifications for the IPublisher benchmark rows.
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

public sealed class Pinged : INotification
{
}

public sealed class PingedHandler : INotificationHandler<Pinged>
{
  public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return default;
  }
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPinged : INotification
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPingedHandler : INotificationHandler<ClientPinged>
{
  public ValueTask Handle(ClientPinged notification, CancellationToken cancellationToken)
  {
    return default;
  }
}
