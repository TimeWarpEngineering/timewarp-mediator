#region Purpose
// Names a handler/message module for membership and future ISender TScope grouping (004-002).
#endregion

using System;

namespace TimeWarp.Mediator;

/// <summary>
/// Names a module for a handler or message. The declaring assembly is treated as a graph member.
/// Scoped senders (<c>ISender&lt;TScope&gt;</c>) consume this in 004-002.
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
