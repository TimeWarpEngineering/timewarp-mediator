#region Purpose
// Aot/Link profile checks: TWM005 for behaviors the profile does not weave, TWM006 for handlers
// ServiceGen cannot construct with 'new Handler()'.
#endregion

#region Design
// Diagnose, not weave: behaviors usually take DI dependencies (loggers, validators, DbContext)
// and per-request state, so static singleton behaviors would be wrong more often than useful.
// The Aot manifest lists no behaviors, so it matches the emitted Dispatch_* bodies.
// TWM005 is one warning per registered behavior that closes for at least one request, placed on
// the [assembly: MediatorBehavior] attribute when it is in this compilation.
// TWM006 handlers get a 'default!' ServiceGen field so TWM006 is the only error, not CS7036
// or CS9035. A required member counts as unconstructible unless the zero-argument constructor
// is marked SetsRequiredMembers.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed class AotProfile
{
    private AotProfile(ImmutableArray<Diagnostic> diagnostics, HashSet<INamedTypeSymbol> unconstructibleHandlers)
    {
        Diagnostics = diagnostics;
        UnconstructibleHandlers = unconstructibleHandlers;
    }

    internal static AotProfile None { get; } =
        new(ImmutableArray<Diagnostic>.Empty, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));

    internal ImmutableArray<Diagnostic> Diagnostics { get; }

    internal HashSet<INamedTypeSymbol> UnconstructibleHandlers { get; }

    internal static AotProfile Analyze(Compilation compilation, MessageGraph graph)
    {
        if (!graph.IsAotProfile)
        {
            return None;
        }

        string profile = graph.Membership.Profile;
        List<Diagnostic> diagnostics = new();
        HashSet<INamedTypeSymbol> unconstructible = new(SymbolEqualityComparer.Default);
        HashSet<INamedTypeSymbol> reportedBehaviors = new(SymbolEqualityComparer.Default);

        foreach (RequestBinding request in graph.Requests)
        {
            foreach (INamedTypeSymbol behavior in request.ClosedBehaviors)
            {
                INamedTypeSymbol definition = behavior.OriginalDefinition;
                if (!reportedBehaviors.Add(definition))
                {
                    continue;
                }

                diagnostics.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.AotBehaviorNotWoven,
                        BehaviorLocation(compilation, definition),
                        definition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                        profile));
            }

            CheckHandler(compilation, request.HandlerType, profile, diagnostics, unconstructible);
        }

        foreach (NotificationBinding notification in graph.Notifications)
        {
            foreach (INamedTypeSymbol handler in notification.HandlerTypes)
            {
                CheckHandler(compilation, handler, profile, diagnostics, unconstructible);
            }
        }

        return new AotProfile(diagnostics.ToImmutableArray(), unconstructible);
    }

    private static void CheckHandler(
        Compilation compilation,
        INamedTypeSymbol handler,
        string profile,
        List<Diagnostic> diagnostics,
        HashSet<INamedTypeSymbol> unconstructible)
    {
        if (unconstructible.Contains(handler) || HasAccessibleParameterlessConstructor(compilation, handler))
        {
            return;
        }

        unconstructible.Add(handler);
        diagnostics.Add(
            Diagnostic.Create(
                DiagnosticDescriptors.AotHandlerNotConstructible,
                SourceLocation(compilation, handler.Locations),
                handler.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                profile));
    }

    private static bool HasAccessibleParameterlessConstructor(Compilation compilation, INamedTypeSymbol handler)
    {
        if (handler.IsAbstract)
        {
            return false;
        }

        bool hasRequiredMembers = HasRequiredMembers(handler);
        if (handler.IsValueType && !hasRequiredMembers)
        {
            return true;
        }

        foreach (IMethodSymbol constructor in handler.InstanceConstructors)
        {
            if (constructor.Parameters.All(p => p.IsOptional || p.IsParams)
                && compilation.IsSymbolAccessibleWithin(constructor, compilation.Assembly)
                && (!hasRequiredMembers || SetsRequiredMembers(constructor)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasRequiredMembers(INamedTypeSymbol handler)
    {
        for (INamedTypeSymbol? current = handler;
            current is not null && current.SpecialType != SpecialType.System_Object;
            current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is IFieldSymbol { IsRequired: true } or IPropertySymbol { IsRequired: true })
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool SetsRequiredMembers(IMethodSymbol constructor)
    {
        foreach (AttributeData attribute in constructor.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString()
                == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute")
            {
                return true;
            }
        }

        return false;
    }

    private static Location BehaviorLocation(Compilation compilation, INamedTypeSymbol definition)
    {
        foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != Membership.BehaviorAttributeMetadataName
                || attribute.ConstructorArguments.Length == 0
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol registered
                || !SymbolEqualityComparer.Default.Equals(registered.OriginalDefinition, definition))
            {
                continue;
            }

            Location? location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();
            if (location is not null)
            {
                return location;
            }
        }

        return SourceLocation(compilation, definition.Locations);
    }

    // A generator may only report locations in this compilation's syntax trees.
    private static Location SourceLocation(Compilation compilation, ImmutableArray<Location> locations)
    {
        foreach (Location location in locations)
        {
            if (location.IsInSource && compilation.ContainsSyntaxTree(location.SourceTree!))
            {
                return location;
            }
        }

        return Location.None;
    }
}
