// Modified by Steven T. Cramer

namespace TimeWarp.Mediator.Examples.ExceptionHandler;

public class PingResourceHandler : IRequestHandler<PingResource, Pong>
{
  private readonly TextWriter TextWriter;

  public PingResourceHandler(TextWriter writer) => TextWriter = writer;

  public ValueTask<Pong> Handle(PingResource request, CancellationToken cancellationToken)
  {
    throw new ResourceNotFoundException();
  }
}

public class PingNewResourceHandler : IRequestHandler<PingNewResource, Pong>
{
  private readonly TextWriter TextWriter;

  public PingNewResourceHandler(TextWriter writer) => TextWriter = writer;

  public ValueTask<Pong> Handle(PingNewResource request, CancellationToken cancellationToken)
  {
    throw new ServerException();
  }
}

public class PingResourceTimeoutHandler : IRequestHandler<PingResourceTimeout, Pong>
{
  private readonly TextWriter TextWriter;

  public PingResourceTimeoutHandler(TextWriter writer) => TextWriter = writer;

  public ValueTask<Pong> Handle(PingResourceTimeout request, CancellationToken cancellationToken)
  {
    throw new TaskCanceledException();
  }
}

public class PingResourceTimeoutOverrideHandler : IRequestHandler<Overrides.PingResourceTimeout, Pong>
{
  private readonly TextWriter TextWriter;

  public PingResourceTimeoutOverrideHandler(TextWriter writer) => TextWriter = writer;

  public ValueTask<Pong> Handle(Overrides.PingResourceTimeout request, CancellationToken cancellationToken)
  {
    throw new TaskCanceledException();
  }
}

public class PingProtectedResourceHandler : IRequestHandler<PingProtectedResource, Pong>
{
  private readonly TextWriter TextWriter;

  public PingProtectedResourceHandler(TextWriter writer) => TextWriter = writer;

  public ValueTask<Pong> Handle(PingProtectedResource request, CancellationToken cancellationToken)
  {
    throw new ForbiddenException();
  }
}