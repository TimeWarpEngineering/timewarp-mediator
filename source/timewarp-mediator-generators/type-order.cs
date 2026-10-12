#region Purpose
// Orders message types most-derived first so emitted type tests never subsume a later one.
#endregion

#region Design
// A `case Base x:` before `case Derived y:` is CS8120, and an `if (o is Base)` chain would run the
// base branch first. Kahn's algorithm over "derived must precede its bases" edges; among ready types
// the ordinal display name wins, so output is deterministic. Assignability is the base-class chain
// plus AllInterfaces (exact symbol match). Generic variance (IEvent<out T>) is not modelled.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static class TypeOrder
{
    internal static List<INamedTypeSymbol> MostDerivedFirst(IEnumerable<INamedTypeSymbol> source)
    {
        List<INamedTypeSymbol> types = new();
        HashSet<INamedTypeSymbol> seen = new(SymbolEqualityComparer.Default);
        foreach (INamedTypeSymbol type in source)
        {
            if (seen.Add(type))
            {
                types.Add(type);
            }
        }

        types.Sort((left, right) => string.CompareOrdinal(Fq(left), Fq(right)));

        // pending[i] = how many other types in the set derive from types[i] and are not yet emitted.
        int[] pending = new int[types.Count];
        for (int i = 0; i < types.Count; i++)
        {
            for (int j = 0; j < types.Count; j++)
            {
                if (i != j && DerivesFrom(types[j], types[i]))
                {
                    pending[i]++;
                }
            }
        }

        List<INamedTypeSymbol> ordered = new(types.Count);
        bool[] emitted = new bool[types.Count];
        while (ordered.Count < types.Count)
        {
            int next = -1;
            for (int i = 0; i < types.Count; i++)
            {
                if (!emitted[i] && pending[i] == 0)
                {
                    next = i;
                    break;
                }
            }

            if (next < 0)
            {
                // Unreachable for a type hierarchy (no cycles); fall back to ordinal order.
                for (int i = 0; i < types.Count; i++)
                {
                    if (!emitted[i])
                    {
                        next = i;
                        break;
                    }
                }
            }

            emitted[next] = true;
            ordered.Add(types[next]);
            for (int i = 0; i < types.Count; i++)
            {
                if (!emitted[i] && DerivesFrom(types[next], types[i]))
                {
                    pending[i]--;
                }
            }
        }

        return ordered;
    }

    internal static bool DerivesFrom(INamedTypeSymbol derived, INamedTypeSymbol baseType)
    {
        if (SymbolEqualityComparer.Default.Equals(derived, baseType))
        {
            return false;
        }

        for (INamedTypeSymbol? current = derived.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }

        foreach (INamedTypeSymbol candidate in derived.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(candidate, baseType))
            {
                return true;
            }
        }

        return false;
    }

    private static string Fq(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
