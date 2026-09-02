#region Purpose
// Handler contract for IAction. ValueTask is the native shape; IRequestHandler is implemented explicitly.
#endregion

using System.Threading;
using System.Threading.Tasks;

namespace TimeWarp.Mediator;

/// <summary>
/// Handles an <see cref="IAction"/> with a <see cref="ValueTask"/> contract.
/// </summary>
/// <typeparam name="TAction">Action type.</typeparam>
public interface IActionHandler<in TAction> : IRequestHandler<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Handles the action.
    /// </summary>
    /// <param name="request">The action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the handler work.</returns>
    new ValueTask Handle(TAction request, CancellationToken cancellationToken);
}

/// <summary>
/// Base type for nested ActionSet <c>Handler</c> classes.
/// </summary>
/// <typeparam name="TAction">Action type.</typeparam>
public abstract class ActionHandler<TAction> : IActionHandler<TAction>
    where TAction : IAction
{
    /// <inheritdoc />
    public abstract ValueTask Handle(TAction request, CancellationToken cancellationToken);

    Task IRequestHandler<TAction>.Handle(TAction request, CancellationToken cancellationToken)
    {
        return Handle(request, cancellationToken).AsTask();
    }
}
