#region Purpose
// One host with ClientPipeline and ServerPipeline: disjoint handlers, behaviors, and ISender<TScope>.
#endregion

internal static class Program
{
    private static async Task<int> Main()
    {
        ServiceCollection services = new();
        services.AddGeneratedMediator();
        services.AddGeneratedMediator<ClientPipeline>();
        services.AddGeneratedMediator<ServerPipeline>();
        ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IServiceProvider scoped = scope.ServiceProvider;

        ISender unscoped = scoped.GetRequiredService<ISender>();
        ISender<ClientPipeline> client = scoped.GetRequiredService<ISender<ClientPipeline>>();
        ISender<ServerPipeline> server = scoped.GetRequiredService<ISender<ServerPipeline>>();

        string clientResult = await client.Send(new ClientPing("hello"));
        string serverResult = await server.Send(new ServerPing("hello"));

        bool clientMissed = false;
        try
        {
            await client.Send((object)new ServerPing("nope"));
        }
        catch (NoHandlerException)
        {
            clientMissed = true;
        }

        bool unscopedMissed = false;
        try
        {
            await unscoped.Send((object)new ClientPing("nope"));
        }
        catch (NoHandlerException)
        {
            unscopedMissed = true;
        }

        if (clientResult != "client:hello"
            || serverResult != "server:hello"
            || !clientMissed
            || !unscopedMissed
            || !ClientStamp.Ran
            || !ServerStamp.Ran
            || ServerStamp.RanOnClient
            || ClientStamp.RanOnServer)
        {
            return 1;
        }

        Console.WriteLine(clientResult);
        Console.WriteLine(serverResult);
        return 0;
    }
}

public sealed class ClientPipeline
{
}

public sealed class ServerPipeline
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPing : IRequest<string>
{
    public ClientPing(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPingHandler : IRequestHandler<ClientPing, string>
{
    public Task<string> Handle(ClientPing request, CancellationToken cancellationToken)
    {
        return Task.FromResult("client:" + request.Message);
    }
}

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerPing : IRequest<string>
{
    public ServerPing(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerPingHandler : IRequestHandler<ServerPing, string>
{
    public Task<string> Handle(ServerPing request, CancellationToken cancellationToken)
    {
        return Task.FromResult("server:" + request.Message);
    }
}

public static class ClientStamp
{
    public static bool Ran { get; set; }

    public static bool RanOnServer { get; set; }
}

public static class ServerStamp
{
    public static bool Ran { get; set; }

    public static bool RanOnClient { get; set; }
}

public sealed class ClientStampBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ClientStamp.Ran = true;
        if (request is ServerPing)
        {
            ClientStamp.RanOnServer = true;
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}

public sealed class ServerStampBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ServerStamp.Ran = true;
        if (request is ClientPing)
        {
            ServerStamp.RanOnClient = true;
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
