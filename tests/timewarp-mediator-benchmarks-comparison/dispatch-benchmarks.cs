extern alias scopedmartin;

#region Purpose
// Compares generated interface and concrete Send, scoped send, publish, State and
// Architecture pipelines, CallSiteInlining Dispatch_*, martinothamar, and MediatR.
#endregion

#region Design
// Each external row names its TimeWarp pair in a comment. MartinothamarGenerated is
// Transient, matching AddGeneratedMediator(). The scoped martinothamar row lives in
// another compilation because that generator emits one ServiceLifetime. A singleton
// TimeWarp row was measured and was not faster than this cached transient path, so it
// is not part of the suite. MediatR is excluded from the PublishAot build (BENCHMARK_AOT).
#endregion

using TimeWarp.Mediator;

namespace MediatorBenchmarks;

#if !BENCHMARK_AOT
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 8)]
#endif
public class DispatchBenchmarks
{
  private TimeWarp.Mediator.Generated.Mediator GeneratedMediator = null!;
  private IServiceProvider GeneratedScope = null!;
  private ISender GeneratedSender = null!;
  private ISender<ClientPipeline> ScopedSender = null!;
  private IPublisher Publisher = null!;
  private IPublisher<ClientPipeline> ScopedPublisher = null!;
  private global::Mediator.IMediator MartinothamarMediator = null!;
  private global::Mediator.IMediator MartinScopedMediator = null!;
  private IServiceScope MartinScopedScope = null!;
#if !BENCHMARK_AOT
  private MediatR.IMediator MediatRMediator = null!;
#endif
  private readonly Nop NopRequest = new();
  private readonly Ping Request = new() { Message = "hi" };
  private readonly ScopedPing ScopedRequest = new() { Message = "hi" };
  private readonly Pinged PingedNotification = new();
  private readonly ClientPinged ClientPingedNotification = new();
  private readonly Increment IncrementRequest = new();
  private readonly UpdateProfile UpdateProfileRequest = new();
  private readonly MartinPing MartinRequest = new() { Message = "hi" };
  private readonly scopedmartin::MartinothamarScopedBenchmarks.MartinScopedPing MartinScopedRequest = new() { Message = "hi" };

#if !BENCHMARK_AOT
  [GlobalSetup]
#endif
  public void Setup()
  {
    ServiceCollection generatedServices = new();
    generatedServices.AddScoped<scopedmartin::BenchmarkPeers.ScopeToken>();
    generatedServices.AddGeneratedMediator();
    generatedServices.AddGeneratedMediator<ClientPipeline>();
    GeneratedScope = generatedServices.BuildServiceProvider().CreateScope().ServiceProvider;
    GeneratedMediator = GeneratedScope.GetRequiredService<TimeWarp.Mediator.Generated.Mediator>();

    GeneratedSender = GeneratedScope.GetRequiredService<ISender>();
    ScopedSender = GeneratedScope.GetRequiredService<ISender<ClientPipeline>>();
    Publisher = GeneratedScope.GetRequiredService<IPublisher>();
    ScopedPublisher = GeneratedScope.GetRequiredService<IPublisher<ClientPipeline>>();

    ServiceCollection martinServices = new();
    // Assemblies limits this compilation's generator to MartinPing. Empty means every
    // referenced assembly, which would emit the extern-aliased satellite handlers.
    martinServices.AddMediator(options =>
    {
      options.ServiceLifetime = ServiceLifetime.Transient;
      options.Assemblies = [typeof(MartinPing)];
    });
    MartinothamarMediator = martinServices.BuildServiceProvider().GetRequiredService<global::Mediator.IMediator>();

    ServiceCollection martinScopedServices = new();
    scopedmartin::MartinothamarScopedBenchmarks.MartinScopedRegistration.Add(martinScopedServices);
    MartinScopedScope = martinScopedServices.BuildServiceProvider().CreateScope();
    MartinScopedMediator = MartinScopedScope.ServiceProvider.GetRequiredService<global::Mediator.IMediator>();

#if !BENCHMARK_AOT
    ServiceCollection mediatRServices = new();
    // MediatR 14 constructs its mediator only after ILoggerFactory is registered.
    mediatRServices.AddLogging();
    mediatRServices.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<MediatRPingHandler>());
    MediatRMediator = mediatRServices.BuildServiceProvider().GetRequiredService<MediatR.IMediator>();
#endif
  }

  // Pairs with MartinothamarGenerated: transient mediator, Ping with no extra dependency.
#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<Pong> TimeWarpGeneratedMonomorphicSend()
  {
    return GeneratedMediator.Send(Request);
  }

  // Pairs with MartinothamarGenerated: transient ISender, same Ping, same handler body.
#if !BENCHMARK_AOT
  [Benchmark(Baseline = true)]
#endif
  public ValueTask<Pong> TimeWarpGeneratedInterfaceSend()
  {
    return GeneratedSender.Send(Request);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public Task<object?> TimeWarpGeneratedObjectSend()
  {
    return GeneratedSender.Send((object)Request).AsTask();
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask TimeWarpGeneratedInterfaceSendUnit()
  {
    return GeneratedSender.Send(NopRequest);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<Unit> TimeWarpGeneratedMonomorphicSendUnit()
  {
    return GeneratedMediator.Send(NopRequest);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<Pong> TimeWarpCallSiteInliningPrototype()
  {
    return TimeWarp.Mediator.Generated.Mediator.Dispatch_global__MediatorBenchmarks_Ping(
        GeneratedScope,
        Request,
        default);
  }

  // Pairs with MartinothamarScopedSend. Both handlers take BenchmarkPeers.ScopeToken.
#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<Pong> TimeWarpGeneratedScopedSend()
  {
    return ScopedSender.Send(ScopedRequest);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask TimeWarpGeneratedInterfacePublish()
  {
    return Publisher.Publish(PingedNotification);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask TimeWarpGeneratedScopedPublish()
  {
    return ScopedPublisher.Publish(ClientPingedNotification);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask TimeWarpGeneratedStatePipeline()
  {
    return ScopedSender.Send(IncrementRequest);
  }

#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<OneOf<ProfileResponse, ProblemDetails>> TimeWarpGeneratedArchitectureSend()
  {
    return GeneratedSender.Send(UpdateProfileRequest);
  }

  // Pairs with TimeWarpGeneratedInterfaceSend. Transient, not martinothamar's Singleton default.
#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<MartinPong> MartinothamarGenerated()
  {
    return MartinothamarMediator.Send(MartinRequest);
  }

  // Pairs with TimeWarpGeneratedScopedSend. Scoped mediator and the same ScopeToken dependency.
#if !BENCHMARK_AOT
  [Benchmark]
#endif
  public ValueTask<scopedmartin::MartinothamarScopedBenchmarks.MartinScopedPong> MartinothamarScopedSend()
  {
    return MartinScopedMediator.Send(MartinScopedRequest);
  }

#if !BENCHMARK_AOT
  // Reference for TimeWarpGeneratedInterfaceSend on the same Ping. MediatR 14.2.0 returns Task.
  [Benchmark]
  public Task<Pong> MediatRSend()
  {
    return MediatRMediator.Send(Request);
  }
#endif
}
