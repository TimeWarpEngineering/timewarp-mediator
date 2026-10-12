#region Purpose
// Compiles snippets with RequestHandlerAnalyzer and the Contracts assembly.
#endregion

#region Design
// isHost sets build_property.TimeWarpMediatorAssembly=true, which the generator package sets for
// hosts; without it the snippet is an attribute-only library. generatorHost adds
// build_property.TimeWarpMediatorGenerator=true, also set by the generator package, under which
// the generator (not this analyzer) reports TWM001–TWM003.
#endregion

namespace TimeWarp.Mediator.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    internal static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source, bool isHost = false, bool generatorHost = false)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTest",
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        RequestHandlerAnalyzer analyzer = new();
        Dictionary<string, string> properties = new();
        if (isHost)
        {
            properties["build_property.TimeWarpMediatorAssembly"] = "true";
        }

        if (generatorHost)
        {
            properties["build_property.TimeWarpMediatorGenerator"] = "true";
        }

        AnalyzerOptions options = new(
            ImmutableArray<AdditionalText>.Empty,
            new BuildPropertyOptionsProvider(properties));
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
            options);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    // The source text a diagnostic is reported on, so tests pin the location, not only the id.
    internal static string LocatedText(Diagnostic diagnostic)
    {
        diagnostic.Location.IsInSource.ShouldBeTrue(diagnostic.ToString());
        return diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
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

    private sealed class BuildPropertyOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty = new DictionaryOptions(new Dictionary<string, string>());

        public BuildPropertyOptionsProvider(IReadOnlyDictionary<string, string> globalOptions)
        {
            GlobalOptions = new DictionaryOptions(globalOptions);
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;
    }

    private sealed class DictionaryOptions : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> Values;

        public DictionaryOptions(IReadOnlyDictionary<string, string> values)
        {
            Values = values;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            return Values.TryGetValue(key, out value);
        }
    }
}
