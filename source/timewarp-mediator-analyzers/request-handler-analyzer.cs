#region Purpose
// Analyzer-only TWM001–TWM004. Libraries reference this package without running the generator.
#endregion

#region Design
// TWM004 works on IInvocationOperation, so 'sender?.Send(x)' and implicit conversions are seen the
// same as 'sender.Send(x)'. The target must be ISender.Send (directly or through the receiver's
// implementation), not any method named Send. Send(object) is the documented escape hatch and is
// never reported.
// A concrete request type is checked everywhere: its pipeline comes from its own attributes.
// Two checks need the whole program and run only in a generator host (Membership.IsHost):
// an unscoped ISender/IMediator receiver, which is the generated Mediator there, and an argument
// typed as an interface or abstract class (IRequest<T>), reported when every bound request it
// could be belongs to another pipeline. A library sees only part of the graph, so it skips both.
// Membership and the graph are built lazily, on the first ISender.Send call or at compilation end,
// never in CompilationStart: a compilation with no sends and no end analysis (IDE without full
// solution analysis) never pays for them. Both honor the analysis cancellation token.
// In a generator host (TimeWarpMediatorGenerator=true) the generator reports TWM001–TWM003, so
// this analyzer keeps only TWM004 there.
#endregion

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
            Compilation compilation = startContext.Compilation;
            MembershipOptions options = MembershipOptions.Read(startContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions);
            CancellationToken cancellationToken = startContext.CancellationToken;
            Lazy<MessageGraph?> graph = new(
                () => Membership.TryCreate(compilation, options, options.IsGeneratorHost, cancellationToken, out Membership membership)
                    ? MessageGraphBuilder.Build(compilation, membership, cancellationToken)
                    : null,
                LazyThreadSafetyMode.ExecutionAndPublication);

            // The generator reports TWM001–TWM003 in its own host; reporting them here too doubles them.
            if (!options.IsGeneratorHost)
            {
                startContext.RegisterCompilationEndAction(endContext =>
                {
                    foreach (Diagnostic diagnostic in graph.Value?.Diagnostics ?? ImmutableArray<Diagnostic>.Empty)
                    {
                        endContext.ReportDiagnostic(diagnostic);
                    }
                });
            }

            INamedTypeSymbol? sender0 = compilation.GetTypeByMetadataName("TimeWarp.Mediator.ISender");
            INamedTypeSymbol? sender1 = compilation.GetTypeByMetadataName("TimeWarp.Mediator.ISender`1");
            if (sender0 is null || sender1 is null)
            {
                return;
            }

            SenderTypes senders = new(sender0, sender1);
            startContext.RegisterOperationAction(
                operationContext => AnalyzeSendInvocation(operationContext, graph, senders),
                OperationKind.Invocation);
        });
    }

    private static void AnalyzeSendInvocation(
        OperationAnalysisContext context,
        Lazy<MessageGraph?> lazyGraph,
        SenderTypes senders)
    {
        if (context.Operation is not IInvocationOperation invocation
            || invocation.TargetMethod.Name != "Send"
            || invocation.TargetMethod.Parameters.Length == 0
            || invocation.TargetMethod.Parameters[0].Type.SpecialType == SpecialType.System_Object
            || invocation.Instance?.Type is not INamedTypeSymbol receiverType
            || !IsSenderSend(invocation.TargetMethod, receiverType, senders.Sender0)
            || lazyGraph.Value is not MessageGraph graph)
        {
            return;
        }

        INamedTypeSymbol? senderScope = TryGetSenderScope(receiverType, senders.Sender1);
        if (senderScope is null && !graph.Membership.IsHost)
        {
            return;
        }

        ITypeSymbol? argumentType = GetRequestArgumentType(invocation);
        if (argumentType is not INamedTypeSymbol requestType)
        {
            return;
        }

        string? requestScopeName = IsConcrete(requestType)
            ? GetConcreteMismatch(graph, requestType, senderScope)
            : graph.Membership.IsHost ? GetAbstractMismatch(graph, requestType, senderScope) : null;
        if (requestScopeName is null)
        {
            return;
        }

        string senderName = senderScope is null
            ? "ISender (unscoped)"
            : "ISender<" + senderScope.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ">";
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.WrongScopeSend,
                invocation.Syntax.GetLocation(),
                requestType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                requestScopeName,
                senderName));
    }

    // Pipeline name of the bound request when it is not the sender's pipeline; null when it matches or is unbound.
    private static string? GetConcreteMismatch(MessageGraph graph, INamedTypeSymbol requestType, INamedTypeSymbol? senderScope)
    {
        foreach (RequestBinding request in graph.Requests)
        {
            if (SymbolEqualityComparer.Default.Equals(request.RequestType, requestType))
            {
                return SymbolEqualityComparer.Default.Equals(request.ScopeType, senderScope)
                    ? null
                    : ScopeName(request.ScopeType);
            }
        }

        return null;
    }

    // For an IRequest<T>-typed argument: mismatch only when at least one bound request fits and none is in this pipeline.
    private static string? GetAbstractMismatch(MessageGraph graph, INamedTypeSymbol staticType, INamedTypeSymbol? senderScope)
    {
        SortedSet<string> scopes = new(StringComparer.Ordinal);
        foreach (RequestBinding request in graph.Requests)
        {
            if (!IsAssignableTo(request.RequestType, staticType))
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(request.ScopeType, senderScope))
            {
                return null;
            }

            scopes.Add(ScopeName(request.ScopeType));
        }

        return scopes.Count == 0 ? null : string.Join(", ", scopes);
    }

    private static string ScopeName(INamedTypeSymbol? scope) =>
        scope?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "unscoped";

    private static bool IsConcrete(INamedTypeSymbol type) =>
        type.TypeKind is TypeKind.Class or TypeKind.Struct && !type.IsAbstract;

    private static bool IsAssignableTo(INamedTypeSymbol type, INamedTypeSymbol target)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, target))
            {
                return true;
            }
        }

        return type.AllInterfaces.Contains(target, SymbolEqualityComparer.Default);
    }

    // Static type of the request expression before the implicit conversion to the parameter type.
    private static ITypeSymbol? GetRequestArgumentType(IInvocationOperation invocation)
    {
        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            if (argument.Parameter?.Ordinal != 0)
            {
                continue;
            }

            IOperation value = argument.Value;
            while (value is IConversionOperation { IsImplicit: true } conversion)
            {
                value = conversion.Operand;
            }

            return value.Type;
        }

        return null;
    }

    // True when the call binds to ISender.Send, either on the interface or as the receiver's implementation of it.
    private static bool IsSenderSend(IMethodSymbol method, INamedTypeSymbol receiverType, INamedTypeSymbol sender0)
    {
        IMethodSymbol definition = method.OriginalDefinition;
        foreach (ISymbol member in sender0.GetMembers("Send"))
        {
            if (SymbolEqualityComparer.Default.Equals(definition, member))
            {
                return true;
            }

            ISymbol? implementation = receiverType.FindImplementationForInterfaceMember(member);
            if (implementation is not null
                && SymbolEqualityComparer.Default.Equals(definition, implementation.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static INamedTypeSymbol? TryGetSenderScope(INamedTypeSymbol type, INamedTypeSymbol sender1)
    {
        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, sender1)
            && type.TypeArguments.Length == 1
            && type.TypeArguments[0] is INamedTypeSymbol direct)
        {
            return direct;
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
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

    private sealed class SenderTypes
    {
        public SenderTypes(INamedTypeSymbol sender0, INamedTypeSymbol sender1)
        {
            Sender0 = sender0;
            Sender1 = sender1;
        }

        public INamedTypeSymbol Sender0 { get; }

        public INamedTypeSymbol Sender1 { get; }
    }
}
