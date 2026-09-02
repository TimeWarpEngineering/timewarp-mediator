#region Purpose
// Marker-type pipeline membership for ISender<TScope> / IPublisher<TScope> named pipelines.
#endregion

#region Design
// TScope is a marker type, not a string. Applied to a handler, request, containing type, or
// assembly (assembly is the default for types that do not set their own). Type-level wins.
// [MediatorModule(string)] remains graph membership only and does not name a pipeline.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Assigns a handler, request, containing type, or assembly to a named pipeline.
/// The generated <c>ISender&lt;TScope&gt;</c> / <c>IPublisher&lt;TScope&gt;</c> dispatch tables
/// include only members of that scope. Types without this attribute belong to the unscoped
/// default pipeline (<see cref="ISender"/> / <see cref="IPublisher"/>).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class MediatorScopeAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with the pipeline marker type.
    /// </summary>
    /// <param name="scopeType">Marker type that names the pipeline (for example <c>typeof(ClientPipeline)</c>).</param>
    public MediatorScopeAttribute(Type scopeType)
    {
        ScopeType = scopeType ?? throw new ArgumentNullException(nameof(scopeType));
    }

    /// <summary>
    /// Pipeline marker type.
    /// </summary>
    public Type ScopeType { get; }
}
