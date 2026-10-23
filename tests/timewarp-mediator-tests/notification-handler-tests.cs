// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class NotificationHandlerTests
{
  public class Ping : INotification
  {
    public string? Message { get; set; }
  }

  public class PongChildHandler : INotificationHandler<Ping>
  {
    private readonly TextWriter TextWriter;

    public PongChildHandler(TextWriter writer)
    {
      TextWriter = writer;
    }

    public ValueTask Handle(Ping notification, CancellationToken cancellationToken)
    {
      TextWriter.WriteLine(notification.Message + " Pong");
      return default;
    }
  }

  [Fact]
  public async Task Should_call_handle_method()
  {
    StringBuilder builder = new();
    StringWriter writer = new(builder);

    INotificationHandler<Ping> handler = new PongChildHandler(writer);

    await handler.Handle(
        new Ping() { Message = "Ping" },
        default
    );

    string result = builder.ToString();
    result.ShouldContain("Ping Pong");
  }
}