// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.ExceptionHandler.Overrides;

public class CommonExceptionHandler : IRequestExceptionHandler<PingResourceTimeout, Pong, Exception>
{
  private readonly TextWriter TextWriter;

  public CommonExceptionHandler(TextWriter writer) => TextWriter = writer;

  public async Task Handle(PingResourceTimeout request,
      Exception exception,
      RequestExceptionHandlerState<Pong> state,
      CancellationToken cancellationToken)
  {
    // Exception type name must be written in messages by LogExceptionAction before
    // Exception handler type name required because it is checked later in messages
    await TextWriter.WriteLineAsync($"---- Exception Handler: '{typeof(CommonExceptionHandler).FullName}'").ConfigureAwait(false);

    state.SetHandled(new Pong());
  }
}

public class ServerExceptionHandler : ExceptionHandler.ServerExceptionHandler
{
  private readonly TextWriter TextWriter;

  public ServerExceptionHandler(TextWriter writer) : base(writer) => TextWriter = writer;

  public override async Task Handle(PingNewResource request,
      ServerException exception,
      RequestExceptionHandlerState<Pong> state,
      CancellationToken cancellationToken)
  {
    // Exception type name must be written in messages by LogExceptionAction before
    // Exception handler type name required because it is checked later in messages
    await TextWriter.WriteLineAsync($"---- Exception Handler: '{typeof(ServerExceptionHandler).FullName}'").ConfigureAwait(false);

    state.SetHandled(new Pong());
  }
}