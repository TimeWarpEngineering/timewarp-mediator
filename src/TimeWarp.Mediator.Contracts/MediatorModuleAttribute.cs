#region Purpose
// Names a handler/message module so the declaring assembly joins the compile-time graph.
#endregion

using System;

namespace TimeWarp.Mediator;

/// <summary>
/// Names a module for a handler or message. The declaring assembly is treated as a graph member.
/// Pipeline assignment for <c>ISender&lt;TScope&gt;</c> is <see cref="MediatorScopeAttribute"/>,
/// not this string name.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class MediatorModuleAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with a module name.
    /// </summary>
    /// <param name="name">Module name (for example <c>Orders</c> or a pipeline marker name).</param>
    public MediatorModuleAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Module name.
    /// </summary>
    public string Name { get; }
}
