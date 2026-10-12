namespace TimeWarp.Mediator.Tests;

public class TrimmingAnnotationTests
{
    // Matched by full name: the trimmer and NativeAOT compiler do the same.
    private const string RequiresUnreferencedCode = "System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute";
    private const string RequiresDynamicCode = "System.Diagnostics.CodeAnalysis.RequiresDynamicCodeAttribute";

    public static IEnumerable<object[]> ReflectionEntryPoints()
    {
        foreach (MethodInfo method in typeof(ServiceCollectionExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                     .Where(static method => method.Name == nameof(ServiceCollectionExtensions.AddMediator)))
        {
            yield return new object[] { method };
        }

        foreach (Type type in new[] { typeof(Mediator), typeof(RequestExceptionProcessorBehavior<,>), typeof(RequestExceptionActionProcessorBehavior<,>) })
        {
            foreach (ConstructorInfo constructor in type.GetConstructors())
            {
                yield return new object[] { constructor };
            }
        }
    }

    [Theory]
    [MemberData(nameof(ReflectionEntryPoints))]
    public void Should_annotate_reflection_entry_points_as_not_trim_or_aot_safe(MethodBase entryPoint)
    {
        List<string> attributeNames = entryPoint.GetCustomAttributesData()
            .Select(static attribute => attribute.AttributeType.FullName!)
            .ToList();

        attributeNames.ShouldContain(RequiresUnreferencedCode);
        attributeNames.ShouldContain(RequiresDynamicCode);
    }
}
