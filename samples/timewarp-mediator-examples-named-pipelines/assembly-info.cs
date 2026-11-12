#region Purpose
// Marks the sample as a mediator host and weaves the client and server pipeline behaviors.
#endregion

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(ClientStampBehavior<,>), Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ServerStampBehavior<,>), Scope = typeof(ServerPipeline))]