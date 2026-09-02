// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class ServiceFactoryTests
{
    public class Ping : IRequest<Pong>
    {

    }

    public class Pong
    {
        public string? Message { get; set; }
    }

    [Fact]
    public async Task Should_throw_given_no_handler()
    {
        ServiceCollection serviceCollection = new();
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        Mediator med = new(serviceProvider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => med.Send(new Ping())
        );
    }
}
