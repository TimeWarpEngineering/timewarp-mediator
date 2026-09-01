#region Purpose
// Client-scoped requests, handlers, and notifications for named-pipeline tests.
#endregion

using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Mediator;

namespace TimeWarp.Mediator.Generators.Tests.Pipelines;

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPing : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientPingHandler : IRequestHandler<ClientPing, string>
{
    public Task<string> Handle(ClientPing request, CancellationToken cancellationToken)
    {
        return Task.FromResult("client:" + request.Message);
    }
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientReentrant : IRequest<int>
{
    public int Value { get; set; }
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientReentrantHandler : IRequestHandler<ClientReentrant, int>
{
    private readonly ISender<ClientPipeline> Sender;

    public ClientReentrantHandler(ISender<ClientPipeline> sender)
    {
        Sender = sender;
    }

    public async Task<int> Handle(ClientReentrant request, CancellationToken cancellationToken)
    {
        if (request.Value == 0)
        {
            int inner = await Sender.Send(new ClientReentrant { Value = 1 }, cancellationToken).ConfigureAwait(false);
            return inner + 1;
        }

        return request.Value;
    }
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientNote : INotification
{
}

[MediatorScope(typeof(ClientPipeline))]
public sealed class ClientNoteHandler : INotificationHandler<ClientNote>
{
    public static int Count;

    public Task Handle(ClientNote notification, CancellationToken cancellationToken)
    {
        Count++;
        return Task.CompletedTask;
    }
}
