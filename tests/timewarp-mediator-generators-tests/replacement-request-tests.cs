#region Purpose
// A behavior may call next with a different request. The generated nested delegate must forward that instance.
#endregion

[assembly: MediatorBehavior(
  typeof(TimeWarp.Mediator.Generators.Tests.Replacement.LeafReplacementBehavior),
  Scope = typeof(TimeWarp.Mediator.Generators.Tests.Replacement.ReplacementPipeline))]
[assembly: MediatorBehavior(
  typeof(TimeWarp.Mediator.Generators.Tests.Replacement.ChainOuterBehavior),
  Scope = typeof(TimeWarp.Mediator.Generators.Tests.Replacement.ReplacementPipeline))]
[assembly: MediatorBehavior(
  typeof(TimeWarp.Mediator.Generators.Tests.Replacement.ChainInnerBehavior),
  1,
  Scope = typeof(TimeWarp.Mediator.Generators.Tests.Replacement.ReplacementPipeline))]

namespace TimeWarp.Mediator.Generators.Tests.Replacement;

public sealed class ReplacementPipeline
{
}

[MediatorScope(typeof(ReplacementPipeline))]
public sealed class LeafRequest : IRequest<string>
{
  public string Marker { get; set; } = string.Empty;
}

[MediatorScope(typeof(ReplacementPipeline))]
public sealed class LeafRequestHandler : IRequestHandler<LeafRequest, string>
{
  public static LeafRequest? Received { get; set; }

  public ValueTask<string> Handle(LeafRequest request, CancellationToken cancellationToken)
  {
    Received = request;
    return ValueTask.FromResult(request.Marker);
  }
}

public sealed class LeafReplacementBehavior : IPipelineBehavior<LeafRequest, string>
{
  public static LeafRequest Replacement { get; } = new() { Marker = "replaced" };

  public ValueTask<string> Handle(
    LeafRequest request,
    MessageHandlerDelegate<LeafRequest, string> next,
    CancellationToken cancellationToken)
  {
    return next(Replacement, cancellationToken);
  }
}

[MediatorScope(typeof(ReplacementPipeline))]
public sealed class ChainRequest : IRequest<string>
{
  public string Marker { get; set; } = string.Empty;
}

[MediatorScope(typeof(ReplacementPipeline))]
public sealed class ChainRequestHandler : IRequestHandler<ChainRequest, string>
{
  public static ChainRequest? Received { get; set; }

  public ValueTask<string> Handle(ChainRequest request, CancellationToken cancellationToken)
  {
    Received = request;
    return ValueTask.FromResult(request.Marker);
  }
}

public sealed class ChainOuterBehavior : IPipelineBehavior<ChainRequest, string>
{
  public static ChainRequest Replacement { get; } = new() { Marker = "replaced" };

  public ValueTask<string> Handle(
    ChainRequest request,
    MessageHandlerDelegate<ChainRequest, string> next,
    CancellationToken cancellationToken)
  {
    return next(Replacement, cancellationToken);
  }
}

public sealed class ChainInnerBehavior : IPipelineBehavior<ChainRequest, string>
{
  public static ChainRequest? Seen { get; set; }

  public ValueTask<string> Handle(
    ChainRequest request,
    MessageHandlerDelegate<ChainRequest, string> next,
    CancellationToken cancellationToken)
  {
    Seen = request;
    return next(request, cancellationToken);
  }
}

public class ReplacementRequestTests
{
  [Fact]
  public async Task SingleBehavior_ForwardsReplacementRequestToHandler()
  {
    LeafRequestHandler.Received = null;
    LeafRequest original = new() { Marker = "original" };

    string marker = await Send(original);

    marker.ShouldBe("replaced");
    LeafRequestHandler.Received.ShouldBeSameAs(LeafReplacementBehavior.Replacement);
    LeafRequestHandler.Received.ShouldNotBeSameAs(original);
  }

  [Fact]
  public async Task TwoBehaviorChain_ForwardsReplacementThroughNestedDelegate()
  {
    ChainInnerBehavior.Seen = null;
    ChainRequestHandler.Received = null;
    ChainRequest original = new() { Marker = "original" };

    string marker = await Send(original);

    marker.ShouldBe("replaced");
    ChainInnerBehavior.Seen.ShouldBeSameAs(ChainOuterBehavior.Replacement);
    ChainRequestHandler.Received.ShouldBeSameAs(ChainOuterBehavior.Replacement);
    ChainRequestHandler.Received.ShouldNotBeSameAs(original);
  }

  private static async Task<string> Send<TRequest>(TRequest request)
    where TRequest : IRequest<string>
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator();
    services.AddGeneratedMediator<ReplacementPipeline>();
    using ServiceProvider provider = services.BuildServiceProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender<ReplacementPipeline> sender = scope.ServiceProvider.GetRequiredService<ISender<ReplacementPipeline>>();
    return await sender.Send(request);
  }
}
