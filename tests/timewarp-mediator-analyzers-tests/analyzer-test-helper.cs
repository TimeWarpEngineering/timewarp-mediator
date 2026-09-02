#region Purpose
// Compiles snippets with RequestHandlerAnalyzer and the Contracts assembly.
#endregion

namespace TimeWarp.Mediator.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    internal static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTest",
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        RequestHandlerAnalyzer analyzer = new();
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static IReadOnlyList<MetadataReference> GetMetadataReferences()
    {
        List<MetadataReference> references = new();
        string? trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (trusted is not null)
        {
            foreach (string path in trusted.Split(Path.PathSeparator))
            {
                string fileName = Path.GetFileName(path);
                if (fileName.StartsWith("System.", StringComparison.Ordinal)
                    || fileName is "mscorlib.dll" or "netstandard.dll")
                {
                    references.Add(MetadataReference.CreateFromFile(path));
                }
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location));
        return references;
    }
}
