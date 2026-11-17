#region Purpose
// Marks the sample as a mediator host and weaves the trace behavior.
#endregion

using TimeWarp.Mediator.Examples.GeneratedRequests;

[assembly: MediatorAssembly]
[assembly: MediatorBehavior(typeof(TraceBehavior<,>))]
