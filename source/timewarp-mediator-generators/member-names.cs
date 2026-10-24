#region Purpose
// Collision-safe sanitized identifiers for Dispatch_*, Publish_*, handler cache slots, and scope suffixes.
#endregion

#region Design
// Sanitize maps every non-alphanumeric to '_', so A.B_C and A_B.C share a name. Names are assigned
// once per graph: a type whose sanitized name is unique keeps it unchanged (benchmarks and callers
// target Dispatch_global__Ns_Type directly). Every type in a colliding group gets a suffix of the
// FNV-1a hash of its fully qualified name, so the result depends only on the type, not on graph
// order. A further clash (vanishingly rare) appends an index in ordinal display-name order.
// Notification handler types are omitted. No emitted member is named from them, and including
// them would hash-suffix a Dispatch_* or Publish_* name that only collides with a handler.
// Identity is the type key, not record equality, so a declaration copy and a closed copy of the
// same type share one name.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed class MemberNames
{
  private readonly Dictionary<TypeModel, string> Names = new(TypeModelKeyComparer.Instance);

  public MemberNames(EmissionGraph graph)
  {
    List<TypeModel> types = new();
    HashSet<TypeModel> seen = new(TypeModelKeyComparer.Instance);
    foreach (EmissionRequest request in graph.Requests)
    {
      Add(types, seen, request.RequestType);
      Add(types, seen, request.HandlerType);
      Add(types, seen, request.ScopeType);
    }

    foreach (EmissionNotification notification in graph.Notifications)
    {
      Add(types, seen, notification.NotificationType);
      Add(types, seen, notification.ScopeType);
    }

    types.Sort((left, right) => string.CompareOrdinal(left.FullyQualified, right.FullyQualified));

    Dictionary<string, int> counts = new(StringComparer.Ordinal);
    foreach (TypeModel type in types)
    {
      string sanitized = Sanitize(type.FullyQualified);
      counts[sanitized] = counts.TryGetValue(sanitized, out int count) ? count + 1 : 1;
    }

    HashSet<string> taken = new(StringComparer.Ordinal);
    List<TypeModel> colliding = new();
    foreach (TypeModel type in types)
    {
      string sanitized = Sanitize(type.FullyQualified);
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

    foreach (TypeModel type in colliding)
    {
      string candidate = Sanitize(type.FullyQualified) + "_" + StableHash(type.FullyQualified);
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

  public string For(TypeModel type)
  {
    return Names.TryGetValue(type, out string? name) ? name : Sanitize(type.FullyQualified);
  }

  private static void Add(List<TypeModel> types, HashSet<TypeModel> seen, TypeModel? type)
  {
    if (type is not null && seen.Add(type))
    {
      types.Add(type);
    }
  }

  // Same character map as MessageGraphBuilder.Sanitize, over the fully qualified display string.
  private static string Sanitize(string fullyQualified)
  {
    StringBuilder builder = new(fullyQualified.Length);
    foreach (char c in fullyQualified)
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
