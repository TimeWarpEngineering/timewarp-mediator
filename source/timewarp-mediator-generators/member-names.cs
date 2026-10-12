#region Purpose
// Collision-safe sanitized identifiers for Dispatch_*, Publish_*, ServiceGen fields, and scope suffixes.
#endregion

#region Design
// Sanitize maps every non-alphanumeric to '_', so A.B_C and A_B.C share a name. Names are assigned
// once per graph: a type whose sanitized name is unique keeps it unchanged (benchmarks and callers
// target Dispatch_global__Ns_Type directly). Every type in a colliding group gets a suffix of the
// FNV-1a hash of its fully qualified name, so the result depends only on the type, not on graph
// order. A further clash (vanishingly rare) appends an index in ordinal display-name order.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed class MemberNames
{
    private readonly Dictionary<INamedTypeSymbol, string> Names = new(SymbolEqualityComparer.Default);

    public MemberNames(MessageGraph graph)
    {
        List<INamedTypeSymbol> types = new();
        HashSet<INamedTypeSymbol> seen = new(SymbolEqualityComparer.Default);
        foreach (RequestBinding request in graph.Requests)
        {
            Add(types, seen, request.RequestType);
            Add(types, seen, request.HandlerType);
            Add(types, seen, request.ScopeType);
        }

        foreach (NotificationBinding notification in graph.Notifications)
        {
            Add(types, seen, notification.NotificationType);
            Add(types, seen, notification.ScopeType);
            foreach (INamedTypeSymbol handler in notification.HandlerTypes)
            {
                Add(types, seen, handler);
            }
        }

        types.Sort((left, right) => string.CompareOrdinal(Fq(left), Fq(right)));

        Dictionary<string, int> counts = new(StringComparer.Ordinal);
        foreach (INamedTypeSymbol type in types)
        {
            string sanitized = MessageGraphBuilder.Sanitize(type);
            counts[sanitized] = counts.TryGetValue(sanitized, out int count) ? count + 1 : 1;
        }

        HashSet<string> taken = new(StringComparer.Ordinal);
        List<INamedTypeSymbol> colliding = new();
        foreach (INamedTypeSymbol type in types)
        {
            string sanitized = MessageGraphBuilder.Sanitize(type);
            if (counts[sanitized] == 1)
            {
                Names[type] = sanitized;
                taken.Add(sanitized);
            }
            else
            {
                colliding.Add(type);
            }
        }

        foreach (INamedTypeSymbol type in colliding)
        {
            string candidate = MessageGraphBuilder.Sanitize(type) + "_" + StableHash(Fq(type));
            string name = candidate;
            int index = 1;
            while (!taken.Add(name))
            {
                name = candidate + "_" + index;
                index++;
            }

            Names[type] = name;
        }
    }

    public string For(INamedTypeSymbol type)
    {
        return Names.TryGetValue(type, out string? name) ? name : MessageGraphBuilder.Sanitize(type);
    }

    private static void Add(List<INamedTypeSymbol> types, HashSet<INamedTypeSymbol> seen, INamedTypeSymbol? type)
    {
        if (type is not null && seen.Add(type))
        {
            types.Add(type);
        }
    }

    private static string Fq(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string StableHash(string value)
    {
        uint hash = 2166136261;
        foreach (char c in value)
        {
            hash ^= c;
            hash *= 16777619;
        }

        return hash.ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
    }
}
