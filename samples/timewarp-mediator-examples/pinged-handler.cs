// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class PingedHandler : INotificationHandler<Pinged>
{
  private readonly TextWriter TextWriter;

  public PingedHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("Got pinged async.");
  }
}

public class PongedHandler : INotificationHandler<Ponged>
{
  private readonly TextWriter TextWriter;

  public PongedHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Handle(Ponged notification, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("Got ponged async.");
  }
}

public class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : Pinged
{
  private readonly TextWriter TextWriter;

  public ConstrainedPingedHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Handle(TNotification notification, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("Got pinged constrained async.");
  }
}

public class PingedAlsoHandler : INotificationHandler<Pinged>
{
  private readonly TextWriter TextWriter;

  public PingedAlsoHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("Got pinged also async.");
  }
}