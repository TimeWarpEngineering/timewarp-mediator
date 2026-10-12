// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Tests;

public class WrapperCacheTests
{
    public class MultiResponsePing : IRequest<string>, IRequest<int> { }

    public class MultiResponsePingStringHandler : IRequestHandler<MultiResponsePing, string>
    {
        public Task<string> Handle(MultiResponsePing request, CancellationToken cancellationToken) => Task.FromResult("pong");
    }

    public class MultiResponsePingIntHandler : IRequestHandler<MultiResponsePing, int>
    {
        public Task<int> Handle(MultiResponsePing request, CancellationToken cancellationToken) => Task.FromResult(42);
    }

    public class VoidAndResponsePing : IRequest, IRequest<string> { }

    public class VoidAndResponsePingVoidHandler : IRequestHandler<VoidAndResponsePing>
    {
        public bool Handled { get; private set; }

        public Task Handle(VoidAndResponsePing request, CancellationToken cancellationToken)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }

    public class VoidAndResponsePingStringHandler : IRequestHandler<VoidAndResponsePing, string>
    {
        public Task<string> Handle(VoidAndResponsePing request, CancellationToken cancellationToken) => Task.FromResult("pong");
    }

    public class MultiResponseStreamPing : IStreamRequest<string>, IStreamRequest<int> { }

    public class MultiResponseStreamPingStringHandler : IStreamRequestHandler<MultiResponseStreamPing, string>
    {
        public async IAsyncEnumerable<string> Handle(MultiResponseStreamPing request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            yield return "pong";
        }
    }

    public class MultiResponseStreamPingIntHandler : IStreamRequestHandler<MultiResponseStreamPing, int>
    {
        public async IAsyncEnumerable<int> Handle(MultiResponseStreamPing request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            yield return 42;
        }
    }

    [Fact]
    public async Task Should_dispatch_each_response_type_for_a_request_implementing_several_request_interfaces()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddTransient<IRequestHandler<MultiResponsePing, string>, MultiResponsePingStringHandler>()
            .AddTransient<IRequestHandler<MultiResponsePing, int>, MultiResponsePingIntHandler>()
            .BuildServiceProvider();
        Mediator mediator = new(serviceProvider);
        MultiResponsePing request = new();

        (await mediator.Send((object)request)).ShouldNotBeNull();
        (await mediator.Send<string>(request)).ShouldBe("pong");
        (await mediator.Send<int>(request)).ShouldBe(42);
    }

    [Fact]
    public async Task Should_dispatch_void_send_after_untyped_send_for_a_request_implementing_both_request_interfaces()
    {
        VoidAndResponsePingVoidHandler voidHandler = new();
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<IRequestHandler<VoidAndResponsePing>>(voidHandler)
            .AddTransient<IRequestHandler<VoidAndResponsePing, string>, VoidAndResponsePingStringHandler>()
            .BuildServiceProvider();
        Mediator mediator = new(serviceProvider);
        VoidAndResponsePing request = new();

        (await mediator.Send((object)request)).ShouldBe("pong");
        await mediator.Send(request);

        voidHandler.Handled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_stream_each_response_type_for_a_request_implementing_several_stream_interfaces()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddTransient<IStreamRequestHandler<MultiResponseStreamPing, string>, MultiResponseStreamPingStringHandler>()
            .AddTransient<IStreamRequestHandler<MultiResponseStreamPing, int>, MultiResponseStreamPingIntHandler>()
            .BuildServiceProvider();
        Mediator mediator = new(serviceProvider);
        MultiResponseStreamPing request = new();

        await foreach (object? _ in mediator.CreateStream((object)request)) { }

        List<int> ints = new();
        await foreach (int item in mediator.CreateStream<int>(request))
        {
            ints.Add(item);
        }

        List<string> strings = new();
        await foreach (string item in mediator.CreateStream<string>(request))
        {
            strings.Add(item);
        }

        ints.ShouldBe(new[] { 42 });
        strings.ShouldBe(new[] { "pong" });
    }
}
