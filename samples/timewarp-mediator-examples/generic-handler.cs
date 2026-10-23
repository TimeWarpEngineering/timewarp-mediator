// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class GenericHandler : INotificationHandler<INotification>
{
  private readonly TextWriter TextWriter;

  public GenericHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public ValueTask Handle(INotification notification, CancellationToken cancellationToken)
  {
    return new ValueTask(TextWriter.WriteLineAsync("Got notified."));
  }
}