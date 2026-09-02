// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class SendVoidInterfaceTests
{
    public class Ping : IRequest
    {
        public string? Message { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping>
    {
        private readonly TextWriter _writer;

        public PingHandler(TextWriter writer) => _writer = writer;

        public Task Handle(Ping request, CancellationToken cancellationToken)
            => _writer.WriteAsync(request.Message + " Pong");
    }

    [Fact]
    public async Task Should_resolve_main_void_handler()
    {
        StringBuilder builder = new();
        StringWriter writer = new(builder);

        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(PublishTests));
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof (IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof (IRequestHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IMediator>().Use<Mediator>();
        });


        IMediator med = container.GetInstance<IMediator>();

        await med.Send(new Ping { Message = "Ping" });

        builder.ToString().ShouldBe("Ping Pong");
    }
}
