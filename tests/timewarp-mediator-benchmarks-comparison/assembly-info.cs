using TimeWarp.Mediator;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(RenderBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ExceptionBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(StateTransactionBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ArchitectureValidationBehavior))]