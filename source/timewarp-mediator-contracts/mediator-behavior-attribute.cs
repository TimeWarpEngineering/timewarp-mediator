#region Purpose
// Explicit compile-time pipeline membership and order (replaces GetServices().Reverse().Aggregate).
#endregion

#region Design
// First attribute is outermost, matching MediatR registration order. Optional Order is a
// tie-breaker applied before source order. Open generic behaviors are closed per request
// when constraints match (e.g. where TRequest : IAction). Optional Scope named argument
// assigns the behavior to ISender<TScope>; omitted Scope is the unscoped default pipeline.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Declares a pipeline behavior that participates in the generated dispatch chain.
/// Attribute order (then <see cref="Order"/>) is the compile-time pipeline order:
/// the first behavior is outermost, matching MediatR <c>GetServices().Reverse().Aggregate</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class MediatorBehaviorAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute.
    /// </summary>
    /// <param name="behaviorType">Closed or open generic behavior type implementing <c>IPipelineBehavior&lt;,&gt;</c>.</param>
    /// <param name="order">Optional explicit order. Lower runs further out. Default 0 keeps source order.</param>
    public MediatorBehaviorAttribute(Type behaviorType, int order = 0)
    {
        BehaviorType = behaviorType ?? throw new ArgumentNullException(nameof(behaviorType));
        Order = order;
    }

    /// <summary>
    /// Behavior implementation type.
    /// </summary>
    public Type BehaviorType { get; }

    /// <summary>
    /// Sort key. Lower values wrap the rest of the chain (run first).
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Pipeline marker type this behavior belongs to. When null, the behavior runs only on the
    /// unscoped default pipeline. Scoped pipelines never run unscoped behaviors.
    /// </summary>
    public Type? Scope { get; set; }
}
