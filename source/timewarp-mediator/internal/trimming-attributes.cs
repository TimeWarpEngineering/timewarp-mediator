// Polyfills for the trimming/AOT annotations on target frameworks that predate them.
// The trimmer and the NativeAOT compiler match these attributes by full name, so the internal
// copies carry the same meaning as the BCL types they stand in for.

namespace System.Diagnostics.CodeAnalysis;

#if !NET5_0_OR_GREATER
/// <summary>
/// Indicates that the attributed member requires code that may be removed by trimming.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    public RequiresUnreferencedCodeAttribute(string message) => Message = message;

    public string Message { get; }

    public string? Url { get; set; }
}
#endif

#if !NET7_0_OR_GREATER
/// <summary>
/// Indicates that the attributed member requires dynamic code generation and is not compatible with ahead-of-time compilation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    public RequiresDynamicCodeAttribute(string message) => Message = message;

    public string Message { get; }

    public string? Url { get; set; }
}
#endif
