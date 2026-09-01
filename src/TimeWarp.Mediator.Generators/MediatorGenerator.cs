#region Purpose
// Incremental generator: build MessageGraph once per compilation and emit Mediator, scoped Sender/Publisher, and manifest.
#endregion

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TimeWarp.Mediator.Analyzers;

namespace TimeWarp.Mediator.Generators;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<(Compilation Compilation, AnalyzerConfigOptionsProvider Options)> compilationAndOptions =
            context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider);

        context.RegisterSourceOutput(compilationAndOptions, static (SourceProductionContext productionContext, (Compilation Compilation, AnalyzerConfigOptionsProvider Options) input) =>
        {
            Membership membership = Membership.ForGenerator(input.Compilation, input.Options);
            MessageGraph graph = MessageGraphBuilder.Build(input.Compilation, membership);

            foreach (Diagnostic diagnostic in graph.Diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }

            MediatorEmitter.Emit(productionContext, graph);
            ManifestEmitter.Emit(productionContext, graph);
        });
    }
}
