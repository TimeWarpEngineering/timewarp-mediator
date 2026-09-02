[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ServerStampBehavior<,>), Scope = typeof(ServerPipeline))]
