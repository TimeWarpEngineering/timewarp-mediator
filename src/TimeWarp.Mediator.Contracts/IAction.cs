#region Purpose
// Marker for TimeWarp.State ActionSet nested Action types (void IRequest).
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// A void request used by TimeWarp.State ActionSets. Nested <c>Handler</c> types implement
/// <see cref="IActionHandler{TAction}"/>.
/// </summary>
public interface IAction : IRequest
{
}
