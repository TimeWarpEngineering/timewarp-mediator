// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Extensions.Microsoft.DependencyInjection.Tests.Included;

public class Foo : IRequest<Bar>
{
    public string? Message { get; init; }
    public Action<Foo>? ThrowAction { get; init; }
}

public class Bar
{
    public string? Message { get; init; }
}

public class FooHandler : IRequestHandler<Foo, Bar>
{
    private readonly Logger _logger;

    public FooHandler(Logger logger)
    {
        _logger = logger;
    }
    public Task<Bar> Handle(Foo message, CancellationToken cancellationToken)
    {
        _logger.Messages.Add("Handler");

        message.ThrowAction?.Invoke(message);

        return Task.FromResult(new Bar { Message = message.Message + " Bar" });
    }
}
