#region Purpose
// Compares legacy MakeGenericType, generated IMediator.Send, CallSiteInlining Dispatch_*, martinothamar.
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
  private global::Mediator.IMediator MartinothamarMediator = null!;
  private readonly Nop NopRequest = new();
  private readonly Ping Request = new() { Message = "hi" };
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
    GeneratedScope = generatedServices.BuildServiceProvider().CreateScope().ServiceProvider;
    GeneratedMediator = GeneratedScope.GetRequiredService<TimeWarp.Mediator.Generated.Mediator>();

    GeneratedSender = GeneratedScope.GetRequiredService<ISender>();

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
  public Task<Pong> TimeWarpGeneratedInterfaceSend()
  {
    return GeneratedSender.Send(Request).AsTask();
  }

  [Benchmark]
  public Task<object?> TimeWarpGeneratedObjectSend()
  {
    return GeneratedSender.Send((object)Request).AsTask();
  }

  [Benchmark]
  public Task TimeWarpGeneratedInterfaceSendUnit()
  {
    return GeneratedSender.Send(NopRequest).AsTask();
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
  public ValueTask<MartinPong> MartinothamarGenerated()
  {
    return MartinothamarMediator.Send(MartinRequest);
  }
}