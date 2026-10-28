#region Purpose
// Compiles the migration-page Nuru shape: using static Unit, and Unit.ValueTask in the expression.
#endregion

using static TimeWarp.Mediator.Unit;

public sealed class NuruStyleUnitHandler : IRequestHandler<NuruStylePing, Unit>
{
  // ValueTask in this signature binds to System.Threading.Tasks.ValueTask.
  // A bare ValueTask on the right-hand side is CS0229. Qualify the completed value.
  public ValueTask<Unit> Handle(NuruStylePing request, CancellationToken cancellationToken) =>
    Unit.ValueTask;
}
