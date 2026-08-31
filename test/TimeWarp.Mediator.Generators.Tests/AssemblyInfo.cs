using TimeWarp.Mediator;
using TimeWarp.Mediator.Generators.Tests.Pipelines;
using TimeWarp.Mediator.Generators.Tests.State;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(OuterTrackingBehavior<,>), 0)]
[assembly: MediatorBehavior(typeof(InnerTrackingBehavior<,>), 1)]
[assembly: MediatorBehavior(typeof(StateTransactionBehavior<,>), 2)]
[assembly: MediatorBehavior(typeof(ShortCircuitBehavior<,>), 3)]
[assembly: MediatorBehavior(typeof(UnitOnlyBehavior<>), 4)]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ServerStampBehavior<,>), Scope = typeof(ServerPipeline))]
