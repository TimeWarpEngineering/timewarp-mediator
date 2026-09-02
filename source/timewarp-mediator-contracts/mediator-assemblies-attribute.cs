#region Purpose
// Compile-time equivalent of MediatorOptions.Assemblies: include other assemblies by marker type.
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Includes the assemblies containing the given marker types in the compile-time mediator graph.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class MediatorAssembliesAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with marker types whose assemblies should be linked.
    /// </summary>
    /// <param name="markerTypes">Types used only to identify member assemblies.</param>
    public MediatorAssembliesAttribute(params Type[] markerTypes)
    {
        MarkerTypes = markerTypes ?? Array.Empty<Type>();
    }

    /// <summary>
    /// Marker types whose containing assemblies are members of the graph.
    /// </summary>
    public Type[] MarkerTypes { get; }
}
