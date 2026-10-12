// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples;

public class GenericHandler : INotificationHandler<INotification>
{
  private readonly TextWriter TextWriter;

  public GenericHandler(TextWriter writer)
  {
    TextWriter = writer;
  }

  public Task Handle(INotification notification, CancellationToken cancellationToken)
  {
    return TextWriter.WriteLineAsync("Got notified.");
  }
}