#region Purpose
// Orders message types most-derived first so emitted type tests never subsume a later one.
#endregion

#region Design
// A `case Base x:` before `case Derived y:` is CS8120, and an `if (o is Base)` chain would run the
// base branch first. Kahn's algorithm over "derived must precede its bases" edges; among ready types
// the ordinal fully qualified name wins, so output is deterministic. Assignability is the copied
// base-class chain plus interface keys (exact type key). Generic variance (IEvent<out T>) is not modelled.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static class TypeOrder
{
  internal static List<TypeModel> MostDerivedFirst(IEnumerable<TypeModel> source)
  {
    List<TypeModel> types = new();
    HashSet<TypeModel> seen = new(TypeModelKeyComparer.Instance);
    foreach (TypeModel type in source)
    {
      if (seen.Add(type))
      {
        types.Add(type);
      }
    }

    types.Sort((left, right) => string.CompareOrdinal(left.FullyQualified, right.FullyQualified));

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

    List<TypeModel> ordered = new(types.Count);
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

  internal static bool DerivesFrom(TypeModel derived, TypeModel baseType)
  {
    if (TypeModels.Same(derived, baseType))
    {
      return false;
    }

    foreach (string key in derived.BaseKeys)
    {
      if (key == baseType.Key)
      {
        return true;
      }
    }

    foreach (string key in derived.InterfaceKeys)
    {
      if (key == baseType.Key)
      {
        return true;
      }
    }

    return false;
  }
}
