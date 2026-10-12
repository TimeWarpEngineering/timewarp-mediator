namespace TimeWarp.Mediator.Internal;

/// <summary>
/// Shared <see cref="System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute"/> and
/// <see cref="System.Diagnostics.CodeAnalysis.RequiresDynamicCodeAttribute"/> messages for the reflection runtime.
/// </summary>
internal static class TrimmingMessages
{
    public const string UnreferencedCode =
        "The reflection mediator scans assemblies and closes generic handler, wrapper, and exception handler types at runtime, " +
        "which trimming can break. Use the source-generated AddGeneratedMediator() from TimeWarp.Mediator.Generators for trimmed apps.";

    public const string DynamicCode =
        "The reflection mediator calls MakeGenericType and Activator.CreateInstance at runtime, which is not compatible with " +
        "NativeAOT. Use the source-generated AddGeneratedMediator() from TimeWarp.Mediator.Generators for AOT apps.";
}
