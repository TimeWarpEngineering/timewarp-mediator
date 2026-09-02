#region Purpose
// Opt-in linker membership so multi-project solutions do not cross-link handlers by accident.
#endregion

#region Design
// No marker → the assembly is not part of the MessageGraph. The generator host also opts in via
// MSBuild TimeWarpMediatorAssembly=true. Referenced assemblies are included only when they
// carry this attribute (or are listed by MediatorAssembliesAttribute).
#endregion

namespace TimeWarp.Mediator;

/// <summary>
/// Marks this assembly as a member of the compile-time mediator graph.
/// Types in assemblies without membership are not discovered as handlers, requests, or behaviors.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class MediatorAssemblyAttribute : Attribute
{
}
