namespace TimeWarp.Mediator.Tests;

public class SendIdempotentCommandTests
{
    public class Upsert : IIdempotentCommand<string>
    {
        public string? Message { get; set; }
    }

    public class UpsertHandler : IIdempotentCommandHandler<Upsert, string>
    {
        public Task<string> Handle(Upsert request, CancellationToken cancellationToken)
            => Task.FromResult(request.Message + " Upserted");
    }

    public class Reset : IIdempotentCommand
    {
        public string? Message { get; set; }
    }

    public class ResetHandler : IIdempotentCommandHandler<Reset>
    {
        private readonly TextWriter _writer;

        public ResetHandler(TextWriter writer) => _writer = writer;

        public Task Handle(Reset request, CancellationToken cancellationToken)
            => _writer.WriteAsync(request.Message + " Reset");
    }

    [Fact]
    public async Task Should_resolve_generic_idempotent_command_handler()
    {
        IMediator mediator = CreateMediator(new StringWriter());

        string response = await mediator.Send(new Upsert { Message = "Setting" });

        response.ShouldBe("Setting Upserted");
    }

    [Fact]
    public async Task Should_resolve_void_idempotent_command_handler()
    {
        StringBuilder builder = new();
        IMediator mediator = CreateMediator(new StringWriter(builder));

        await mediator.Send(new Reset { Message = "Setting" });

        builder.ToString().ShouldBe("Setting Reset");
    }

    private static IMediator CreateMediator(TextWriter writer)
    {
        Container container = new(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(SendIdempotentCommandTests));
                scanner.IncludeNamespaceContainingType<Upsert>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
            });
            cfg.For<TextWriter>().Use(writer);
            cfg.For<IMediator>().Use<Mediator>();
        });

        return container.GetInstance<IMediator>();
    }
}
