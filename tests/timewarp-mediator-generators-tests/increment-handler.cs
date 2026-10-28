#region Purpose
// Compiles the migration-page ActionHandler example. Handle returns ValueTask.CompletedTask.
#endregion

#region Design
// ActionHandler<T>.Handle returns ValueTask. Unit.ValueTask is ValueTask<Unit>, so that return is CS0029.
#endregion

public sealed class IncrementHandler : ActionHandler<Increment.Action>
{
  public override ValueTask Handle(Increment.Action request, CancellationToken cancellationToken) =>
    ValueTask.CompletedTask;
}
