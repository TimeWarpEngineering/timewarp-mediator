#region Purpose
// Incremental generator: build MessageGraph once per compilation and emit Mediator, scoped Sender/Publisher, and manifest.
#endregion

#region Design
// Generated code names TimeWarp.Mediator.IMediator and, under the Host profile, the MS.DI
// abstractions. When one cannot be resolved (for example a project that references only the
// Generators package) the generator emits nothing and reports TWM008 once, instead of a wall of
// CS0234/CS0246 inside .g.cs files.
#endregion

namespace TimeWarp.Mediator.Generators;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
    private const string ContractsHint = "Reference the TimeWarp.Mediator.Contracts package";
    private const string DependencyInjectionHint =
        "Reference Microsoft.Extensions.DependencyInjection.Abstractions, or set TimeWarpMediatorProfile to Aot";

    private static readonly (string MetadataName, string Hint)[] RequiredTypes =
    {
        ("TimeWarp.Mediator.IMediator", ContractsHint),
    };

    private static readonly (string MetadataName, string Hint)[] HostRequiredTypes =
    {
        ("Microsoft.Extensions.DependencyInjection.IServiceCollection", DependencyInjectionHint),
        ("Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions", DependencyInjectionHint),
        ("Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions", DependencyInjectionHint),
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<(Compilation Compilation, AnalyzerConfigOptionsProvider Options)> compilationAndOptions =
            context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider);

        context.RegisterSourceOutput(compilationAndOptions, static (SourceProductionContext productionContext, (Compilation Compilation, AnalyzerConfigOptionsProvider Options) input) =>
        {
            Membership membership = Membership.ForGenerator(input.Compilation, input.Options);
            Diagnostic? missing = FindMissingRuntime(input.Compilation, membership);
            if (missing is not null)
            {
                productionContext.ReportDiagnostic(missing);
                return;
            }

            MessageGraph graph = MessageGraphBuilder.Build(input.Compilation, membership);

            foreach (Diagnostic diagnostic in graph.Diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }

            foreach (Diagnostic diagnostic in graph.GeneratorDiagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }

            AotProfile aotProfile = AotProfile.Analyze(input.Compilation, graph);
            foreach (Diagnostic diagnostic in aotProfile.Diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }

            MediatorEmitter.Emit(productionContext, graph, aotProfile);
            ManifestEmitter.Emit(productionContext, graph);
        });
    }

    private static Diagnostic? FindMissingRuntime(Compilation compilation, Membership membership)
    {
        bool aot = string.Equals(membership.Profile, "Aot", StringComparison.OrdinalIgnoreCase)
            || string.Equals(membership.Profile, "Link", StringComparison.OrdinalIgnoreCase);
        IEnumerable<(string MetadataName, string Hint)> required = aot
            ? RequiredTypes
            : RequiredTypes.Concat(HostRequiredTypes);

        foreach ((string metadataName, string hint) in required)
        {
            if (compilation.GetTypeByMetadataName(metadataName) is null)
            {
                return Diagnostic.Create(DiagnosticDescriptors.MediatorRuntimeMissing, Location.None, metadataName, hint);
            }
        }

        return null;
    }
}
