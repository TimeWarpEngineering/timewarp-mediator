#region Purpose
// Requests and behaviors that exercise covariant send, await-before-next, and faulted ValueTask.
#endregion

#region Design
// YieldTwiceBehavior is closed over ScopeProbe so the open tracking behaviors stay the only
// wrappers on every other request. It awaits before next and calls next again so the chain
// cannot keep its scope in a thread-static slot. ScopeMarker is registered by the test.
#endregion

using System.Threading;

namespace TimeWarp.Mediator.Generators.Tests.Dispatch;

public interface ILabel
{
  string Text { get; }
}

public sealed class Label : ILabel
{
  public Label(string text)
  {
    Text = text;
  }

  public string Text { get; }
}

public sealed class SlowEcho : IRequest<string>
{
  public string Text { get; set; } = string.Empty;
}

public sealed class SlowEchoHandler : IRequestHandler<SlowEcho, string>
{
  public async ValueTask<string> Handle(SlowEcho request, CancellationToken cancellationToken)
  {
    await Task.Yield();
    return request.Text;
  }
}

public sealed class LabelQuery : IRequest<Label>
{
  public string Text { get; set; } = string.Empty;
}

public sealed class LabelQueryHandler : IRequestHandler<LabelQuery, Label>
{
  public async ValueTask<Label> Handle(LabelQuery request, CancellationToken cancellationToken)
  {
    await Task.Yield();
    return new Label(request.Text);
  }
}

public sealed class DualPing : IRequest<string>, IRequest<int>
{
}

public sealed class DualPingHandler : IRequestHandler<DualPing, string>
{
  public async ValueTask<string> Handle(DualPing request, CancellationToken cancellationToken)
  {
    await Task.Yield();
    return "s";
  }
}

public sealed class DualValue : IRequest<int>, IRequest<long>
{
}

public sealed class DualValueHandler : IRequestHandler<DualValue, int>
{
  public async ValueTask<int> Handle(DualValue request, CancellationToken cancellationToken)
  {
    await Task.Yield();
    return 7;
  }
}

public sealed class FaultingPing : IRequest<string>
{
}

public sealed class FaultingPingHandler : IRequestHandler<FaultingPing, string>
{
  public ValueTask<string> Handle(FaultingPing request, CancellationToken cancellationToken)
  {
    return ValueTask.FromException<string>(new InvalidOperationException("boom"));
  }
}

public sealed class FaultingNop : IRequest
{
}

public sealed class FaultingNopHandler : IRequestHandler<FaultingNop>
{
  public ValueTask Handle(FaultingNop request, CancellationToken cancellationToken)
  {
    return ValueTask.FromException(new InvalidOperationException("boom"));
  }
}

public sealed class CanceledNop : IRequest
{
}

public sealed class CanceledNopHandler : IRequestHandler<CanceledNop>
{
  public ValueTask Handle(CanceledNop request, CancellationToken cancellationToken)
  {
    return ValueTask.FromCanceled(new CancellationToken(true));
  }
}

public sealed class ScopeMarker
{
  private int HitCount;

  public Guid Id { get; } = Guid.NewGuid();

  public int Hits => Volatile.Read(ref HitCount);

  public void Hit()
  {
    Interlocked.Increment(ref HitCount);
  }
}

public sealed class ScopeProbe : IRequest<Guid>
{
}

public sealed class ScopeProbeHandler : IRequestHandler<ScopeProbe, Guid>
{
  private readonly ScopeMarker Marker;

  public ScopeProbeHandler(ScopeMarker marker)
  {
    Marker = marker;
  }

  public ValueTask<Guid> Handle(ScopeProbe request, CancellationToken cancellationToken)
  {
    Marker.Hit();
    return ValueTask.FromResult(Marker.Id);
  }
}

public sealed class YieldTwiceBehavior : IPipelineBehavior<ScopeProbe, Guid>
{
  public static Barrier? Gate { get; set; }

  public async ValueTask<Guid> Handle(
      ScopeProbe request,
      MessageHandlerDelegate<ScopeProbe, Guid> next,
      CancellationToken cancellationToken)
  {
    await Task.Yield();
    if (!Wait(Gate))
    {
      throw new TimeoutException("Concurrent sends did not meet.");
    }

    Guid first = await next(request, cancellationToken).ConfigureAwait(false);
    await Task.Yield();
    if (!Wait(Gate))
    {
      throw new TimeoutException("Concurrent sends did not meet.");
    }

    Guid second = await next(request, cancellationToken).ConfigureAwait(false);
    if (first != second)
    {
      throw new InvalidOperationException("Scope drifted.");
    }

    return second;
  }

  private static bool Wait(Barrier? gate)
  {
    if (gate is null)
    {
      return true;
    }

    return gate.SignalAndWait(TimeSpan.FromSeconds(5));
  }
}
