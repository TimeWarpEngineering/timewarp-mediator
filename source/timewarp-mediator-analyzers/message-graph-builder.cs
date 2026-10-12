#region Purpose
// Handler-first discovery: walk member assemblies, bind request→handler, close behaviors, emit diagnostics.
#endregion

#region Design
// Closed concrete handlers only (no MaxTypesClosing combinatorics). Behaviors come exclusively from
// [assembly: MediatorBehavior] so pipelines do not pick up unrelated IPipelineBehavior types from
// referenced projects. Order matches MediatR GetServices().Reverse().Aggregate: first listed is outermost.
// Member assemblies are ordered by name before SourceIndex so tie-breaking is deterministic.
// Discovery walks member assemblies by name and sorts each handler list by display name, so
// notification handler order and the TWM002 handler list do not depend on hash-set order.
// A class binds every handler interface it implements; an interface already implemented by a
// concrete, discovered base handler is inherited, not a second binding (no TWM002, no double publish).
// TryCloseBehavior returns null when the constructed type is not IPipelineBehavior<TRequest, TResponse>.
// Pipeline membership is [MediatorScope(typeof(TScope))] on handler, request, containing type, or
// assembly. Unscoped (no attribute) is the default ISender/IPublisher pipeline. Behaviors with a
// Scope named argument weave only that pipeline; unscoped behaviors never run on scoped requests.
// Void dispatch comes from matching IRequestHandler<T>, never from TResponse == Unit: an explicit
// IRequestHandler<T, Unit> returns Task<Unit> and must be registered as the two-arity interface.
// Responses are ITypeSymbol so arrays and other non-named types (T[]) bind like named ones.
// A handler, message, response, behavior, or pipeline scope marker the host cannot access
// (internal in a referenced member without InternalsVisibleTo) is not bound and is TWM007,
// never CS0122 in generated code; its message is then not TWM001. Generated senders and
// publishers name that marker. Handler kinds the generated mediator ignores are TWM009, and
// [MediatorBehavior] entries that are not pipeline behaviors or close over no request are
// TWM010/TWM011. Those land in GeneratorDiagnostics so the analyzer-only package stays TWM001–TWM004.
// Diagnostic locations are limited to this compilation's syntax trees: analyzers and generators
// may not report into another project's source.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public static class MessageGraphBuilder
{
    private const string RequestHandler1 = "TimeWarp.Mediator.IRequestHandler`1";
    private const string RequestHandler2 = "TimeWarp.Mediator.IRequestHandler`2";
    private const string NotificationHandler1 = "TimeWarp.Mediator.INotificationHandler`1";
    private const string PipelineBehavior2 = "TimeWarp.Mediator.IPipelineBehavior`2";
    private const string Request1 = "TimeWarp.Mediator.IRequest`1";
    private const string Request0 = "TimeWarp.Mediator.IRequest";
    private const string Notification0 = "TimeWarp.Mediator.INotification";
    private const string UnitTypeName = "TimeWarp.Mediator.Unit";

    // Handler kinds the generated mediator does not dispatch (TWM009).
    private static readonly string[] UnsupportedHandlerKinds =
    {
        "TimeWarp.Mediator.IStreamRequestHandler`2",
        "TimeWarp.Mediator.IStreamPipelineBehavior`2",
        "TimeWarp.Mediator.Pipeline.IRequestPreProcessor`1",
        "TimeWarp.Mediator.Pipeline.IRequestPostProcessor`2",
        "TimeWarp.Mediator.Pipeline.IRequestExceptionHandler`3",
        "TimeWarp.Mediator.Pipeline.IRequestExceptionAction`2",
    };

    public static MessageGraph Build(Compilation compilation, Membership membership)
    {
        INamedTypeSymbol? requestHandler1 = compilation.GetTypeByMetadataName(RequestHandler1);
        INamedTypeSymbol? requestHandler2 = compilation.GetTypeByMetadataName(RequestHandler2);
        INamedTypeSymbol? notificationHandler1 = compilation.GetTypeByMetadataName(NotificationHandler1);
        INamedTypeSymbol? pipelineBehavior2 = compilation.GetTypeByMetadataName(PipelineBehavior2);
        INamedTypeSymbol? request0 = compilation.GetTypeByMetadataName(Request0);
        INamedTypeSymbol? request1 = compilation.GetTypeByMetadataName(Request1);
        INamedTypeSymbol? notification0 = compilation.GetTypeByMetadataName(Notification0);
        INamedTypeSymbol? unitType = compilation.GetTypeByMetadataName(UnitTypeName);
        ImmutableArray<INamedTypeSymbol> unsupportedKinds = UnsupportedHandlerKinds
            .Select(compilation.GetTypeByMetadataName)
            .Where(kind => kind is not null)
            .Select(kind => kind!)
            .ToImmutableArray();

        List<Diagnostic> diagnostics = new();
        List<Diagnostic> generatorDiagnostics = new();
        AccessGuard access = new(compilation, generatorDiagnostics);
        HashSet<INamedTypeSymbol> droppedMessages = new(SymbolEqualityComparer.Default);
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByRequest =
            new(SymbolEqualityComparer.Default);
        Dictionary<INamedTypeSymbol, ITypeSymbol> responseByRequest =
            new(SymbolEqualityComparer.Default);
        HashSet<INamedTypeSymbol> voidRequests = new(SymbolEqualityComparer.Default);
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByNotification =
            new(SymbolEqualityComparer.Default);
        HashSet<INamedTypeSymbol> requests = new(SymbolEqualityComparer.Default);

        foreach (IAssemblySymbol assembly in membership.MemberAssemblies.OrderBy(a => a.Name, System.StringComparer.Ordinal))
        {
            foreach (INamedTypeSymbol type in Membership.EnumerateTypes(assembly.GlobalNamespace))
            {
                if (!IsDiscoverableType(type))
                {
                    continue;
                }

                CollectRequest(compilation, type, request0, request1, requests);
                ReportUnsupportedKinds(compilation, type, unsupportedKinds, generatorDiagnostics);

                foreach (RequestHandlerMatch match in GetRequestHandlers(type, requestHandler1, requestHandler2, unitType, membership))
                {
                    if (!access.Allows(type) || !access.Allows(match.RequestType) || !access.Allows(match.ResponseType))
                    {
                        droppedMessages.Add(match.RequestType);
                        continue;
                    }

                    if (!handlersByRequest.TryGetValue(match.RequestType, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>();
                        handlersByRequest[match.RequestType] = list;
                    }

                    list.Add(type);
                    responseByRequest[match.RequestType] = match.ResponseType;
                    if (match.IsVoid)
                    {
                        voidRequests.Add(match.RequestType);
                    }

                    requests.Add(match.RequestType);
                }

                foreach (INamedTypeSymbol notificationType in GetNotificationHandlers(type, notificationHandler1, membership))
                {
                    if (!access.Allows(type) || !access.Allows(notificationType))
                    {
                        continue;
                    }

                    if (!handlersByNotification.TryGetValue(notificationType, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>();
                        handlersByNotification[notificationType] = list;
                    }

                    list.Add(type);
                }
            }
        }

        foreach (List<INamedTypeSymbol> list in handlersByRequest.Values)
        {
            SortByDisplayName(list);
        }

        foreach (List<INamedTypeSymbol> list in handlersByNotification.Values)
        {
            SortByDisplayName(list);
        }

        ImmutableArray<BehaviorRegistration> behaviors = DiscoverBehaviors(
            compilation,
            membership,
            pipelineBehavior2,
            access,
            generatorDiagnostics);
        HashSet<BehaviorRegistration> wovenBehaviors = new();

        foreach (INamedTypeSymbol requestType in requests)
        {
            if (!IsDiscoverableType(requestType) || requestType.Arity > 0)
            {
                continue;
            }

            if (!membership.Includes(requestType) || droppedMessages.Contains(requestType))
            {
                continue;
            }

            handlersByRequest.TryGetValue(requestType, out List<INamedTypeSymbol>? handlerList);
            int handlerCount = handlerList?.Count ?? 0;

            if (handlerCount == 0)
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.RequestHasNoHandler,
                        GetSourceLocation(compilation, requestType),
                        requestType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
            }
            else if (handlerCount > 1 && handlerList is not null)
            {
                string names = string.Join(
                    ", ",
                    handlerList.Select(h => h.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.DuplicateHandler,
                        GetSourceLocation(compilation, handlerList[1]),
                        requestType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        names));
            }
        }

        ImmutableArray<RequestBinding>.Builder requestBindings = ImmutableArray.CreateBuilder<RequestBinding>();
        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> pair in handlersByRequest.OrderBy(p => p.Key.ToDisplayString(), System.StringComparer.Ordinal))
        {
            if (pair.Value.Count != 1)
            {
                continue;
            }

            INamedTypeSymbol requestType = pair.Key;
            INamedTypeSymbol handlerType = pair.Value[0];
            ITypeSymbol responseType = responseByRequest[requestType];
            bool isUnit = voidRequests.Contains(requestType);

            INamedTypeSymbol? handlerScope = ResolveScope(handlerType);
            INamedTypeSymbol? requestScope = ResolveScope(requestType);
            ReportScopeMismatch(compilation, handlerType, handlerScope, requestType, requestScope, diagnostics);

            INamedTypeSymbol? scopeType = handlerScope ?? requestScope;
            if (!PipelineScopeAllowed(access, scopeType))
            {
                continue;
            }

            ImmutableArray<INamedTypeSymbol> closedBehaviors = CloseBehaviors(
                behaviors,
                requestType,
                responseType,
                pipelineBehavior2,
                scopeType,
                wovenBehaviors);

            requestBindings.Add(
                new RequestBinding(
                    requestType,
                    responseType,
                    handlerType,
                    isUnit,
                    closedBehaviors,
                    scopeType));
        }

        foreach (BehaviorRegistration registration in behaviors)
        {
            if (wovenBehaviors.Contains(registration))
            {
                continue;
            }

            generatorDiagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.BehaviorAppliesToNoRequest,
                    registration.Location,
                    registration.BehaviorType.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    registration.ScopeType?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "unscoped"));
        }

        ImmutableArray<NotificationBinding>.Builder notificationBindings = ImmutableArray.CreateBuilder<NotificationBinding>();
        List<NotificationGroup> groupedNotifications = new();
        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> pair in handlersByNotification)
        {
            INamedTypeSymbol? notificationScope = ResolveScope(pair.Key);
            foreach (INamedTypeSymbol handler in pair.Value)
            {
                INamedTypeSymbol? handlerScope = ResolveScope(handler);
                ReportScopeMismatch(compilation, handler, handlerScope, pair.Key, notificationScope, diagnostics);
                INamedTypeSymbol? scopeType = handlerScope ?? notificationScope;
                if (!PipelineScopeAllowed(access, scopeType))
                {
                    continue;
                }

                NotificationGroup? bucket = null;
                foreach (NotificationGroup candidate in groupedNotifications)
                {
                    if (SymbolEqualityComparer.Default.Equals(candidate.Notification, pair.Key)
                        && SymbolEqualityComparer.Default.Equals(candidate.Scope, scopeType))
                    {
                        bucket = candidate;
                        break;
                    }
                }

                if (bucket is null)
                {
                    bucket = new NotificationGroup(pair.Key, scopeType);
                    groupedNotifications.Add(bucket);
                }

                bucket.Handlers.Add(handler);
            }
        }

        foreach (NotificationGroup grouped in groupedNotifications
            .OrderBy(g => g.Notification.ToDisplayString(), System.StringComparer.Ordinal)
            .ThenBy(g => g.Scope?.ToDisplayString() ?? string.Empty, System.StringComparer.Ordinal))
        {
            notificationBindings.Add(
                new NotificationBinding(
                    grouped.Notification,
                    grouped.Handlers.ToImmutableArray(),
                    grouped.Scope));
        }

        return new MessageGraph(
            requestBindings.ToImmutable(),
            notificationBindings.ToImmutable(),
            diagnostics.ToImmutableArray(),
            generatorDiagnostics.ToImmutableArray(),
            membership);
    }

    private static void ReportScopeMismatch(
        Compilation compilation,
        INamedTypeSymbol handlerType,
        INamedTypeSymbol? handlerScope,
        INamedTypeSymbol messageType,
        INamedTypeSymbol? messageScope,
        List<Diagnostic> diagnostics)
    {
        if (handlerScope is null
            || messageScope is null
            || SymbolEqualityComparer.Default.Equals(handlerScope, messageScope))
        {
            return;
        }

        diagnostics.Add(
            Diagnostic.Create(
                DiagnosticDescriptors.ScopeMismatch,
                GetSourceLocation(compilation, handlerType),
                handlerType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                handlerScope.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                messageType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                messageScope.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
    }

    // The emitted pipeline names this marker. An inaccessible marker is TWM007 and is left unbound.
    private static bool PipelineScopeAllowed(AccessGuard access, INamedTypeSymbol? scopeType)
    {
        return scopeType is null || access.Allows(scopeType);
    }

    // One TWM009 per class and ignored interface definition (a class handling two stream requests is one warning).
    private static void ReportUnsupportedKinds(
        Compilation compilation,
        INamedTypeSymbol type,
        ImmutableArray<INamedTypeSymbol> unsupportedKinds,
        List<Diagnostic> generatorDiagnostics)
    {
        if (unsupportedKinds.IsEmpty || type.TypeKind != TypeKind.Class || type.IsAbstract)
        {
            return;
        }

        foreach (INamedTypeSymbol kind in unsupportedKinds)
        {
            if (!type.AllInterfaces.Any(iface => SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, kind)))
            {
                continue;
            }

            generatorDiagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedHandlerKind,
                    GetSourceLocation(compilation, type),
                    type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    kind.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        }
    }

    private static void CollectRequest(
        Compilation compilation,
        INamedTypeSymbol type,
        INamedTypeSymbol? request0,
        INamedTypeSymbol? request1,
        HashSet<INamedTypeSymbol> requests)
    {
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || type.IsAbstract || type.Arity > 0)
        {
            return;
        }

        // An inaccessible request cannot appear in generated code; its handler reports TWM007.
        if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return;
        }

        if (Implements(type, request0) || ImplementsGeneric(type, request1))
        {
            requests.Add(type);
        }
    }

    private static List<RequestHandlerMatch> GetRequestHandlers(
        INamedTypeSymbol type,
        INamedTypeSymbol? handler1,
        INamedTypeSymbol? handler2,
        INamedTypeSymbol? unitType,
        Membership membership)
    {
        List<RequestHandlerMatch> matches = new();
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
        {
            return matches;
        }

        HashSet<INamedTypeSymbol> bound = new(SymbolEqualityComparer.Default);
        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (handler2 is not null
                && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler2)
                && iface.TypeArguments.Length == 2
                && iface.TypeArguments[0] is INamedTypeSymbol request2
                && iface.TypeArguments[1] is ITypeSymbol response2
                && !IsBoundByBaseHandler(type, iface, membership)
                && bound.Add(request2))
            {
                matches.Add(new RequestHandlerMatch(request2, response2, false));
            }
        }

        // A two-arity binding wins over IRequestHandler<T> for the same request on one class.
        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (handler1 is not null
                && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
                && iface.TypeArguments.Length == 1
                && iface.TypeArguments[0] is INamedTypeSymbol request1
                && !IsBoundByBaseHandler(type, iface, membership)
                && bound.Add(request1))
            {
                matches.Add(new RequestHandlerMatch(request1, unitType ?? request1, true));
            }
        }

        return matches;
    }

    private static List<INamedTypeSymbol> GetNotificationHandlers(
        INamedTypeSymbol type,
        INamedTypeSymbol? handler1,
        Membership membership)
    {
        List<INamedTypeSymbol> notifications = new();
        if (handler1 is null || type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
        {
            return notifications;
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
                && iface.TypeArguments.Length == 1
                && iface.TypeArguments[0] is INamedTypeSymbol notification
                && !IsBoundByBaseHandler(type, iface, membership))
            {
                notifications.Add(notification);
            }
        }

        return notifications;
    }

    // True when a concrete base class that discovery already binds implements the same closed
    // interface: the derived class inherits that binding instead of introducing a second one.
    private static bool IsBoundByBaseHandler(INamedTypeSymbol type, INamedTypeSymbol iface, Membership membership)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.TypeKind == TypeKind.Class
                && !current.IsAbstract
                && current.Arity == 0
                && IsDiscoverableType(current)
                && membership.Includes(current)
                && current.AllInterfaces.Contains(iface, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }

    private static void SortByDisplayName(List<INamedTypeSymbol> handlers)
    {
        handlers.Sort((left, right) => System.StringComparer.Ordinal.Compare(left.ToDisplayString(), right.ToDisplayString()));
    }

    private static ImmutableArray<BehaviorRegistration> DiscoverBehaviors(
        Compilation compilation,
        Membership membership,
        INamedTypeSymbol? pipelineBehavior2,
        AccessGuard access,
        List<Diagnostic> generatorDiagnostics)
    {
        List<BehaviorRegistration> list = new();
        int sourceIndex = 0;

        IEnumerable<IAssemblySymbol> assemblies = membership.MemberAssemblies
            .OrderBy(a => a.Name, System.StringComparer.Ordinal);
        foreach (IAssemblySymbol assembly in assemblies)
        {
            foreach (AttributeData attribute in assembly.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != Membership.BehaviorAttributeMetadataName)
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 0
                    || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol behaviorType)
                {
                    continue;
                }

                int order = 0;
                if (attribute.ConstructorArguments.Length > 1
                    && attribute.ConstructorArguments[1].Value is int orderValue)
                {
                    order = orderValue;
                }

                INamedTypeSymbol? scopeType = null;
                foreach (KeyValuePair<string, TypedConstant> named in attribute.NamedArguments)
                {
                    if (named.Key == "Scope" && named.Value.Value is INamedTypeSymbol namedScope)
                    {
                        scopeType = namedScope;
                    }
                }

                Location location = GetAttributeLocation(compilation, attribute);
                INamedTypeSymbol definition = behaviorType.OriginalDefinition;
                if (pipelineBehavior2 is not null && !ImplementsGeneric(behaviorType, pipelineBehavior2)
                    && !ImplementsGeneric(definition, pipelineBehavior2))
                {
                    generatorDiagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.InvalidBehaviorRegistration,
                            location,
                            definition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                    continue;
                }

                if (!access.Allows(definition))
                {
                    continue;
                }

                list.Add(new BehaviorRegistration(behaviorType, order, sourceIndex, scopeType, location));
                sourceIndex++;
            }
        }

        return list
            .OrderBy(b => b.Order)
            .ThenBy(b => b.SourceIndex)
            .ToImmutableArray();
    }

    private static ImmutableArray<INamedTypeSymbol> CloseBehaviors(
        ImmutableArray<BehaviorRegistration> behaviors,
        INamedTypeSymbol requestType,
        ITypeSymbol responseType,
        INamedTypeSymbol? pipelineBehavior2,
        INamedTypeSymbol? requestScope,
        HashSet<BehaviorRegistration> wovenBehaviors)
    {
        ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (BehaviorRegistration registration in behaviors)
        {
            if (!SymbolEqualityComparer.Default.Equals(registration.ScopeType, requestScope))
            {
                continue;
            }

            INamedTypeSymbol? closed = TryCloseBehavior(registration.BehaviorType, requestType, responseType, pipelineBehavior2);
            if (closed is not null)
            {
                builder.Add(closed);
                wovenBehaviors.Add(registration);
            }
        }

        return builder.ToImmutable();
    }

    internal static INamedTypeSymbol? ResolveScope(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            INamedTypeSymbol? typeScope = GetScopeFromAttributes(current.GetAttributes());
            if (typeScope is not null)
            {
                return typeScope;
            }
        }

        return type.ContainingAssembly is null
            ? null
            : GetScopeFromAttributes(type.ContainingAssembly.GetAttributes());
    }

    private static INamedTypeSymbol? GetScopeFromAttributes(System.Collections.Immutable.ImmutableArray<AttributeData> attributes)
    {
        foreach (AttributeData attribute in attributes)
        {
            if (attribute.AttributeClass?.ToDisplayString() != Membership.ScopeAttributeMetadataName)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length > 0
                && attribute.ConstructorArguments[0].Value is INamedTypeSymbol scopeType)
            {
                return scopeType;
            }
        }

        return null;
    }

    private static INamedTypeSymbol? TryCloseBehavior(
        INamedTypeSymbol behaviorType,
        INamedTypeSymbol requestType,
        ITypeSymbol responseType,
        INamedTypeSymbol? pipelineBehavior2)
    {
        if (behaviorType.IsUnboundGenericType)
        {
            behaviorType = behaviorType.OriginalDefinition;
        }

        if (behaviorType.Arity == 0)
        {
            return ImplementsPipeline(behaviorType, requestType, responseType, pipelineBehavior2) ? behaviorType : null;
        }

        INamedTypeSymbol definition = behaviorType.OriginalDefinition;
        ITypeSymbol[] arguments;
        if (definition.Arity == 2)
        {
            arguments = new ITypeSymbol[] { requestType, responseType };
        }
        else if (definition.Arity == 1)
        {
            arguments = new ITypeSymbol[] { requestType };
        }
        else
        {
            return null;
        }

        if (!CanConstruct(definition, arguments))
        {
            return null;
        }

        INamedTypeSymbol constructed = definition.Construct(arguments);
        return ImplementsPipeline(constructed, requestType, responseType, pipelineBehavior2) ? constructed : null;
    }

    private static bool ImplementsPipeline(
        INamedTypeSymbol behaviorType,
        INamedTypeSymbol requestType,
        ITypeSymbol responseType,
        INamedTypeSymbol? pipelineBehavior2)
    {
        if (pipelineBehavior2 is null)
        {
            return true;
        }

        foreach (INamedTypeSymbol iface in behaviorType.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, pipelineBehavior2)
                && iface.TypeArguments.Length == 2
                && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], requestType)
                && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[1], responseType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanConstruct(INamedTypeSymbol openGeneric, ITypeSymbol[] typeArguments)
    {
        if (openGeneric.Arity != typeArguments.Length)
        {
            return false;
        }

        for (int i = 0; i < openGeneric.TypeParameters.Length; i++)
        {
            ITypeParameterSymbol parameter = openGeneric.TypeParameters[i];
            ITypeSymbol argument = typeArguments[i];

            if (parameter.HasReferenceTypeConstraint && !argument.IsReferenceType)
            {
                return false;
            }

            if (parameter.HasValueTypeConstraint && !argument.IsValueType)
            {
                return false;
            }

            if (parameter.HasUnmanagedTypeConstraint && argument is not { IsUnmanagedType: true })
            {
                return false;
            }

            foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
            {
                if (!ImplementsOrInherits(argument, constraint))
                {
                    return false;
                }
            }

            if (parameter.HasConstructorConstraint)
            {
                if (argument is not INamedTypeSymbol named || named.IsAbstract)
                {
                    return false;
                }

                bool hasPublicCtor = false;
                foreach (IMethodSymbol ctor in named.InstanceConstructors)
                {
                    if (ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public)
                    {
                        hasPublicCtor = true;
                        break;
                    }
                }

                if (!hasPublicCtor)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool ImplementsOrInherits(ITypeSymbol type, ITypeSymbol constraint)
    {
        if (SymbolEqualityComparer.Default.Equals(type, constraint))
        {
            return true;
        }

        for (INamedTypeSymbol? current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, constraint)
                || SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, constraint.OriginalDefinition))
            {
                return true;
            }
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, constraint)
                || SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, constraint.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol? interfaceType)
    {
        if (interfaceType is null)
        {
            return false;
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ImplementsGeneric(INamedTypeSymbol type, INamedTypeSymbol? unboundInterface)
    {
        if (unboundInterface is null)
        {
            return false;
        }

        INamedTypeSymbol definition = unboundInterface.OriginalDefinition;
        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, definition))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDiscoverableType(INamedTypeSymbol type)
    {
        if (type.IsStatic || type.TypeKind is TypeKind.Interface or TypeKind.Delegate or TypeKind.Enum)
        {
            return false;
        }

        return !IsEffectivelyPrivate(type);
    }

    private static bool IsEffectivelyPrivate(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility is Accessibility.Private
                or Accessibility.Protected
                or Accessibility.ProtectedAndInternal)
            {
                return true;
            }
        }

        return false;
    }

    private static Location GetSourceLocation(Compilation compilation, ISymbol symbol)
    {
        foreach (Location location in symbol.Locations)
        {
            if (location.IsInSource && compilation.ContainsSyntaxTree(location.SourceTree!))
            {
                return location;
            }
        }

        return Location.None;
    }

    private static Location GetAttributeLocation(Compilation compilation, AttributeData attribute)
    {
        SyntaxReference? reference = attribute.ApplicationSyntaxReference;
        return reference is not null && compilation.ContainsSyntaxTree(reference.SyntaxTree)
            ? reference.GetSyntax().GetLocation()
            : Location.None;
    }

    public static string Sanitize(INamedTypeSymbol type)
    {
        string display = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        StringBuilder builder = new(display.Length);
        foreach (char c in display)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }

    private readonly struct RequestHandlerMatch
    {
        public RequestHandlerMatch(INamedTypeSymbol requestType, ITypeSymbol responseType, bool isVoid)
        {
            RequestType = requestType;
            ResponseType = responseType;
            IsVoid = isVoid;
        }

        public INamedTypeSymbol RequestType { get; }

        public ITypeSymbol ResponseType { get; }

        public bool IsVoid { get; }
    }

    // Reports TWM007 once per inaccessible type and says whether a type may appear in generated code.
    private sealed class AccessGuard
    {
        private readonly Compilation Compilation;
        private readonly List<Diagnostic> Diagnostics;
        private readonly HashSet<ITypeSymbol> Reported = new(SymbolEqualityComparer.Default);

        public AccessGuard(Compilation compilation, List<Diagnostic> diagnostics)
        {
            Compilation = compilation;
            Diagnostics = diagnostics;
        }

        public bool Allows(ITypeSymbol type)
        {
            ITypeSymbol? inaccessible = FindInaccessible(type);
            if (inaccessible is null)
            {
                return true;
            }

            if (Reported.Add(inaccessible))
            {
                Diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InaccessibleMemberType,
                        GetSourceLocation(Compilation, inaccessible),
                        inaccessible.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        inaccessible.ContainingAssembly?.Name ?? string.Empty,
                        Compilation.AssemblyName ?? string.Empty));
            }

            return false;
        }

        private ITypeSymbol? FindInaccessible(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol array)
            {
                return FindInaccessible(array.ElementType);
            }

            if (type is INamedTypeSymbol named)
            {
                foreach (ITypeSymbol argument in named.TypeArguments)
                {
                    ITypeSymbol? inner = FindInaccessible(argument);
                    if (inner is not null)
                    {
                        return inner;
                    }
                }

                INamedTypeSymbol definition = named.OriginalDefinition;
                return Compilation.IsSymbolAccessibleWithin(definition, Compilation.Assembly) ? null : definition;
            }

            return type is ITypeParameterSymbol || Compilation.IsSymbolAccessibleWithin(type, Compilation.Assembly)
                ? null
                : type;
        }
    }

    private sealed class NotificationGroup
    {
        public NotificationGroup(INamedTypeSymbol notification, INamedTypeSymbol? scope)
        {
            Notification = notification;
            Scope = scope;
            Handlers = new List<INamedTypeSymbol>();
        }

        public INamedTypeSymbol Notification { get; }

        public INamedTypeSymbol? Scope { get; }

        public List<INamedTypeSymbol> Handlers { get; }
    }
}
