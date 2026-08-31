using TimeWarp.Mediator;
using TimeWarp.Mediator.Generators.Tests.State;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(OuterTrackingBehavior<,>), 0)]
[assembly: MediatorBehavior(typeof(InnerTrackingBehavior<,>), 1)]
[assembly: MediatorBehavior(typeof(StateTransactionBehavior<,>), 2)]
[assembly: MediatorBehavior(typeof(ShortCircuitBehavior<,>), 3)]
