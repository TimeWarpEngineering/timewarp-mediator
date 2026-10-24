#region Purpose
// Base type for TimeWarp.State ActionSet nested Handler classes.
#endregion

#region Design
// Implements IRequestHandler<TAction> directly and returns ValueTask. There is no Task shim
// and no IRequestHandler: message kind lives on IAction.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Base type for nested ActionSet <c>Handler</c> classes.
/// </summary>
/// <typeparam name="TAction">Action type.</typeparam>
public abstract class ActionHandler<TAction> : IRequestHandler<TAction>
    where TAction : IAction
{
  /// <summary>
  /// Handles the action.
  /// </summary>
  /// <param name="request">The action.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A <see cref="ValueTask"/> representing the handler work.</returns>
  public abstract ValueTask Handle(TAction request, CancellationToken cancellationToken);
}
