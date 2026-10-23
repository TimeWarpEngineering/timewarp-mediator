// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class PingedHandler : INotificationHandler<Pinged>
{
  private readonly TextWriter TextWriter;

  public PingedHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return new ValueTask(TextWriter.WriteLineAsync("Got pinged async."));
  }
}

public class PongedHandler : INotificationHandler<Ponged>
{
  private readonly TextWriter TextWriter;

  public PongedHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public ValueTask Handle(Ponged notification, CancellationToken cancellationToken)
  {
    return new ValueTask(TextWriter.WriteLineAsync("Got ponged async."));
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

  public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
  {
    return new ValueTask(TextWriter.WriteLineAsync("Got pinged constrained async."));
  }
}

public class PingedAlsoHandler : INotificationHandler<Pinged>
{
  private readonly TextWriter TextWriter;

  public PingedAlsoHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
  {
    return new ValueTask(TextWriter.WriteLineAsync("Got pinged also async."));
  }
}