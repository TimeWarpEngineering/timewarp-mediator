#region Purpose
// Compiles an in-memory host assembly with MediatorGenerator, the way a generator-referencing project builds.
#endregion

#region Design
// The Generators package ships the analyzer and sets TimeWarpMediatorGenerator=true, so a host runs
// both. Diagnostics here are the generator's plus the analyzer's on the generated compilation.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

internal static class HostCompiler
{
    internal const string DefaultNamespace = "TimeWarp.Mediator.Generated";

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
        return CompileWithReferences(assemblyName, source, profile, GetBaseReferences().Concat(hostReferences));
    }

    // Exact reference set, for hosts that are missing Contracts or the MS.DI abstractions.
    internal static HostCompilation CompileWithReferences(
        string assemblyName,
        string source,
        string profile,
        IEnumerable<MetadataReference> references,
        string generatedNamespace = DefaultNamespace)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, ParseOptions) },
            references: references,
            options: CompilationOptions);

        GeneratorDriver driver = CreateDriver(profile, generatedNamespace: generatedNamespace);

        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Microsoft.CodeAnalysis.Compilation output,
            out ImmutableArray<Diagnostic> generatorDiagnostics);

        ImmutableArray<Diagnostic> analyzerDiagnostics = output
            .WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(new TimeWarp.Mediator.Analyzers.RequestHandlerAnalyzer()),
                new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, CreateOptionsProvider(profile, generatedNamespace)))
            .GetAnalyzerDiagnosticsAsync()
            .GetAwaiter()
            .GetResult();

        return new HostCompilation(
            output,
            generatorDiagnostics.AddRange(analyzerDiagnostics).AddRange(output.GetDiagnostics()));
    }

    internal static GeneratorDriver CreateDriver(
        string profile = "Host",
        bool trackSteps = false,
        string generatedNamespace = DefaultNamespace)
    {
        return CSharpGeneratorDriver.Create(
            generators: new[] { new MediatorGenerator().AsSourceGenerator() },
            additionalTexts: null,
            parseOptions: ParseOptions,
            optionsProvider: CreateOptionsProvider(profile, generatedNamespace),
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: trackSteps));
    }

    private static AnalyzerConfigOptionsProvider CreateOptionsProvider(string profile, string generatedNamespace)
    {
        return new BuildPropertyOptionsProvider(new Dictionary<string, string>
        {
            ["build_property.TimeWarpMediatorAssembly"] = "true",
            ["build_property.TimeWarpMediatorGenerator"] = "true",
            ["build_property.TimeWarpMediatorProfile"] = profile,
            ["build_property.TimeWarpMediatorNamespace"] = generatedNamespace,
        });
    }

    // A member assembly compiled without the generator, as a referenced library project would be.
    internal static MetadataReference CompileLibrary(string assemblyName, string source)
    {
        return MetadataReference.CreateFromImage(CompileLibraryImage(assemblyName, source));
    }

    // The emitted image, for runtime tests that load the library beside the host.
    internal static byte[] CompileLibraryImage(string assemblyName, string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, ParseOptions) },
            references: GetBaseReferences(),
            options: CompilationOptions);
        using MemoryStream stream = new();
        EmitResult result = compilation.Emit(stream);
        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return stream.ToArray();
    }

    internal static IEnumerable<MetadataReference> GetBaseReferences()
    {
        List<MetadataReference> references = GetSystemReferences();
        references.Add(MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location));
        return references;
    }

    internal static List<MetadataReference> GetSystemReferences()
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

    public ManifestDocument Manifest(string generatedNamespace = HostCompiler.DefaultNamespace)
    {
        IFieldSymbol json = GetType(generatedNamespace + ".MediatorManifest")
            .GetMembers("Json")
            .OfType<IFieldSymbol>()
            .Single();
        return ManifestDocument.Parse((string)json.ConstantValue!);
    }

    public INamedTypeSymbol GetType(string metadataName)
    {
        INamedTypeSymbol? type = Compilation.Assembly.GetTypeByMetadataName(metadataName);
        type.ShouldNotBeNull(metadataName + " was not generated");
        return type;
    }
}
