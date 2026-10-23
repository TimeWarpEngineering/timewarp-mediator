#region Purpose
// Server-scoped requests, handlers, and notifications for named-pipeline tests.
#endregion

namespace TimeWarp.Mediator.Generators.Tests.Pipelines;

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerPing : IRequest<string>
{
  public string Message { get; set; } = string.Empty;
}

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerPingHandler : IRequestHandler<ServerPing, string>
{
  public ValueTask<string> Handle(ServerPing request, CancellationToken cancellationToken)
  {
    return ValueTask.FromResult("server:" + request.Message);
  }
}

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerNote : INotification
{
}

[MediatorScope(typeof(ServerPipeline))]
public sealed class ServerNoteHandler : INotificationHandler<ServerNote>
{
  public static int Count;

  public ValueTask Handle(ServerNote notification, CancellationToken cancellationToken)
  {
    Count++;
    return default;
  }
}