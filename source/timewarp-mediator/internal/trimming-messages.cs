namespace TimeWarp.Mediator.Internal;

/// <summary>
/// Shared <see cref="System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute"/> and
/// <see cref="System.Diagnostics.CodeAnalysis.RequiresDynamicCodeAttribute"/> messages for the reflection runtime.
/// </summary>
internal static class TrimmingMessages
{
    public const string UnreferencedCode =
        "The reflection mediator scans assemblies and closes generic handler, wrapper, and exception handler types at runtime, " +
        "which trimming can break. Reference TimeWarp.Mediator.Contracts and the TimeWarp.Mediator.Generators source generator, which emits AddGeneratedMediator() into the host, and call that instead.";

    public const string DynamicCode =
        "The reflection mediator calls MakeGenericType and Activator.CreateInstance at runtime, which is not compatible with " +
        "NativeAOT. Reference TimeWarp.Mediator.Contracts and the TimeWarp.Mediator.Generators source generator, which emits AddGeneratedMediator() into the host, and call that instead.";
}
