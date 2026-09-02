// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class NotificationHandlerTests
{
    public class Ping : INotification
    {
        public string? Message { get; set; }
    }

    public class PongChildHandler : NotificationHandler<Ping>
    {
        private readonly TextWriter _writer;

        public PongChildHandler(TextWriter writer)
        {
            _writer = writer;
        }

        protected override void Handle(Ping notification)
        {
            _writer.WriteLine(notification.Message + " Pong");
        }
    }

    [Fact]
    public async Task Should_call_abstract_handle_method()
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
