#region Purpose
// Compares legacy MakeGenericType, generated interface and concrete Send, scoped send,
// publish, State and Architecture pipelines, CallSiteInlining Dispatch_*, and martinothamar.
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 8)]
public class DispatchBenchmarks
{
  private IMediator LegacyMediator = null!;
  private TimeWarp.Mediator.Generated.Mediator GeneratedMediator = null!;
  private IServiceProvider GeneratedScope = null!;
  private ISender GeneratedSender = null!;
  private ISender<ClientPipeline> ScopedSender = null!;
  private IPublisher Publisher = null!;
  private IPublisher<ClientPipeline> ScopedPublisher = null!;
  private global::Mediator.IMediator MartinothamarMediator = null!;
  private readonly Nop NopRequest = new();
  private readonly Ping Request = new() { Message = "hi" };
  private readonly ScopedPing ScopedRequest = new() { Message = "hi" };
  private readonly Pinged PingedNotification = new();
  private readonly ClientPinged ClientPingedNotification = new();
  private readonly Increment IncrementRequest = new();
  private readonly UpdateProfile UpdateProfileRequest = new();
  private readonly MartinPing MartinRequest = new() { Message = "hi" };

  [GlobalSetup]
  public void Setup()
  {
    ServiceCollection legacyServices = new();
    legacyServices.AddMediator(cfg => cfg.RegisterServicesFromAssemblyContaining<Ping>());
    IServiceProvider legacyProvider = legacyServices.BuildServiceProvider();
    LegacyMediator = legacyProvider.GetRequiredService<IMediator>();

    ServiceCollection generatedServices = new();
    generatedServices.AddGeneratedMediator();
    generatedServices.AddGeneratedMediator<ClientPipeline>();
    GeneratedScope = generatedServices.BuildServiceProvider().CreateScope().ServiceProvider;
    GeneratedMediator = GeneratedScope.GetRequiredService<TimeWarp.Mediator.Generated.Mediator>();

    GeneratedSender = GeneratedScope.GetRequiredService<ISender>();
    ScopedSender = GeneratedScope.GetRequiredService<ISender<ClientPipeline>>();
    Publisher = GeneratedScope.GetRequiredService<IPublisher>();
    ScopedPublisher = GeneratedScope.GetRequiredService<IPublisher<ClientPipeline>>();

    ServiceCollection martinServices = new();
    martinServices.AddMediator();
    MartinothamarMediator = martinServices.BuildServiceProvider().GetRequiredService<Mediator.IMediator>();
  }

  [Benchmark(Baseline = true)]
  public Task<Pong> TimeWarpLegacyMakeGenericType()
  {
    return LegacyMediator.Send(Request).AsTask();
  }

  [Benchmark]
  public ValueTask<Pong> TimeWarpGeneratedMonomorphicSend()
  {
    return GeneratedMediator.Send(Request);
  }

  [Benchmark]
  public ValueTask<Pong> TimeWarpGeneratedInterfaceSend()
  {
    return GeneratedSender.Send(Request);
  }

  [Benchmark]
  public Task<object?> TimeWarpGeneratedObjectSend()
  {
    return GeneratedSender.Send((object)Request).AsTask();
  }

  [Benchmark]
  public ValueTask TimeWarpGeneratedInterfaceSendUnit()
  {
    return GeneratedSender.Send(NopRequest);
  }

  [Benchmark]
  public ValueTask<Unit> TimeWarpGeneratedMonomorphicSendUnit()
  {
    return GeneratedMediator.Send(NopRequest);
  }

  [Benchmark]
  public ValueTask<Pong> TimeWarpCallSiteInliningPrototype()
  {
    return TimeWarp.Mediator.Generated.Mediator.Dispatch_global__MediatorBenchmarks_Ping(
        GeneratedScope,
        Request,
        default);
  }

  [Benchmark]
  public ValueTask<Pong> TimeWarpGeneratedScopedSend()
  {
    return ScopedSender.Send(ScopedRequest);
  }

  [Benchmark]
  public ValueTask TimeWarpGeneratedInterfacePublish()
  {
    return Publisher.Publish(PingedNotification);
  }

  [Benchmark]
  public ValueTask TimeWarpGeneratedScopedPublish()
  {
    return ScopedPublisher.Publish(ClientPingedNotification);
  }

  [Benchmark]
  public ValueTask TimeWarpGeneratedStatePipeline()
  {
    return ScopedSender.Send(IncrementRequest);
  }

  [Benchmark]
  public ValueTask<OneOf<ProfileResponse, ProblemDetails>> TimeWarpGeneratedArchitectureSend()
  {
    return GeneratedSender.Send(UpdateProfileRequest);
  }

  [Benchmark]
  public ValueTask<MartinPong> MartinothamarGenerated()
  {
    return MartinothamarMediator.Send(MartinRequest);
  }
}