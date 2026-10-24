#region Purpose
// Runtime checks for covariant send, a scope-stable behavior chain, faulted tasks, and a read-only manifest.
#endregion

namespace TimeWarp.Mediator.Generators.Tests;

public class DispatchSoundnessTests
{
  [Fact]
  public async Task AsyncCovariantSend_ReturnsTheHandlerResult()
  {
    await using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

    string exact = await sender.Send(new SlowEcho { Text = "hi" });
    object asObject = await sender.Send((IRequest<object>) new SlowEcho { Text = "hi" });
    ILabel label = await sender.Send((IRequest<ILabel>) new LabelQuery { Text = "label" });

    exact.ShouldBe("hi");
    asObject.ShouldBe("hi");
    label.Text.ShouldBe("label");
  }

  [Fact]
  public async Task DualInterfaceSend_ThrowsInvalidCastExceptionForTheUnboundResponse()
  {
    await using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    DualPing dual = new();
    DualValue dualValue = new();

    (await sender.Send((IRequest<string>) dual)).ShouldBe("s");
    (await sender.Send((IRequest<int>) dualValue)).ShouldBe(7);
    await Should.ThrowAsync<InvalidCastException>(async () => await sender.Send((IRequest<int>) dual));
    await Should.ThrowAsync<InvalidCastException>(async () => await sender.Send((IRequest<long>) dualValue));
  }

  [Fact]
  public async Task Behavior_CanAwaitBeforeNext_AndCallNextAgain()
  {
    await using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    ScopeMarker marker = scope.ServiceProvider.GetRequiredService<ScopeMarker>();

    Guid id = await sender.Send(new ScopeProbe());

    id.ShouldBe(marker.Id);
    marker.Hits.ShouldBe(2);
  }

  [Fact]
  public async Task ConcurrentSends_KeepEachScope()
  {
    await using ServiceProvider provider = CreateProvider();
    using IServiceScope first = provider.CreateScope();
    using IServiceScope second = provider.CreateScope();
    YieldTwiceBehavior.Gate = new Barrier(2);
    try
    {
      await Task.WhenAll(Probe(first), Probe(second));
    }
    finally
    {
      YieldTwiceBehavior.Gate = null;
    }
  }

  [Fact]
  public async Task FaultedValueTask_StaysFaultedUntilAwaited()
  {
    await using ServiceProvider provider = CreateProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

    ValueTask<string> typed = Should.NotThrow(() => sender.Send(new FaultingPing()));
    ValueTask unit = Should.NotThrow(() => sender.Send(new FaultingNop()));
    ValueTask canceled = Should.NotThrow(() => sender.Send(new CanceledNop()));

    await Should.ThrowAsync<InvalidOperationException>(async () => await typed);
    await Should.ThrowAsync<InvalidOperationException>(async () => await unit);
    await Should.ThrowAsync<OperationCanceledException>(async () => await canceled);
  }

  [Fact]
  public void Manifest_RequestsAndBehaviorsCannotBeCastToArrays()
  {
    (MediatorManifest.Requests as MediatorManifest.RequestEntry[]).ShouldBeNull();
    MediatorManifest.Requests.ShouldNotBeEmpty();
    MediatorManifest.RequestEntry entry = MediatorManifest.Requests[0];
    (entry.Behaviors as string[]).ShouldBeNull();
    entry.Behaviors.ShouldBeAssignableTo<IReadOnlyList<string>>();
  }

  private static async Task Probe(IServiceScope scope)
  {
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    ScopeMarker marker = scope.ServiceProvider.GetRequiredService<ScopeMarker>();
    for (int i = 0; i < 8; i++)
    {
      Guid id = await sender.Send(new ScopeProbe());
      id.ShouldBe(marker.Id);
    }

    marker.Hits.ShouldBe(16);
  }

  private static ServiceProvider CreateProvider()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator();
    services.AddScoped<ScopeMarker>();
    return services.BuildServiceProvider();
  }
}
