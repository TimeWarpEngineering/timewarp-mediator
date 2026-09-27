#region Purpose
// Compiles an in-memory host assembly with MediatorGenerator, the way a generator-referencing project builds.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

internal static class HostCompiler
{
    internal static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    // The SDK suppresses CS1701/CS1702 (assembly unification) by default; match it.
    internal static readonly CSharpCompilationOptions CompilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable)
        .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
        {
            ["CS1701"] = ReportDiagnostic.Suppress,
            ["CS1702"] = ReportDiagnostic.Suppress,
        });

    internal static HostCompilation Compile(
        string assemblyName,
        string source,
        string profile = "Host",
        params MetadataReference[] hostReferences)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, ParseOptions) },
            references: GetBaseReferences().Concat(hostReferences),
            options: CompilationOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { new MediatorGenerator().AsSourceGenerator() },
            parseOptions: ParseOptions,
            optionsProvider: new BuildPropertyOptionsProvider(new Dictionary<string, string>
            {
                ["build_property.TimeWarpMediatorAssembly"] = "true",
                ["build_property.TimeWarpMediatorProfile"] = profile,
                ["build_property.TimeWarpMediatorNamespace"] = "TimeWarp.Mediator.Generated",
            }));

        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Microsoft.CodeAnalysis.Compilation output,
            out ImmutableArray<Diagnostic> generatorDiagnostics);

        return new HostCompilation(output, generatorDiagnostics.AddRange(output.GetDiagnostics()));
    }

    internal static IEnumerable<MetadataReference> GetBaseReferences()
    {
        List<MetadataReference> references = new();
        string? trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (trusted is not null)
        {
            foreach (string path in trusted.Split(Path.PathSeparator))
            {
                string fileName = Path.GetFileName(path);
                if (fileName.StartsWith("System.", StringComparison.Ordinal)
                    || fileName is "mscorlib.dll" or "netstandard.dll" or "Microsoft.Bcl.AsyncInterfaces.dll")
                {
                    references.Add(MetadataReference.CreateFromFile(path));
                }
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location));
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

internal sealed class HostCompilation
{
    public HostCompilation(Microsoft.CodeAnalysis.Compilation compilation, ImmutableArray<Diagnostic> diagnostics)
    {
        Compilation = compilation;
        Diagnostics = diagnostics;
    }

    public Microsoft.CodeAnalysis.Compilation Compilation { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public MetadataReference ToMetadataReference()
    {
        using MemoryStream stream = new();
        EmitResult result = Compilation.Emit(stream);
        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    public INamedTypeSymbol GetType(string metadataName)
    {
        INamedTypeSymbol? type = Compilation.Assembly.GetTypeByMetadataName(metadataName);
        type.ShouldNotBeNull(metadataName + " was not generated");
        return type;
    }
}
