#region Purpose
// Handler-first discovery: walk member assemblies, bind request→handler, close behaviors, emit diagnostics.
#endregion

#region Design
// Closed concrete handlers only (no MaxTypesClosing combinatorics). Behaviors come exclusively from
// [assembly: MediatorBehavior] so pipelines do not pick up unrelated IPipelineBehavior types from
// referenced projects. Order matches MediatR GetServices().Reverse().Aggregate: first listed is outermost.
#endregion

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

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

        List<Diagnostic> diagnostics = new();
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByRequest =
            new(SymbolEqualityComparer.Default);
        Dictionary<INamedTypeSymbol, INamedTypeSymbol> responseByRequest =
            new(SymbolEqualityComparer.Default);
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByNotification =
            new(SymbolEqualityComparer.Default);
        HashSet<INamedTypeSymbol> requests = new(SymbolEqualityComparer.Default);

        foreach (IAssemblySymbol assembly in membership.MemberAssemblies)
        {
            foreach (INamedTypeSymbol type in Membership.EnumerateTypes(assembly.GlobalNamespace))
            {
                if (!IsDiscoverableType(type))
                {
                    continue;
                }

                CollectRequest(type, request0, request1, requests);

                if (TryGetRequestHandler(type, requestHandler1, requestHandler2, unitType, out INamedTypeSymbol requestType, out INamedTypeSymbol responseType))
                {
                    if (!handlersByRequest.TryGetValue(requestType, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>();
                        handlersByRequest[requestType] = list;
                    }

                    list.Add(type);
                    responseByRequest[requestType] = responseType;
                    requests.Add(requestType);
                }

                if (TryGetNotificationHandler(type, notificationHandler1, out INamedTypeSymbol notificationType))
                {
                    if (!handlersByNotification.TryGetValue(notificationType, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>();
                        handlersByNotification[notificationType] = list;
                    }

                    list.Add(type);
                }
            }
        }

        ImmutableArray<BehaviorRegistration> behaviors = DiscoverBehaviors(membership, pipelineBehavior2);

        foreach (INamedTypeSymbol requestType in requests)
        {
            if (!IsDiscoverableType(requestType) || requestType.Arity > 0)
            {
                continue;
            }

            if (!membership.Includes(requestType))
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
                        GetSourceLocation(requestType),
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
                        GetSourceLocation(handlerList[1]),
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
            INamedTypeSymbol responseType = responseByRequest[requestType];
            bool isUnit = unitType is not null
                && SymbolEqualityComparer.Default.Equals(responseType, unitType);

            ImmutableArray<INamedTypeSymbol> closedBehaviors = CloseBehaviors(
                behaviors,
                requestType,
                responseType,
                pipelineBehavior2);

            requestBindings.Add(
                new RequestBinding(
                    requestType,
                    responseType,
                    handlerType,
                    isUnit,
                    closedBehaviors));
        }

        ImmutableArray<NotificationBinding>.Builder notificationBindings = ImmutableArray.CreateBuilder<NotificationBinding>();
        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> pair in handlersByNotification.OrderBy(p => p.Key.ToDisplayString(), System.StringComparer.Ordinal))
        {
            notificationBindings.Add(
                new NotificationBinding(
                    pair.Key,
                    pair.Value.ToImmutableArray()));
        }

        return new MessageGraph(
            requestBindings.ToImmutable(),
            notificationBindings.ToImmutable(),
            diagnostics.ToImmutableArray(),
            membership);
    }

    private static void CollectRequest(
        INamedTypeSymbol type,
        INamedTypeSymbol? request0,
        INamedTypeSymbol? request1,
        HashSet<INamedTypeSymbol> requests)
    {
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || type.IsAbstract || type.Arity > 0)
        {
            return;
        }

        if (Implements(type, request0) || ImplementsGeneric(type, request1))
        {
            requests.Add(type);
        }
    }

    private static bool TryGetRequestHandler(
        INamedTypeSymbol type,
        INamedTypeSymbol? handler1,
        INamedTypeSymbol? handler2,
        INamedTypeSymbol? unitType,
        out INamedTypeSymbol requestType,
        out INamedTypeSymbol responseType)
    {
        requestType = null!;
        responseType = null!;

        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
        {
            return false;
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (handler2 is not null
                && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler2)
                && iface.TypeArguments.Length == 2
                && iface.TypeArguments[0] is INamedTypeSymbol request2
                && iface.TypeArguments[1] is INamedTypeSymbol response2)
            {
                requestType = request2;
                responseType = response2;
                return true;
            }
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (handler1 is not null
                && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
                && iface.TypeArguments.Length == 1
                && iface.TypeArguments[0] is INamedTypeSymbol request1)
            {
                requestType = request1;
                responseType = unitType ?? request1;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetNotificationHandler(
        INamedTypeSymbol type,
        INamedTypeSymbol? handler1,
        out INamedTypeSymbol notificationType)
    {
        notificationType = null!;
        if (handler1 is null || type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
        {
            return false;
        }

        foreach (INamedTypeSymbol iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
                && iface.TypeArguments.Length == 1
                && iface.TypeArguments[0] is INamedTypeSymbol notification)
            {
                notificationType = notification;
                return true;
            }
        }

        return false;
    }

    private static ImmutableArray<BehaviorRegistration> DiscoverBehaviors(
        Membership membership,
        INamedTypeSymbol? pipelineBehavior2)
    {
        List<BehaviorRegistration> list = new();
        int sourceIndex = 0;

        foreach (IAssemblySymbol assembly in membership.MemberAssemblies)
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

                if (pipelineBehavior2 is not null && !ImplementsGeneric(behaviorType, pipelineBehavior2)
                    && !ImplementsGeneric(behaviorType.OriginalDefinition, pipelineBehavior2))
                {
                    continue;
                }

                list.Add(new BehaviorRegistration(behaviorType, order, sourceIndex));
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
        INamedTypeSymbol responseType,
        INamedTypeSymbol? pipelineBehavior2)
    {
        ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (BehaviorRegistration registration in behaviors)
        {
            INamedTypeSymbol? closed = TryCloseBehavior(registration.BehaviorType, requestType, responseType, pipelineBehavior2);
            if (closed is not null)
            {
                builder.Add(closed);
            }
        }

        return builder.ToImmutable();
    }

    private static INamedTypeSymbol? TryCloseBehavior(
        INamedTypeSymbol behaviorType,
        INamedTypeSymbol requestType,
        INamedTypeSymbol responseType,
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
        return ImplementsPipeline(constructed, requestType, responseType, pipelineBehavior2) ? constructed : constructed;
    }

    private static bool ImplementsPipeline(
        INamedTypeSymbol behaviorType,
        INamedTypeSymbol requestType,
        INamedTypeSymbol responseType,
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

    private static Location GetSourceLocation(ISymbol symbol)
    {
        foreach (Location location in symbol.Locations)
        {
            if (location.IsInSource)
            {
                return location;
            }
        }

        return Location.None;
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
}
