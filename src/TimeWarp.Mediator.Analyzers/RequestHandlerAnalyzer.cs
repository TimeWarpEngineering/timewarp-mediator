#region Purpose
// Analyzer-only TWM001/TWM002. Libraries reference this package without running the generator.
#endregion

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TimeWarp.Mediator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequestHandlerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.RequestHasNoHandler,
            DiagnosticDescriptors.DuplicateHandler);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(compilationContext =>
        {
            if (!Membership.TryCreate(
                    compilationContext.Compilation,
                    compilationContext.Options.AnalyzerConfigOptionsProvider,
                    currentCompilationIsHost: false,
                    out Membership membership))
            {
                return;
            }

            MessageGraph graph = MessageGraphBuilder.Build(compilationContext.Compilation, membership);
            foreach (Diagnostic diagnostic in graph.Diagnostics)
            {
                compilationContext.ReportDiagnostic(diagnostic);
            }
        });
    }
}
