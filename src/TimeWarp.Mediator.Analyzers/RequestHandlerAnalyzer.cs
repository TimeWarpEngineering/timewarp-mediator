#region Purpose
// Analyzer-only TWM001–TWM004. Libraries reference this package without running the generator.
#endregion

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TimeWarp.Mediator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequestHandlerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.RequestHasNoHandler,
            DiagnosticDescriptors.DuplicateHandler,
            DiagnosticDescriptors.ScopeMismatch,
            DiagnosticDescriptors.WrongScopeSend);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            if (!Membership.TryCreate(
                    startContext.Compilation,
                    startContext.Options.AnalyzerConfigOptionsProvider,
                    currentCompilationIsHost: false,
                    out Membership membership))
            {
                return;
            }

            MessageGraph graph = MessageGraphBuilder.Build(startContext.Compilation, membership);
            INamedTypeSymbol? sender1 = startContext.Compilation.GetTypeByMetadataName("TimeWarp.Mediator.ISender`1");

            startContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (Diagnostic diagnostic in graph.Diagnostics)
                {
                    endContext.ReportDiagnostic(diagnostic);
                }
            });

            if (sender1 is null)
            {
                return;
            }

            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeSendInvocation(nodeContext, graph, sender1),
                SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeSendInvocation(
        SyntaxNodeAnalysisContext context,
        MessageGraph graph,
        INamedTypeSymbol sender1)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol method
            || method.Name != "Send")
        {
            return;
        }

        ITypeSymbol? receiverType = GetReceiverType(invocation, context.SemanticModel, context.CancellationToken);
        INamedTypeSymbol? senderScope = TryGetSenderScope(receiverType, sender1);
        if (senderScope is null)
        {
            return;
        }

        ITypeSymbol? argumentType = context.SemanticModel.GetTypeInfo(
            invocation.ArgumentList.Arguments[0].Expression,
            context.CancellationToken).Type;
        if (argumentType is not INamedTypeSymbol requestType)
        {
            return;
        }

        RequestBinding? binding = null;
        foreach (RequestBinding request in graph.Requests)
        {
            if (SymbolEqualityComparer.Default.Equals(request.RequestType, requestType))
            {
                binding = request;
                break;
            }
        }

        if (binding is null)
        {
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(binding.ScopeType, senderScope))
        {
            return;
        }

        string requestScopeName = binding.ScopeType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            ?? "unscoped";
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.WrongScopeSend,
                invocation.GetLocation(),
                requestType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                requestScopeName,
                senderScope.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
    }

    private static ITypeSymbol? GetReceiverType(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        System.Threading.CancellationToken cancellationToken)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;
        }

        return null;
    }

    private static INamedTypeSymbol? TryGetSenderScope(ITypeSymbol? type, INamedTypeSymbol sender1)
    {
        if (type is not INamedTypeSymbol named)
        {
            return null;
        }

        if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, sender1)
            && named.TypeArguments.Length == 1
            && named.TypeArguments[0] is INamedTypeSymbol direct)
        {
            return direct;
        }

        foreach (INamedTypeSymbol iface in named.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, sender1)
                && iface.TypeArguments.Length == 1
                && iface.TypeArguments[0] is INamedTypeSymbol argument)
            {
                return argument;
            }
        }

        return null;
    }
}
