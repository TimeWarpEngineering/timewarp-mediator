#region Purpose
// Closes pipeline behaviors and orders member facts.
#endregion

#region Design
// Constraint checks use the slots copied at the syntax boundary. Notification groups keep one scope per notification.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class CandidateGraph
{
  private static ImmutableArray<TypeModel> CloseBehaviors(
      List<OrderedBehavior> behaviors,
      TypeModel requestType,
      TypeModel responseType,
      TypeModel? requestScope,
      HashSet<int> woven)
  {
    List<TypeModel> closed = new();
    foreach (OrderedBehavior behavior in behaviors)
    {
      if (!TypeModels.Same(behavior.Behavior.Scope, requestScope))
      {
        continue;
      }

      TypeModel? constructed = TryClose(behavior.Behavior, requestType, responseType);
      if (constructed is null)
      {
        continue;
      }

      closed.Add(constructed);
      woven.Add(behavior.SourceIndex);
    }

    return closed.ToImmutableArray();
  }

  private static TypeModel? TryClose(BehaviorFact behavior, TypeModel requestType, TypeModel responseType)
  {
    if (behavior.Parameters.Length == 0)
    {
      return ImplementsPipeline(behavior, Array.Empty<TypeModel>(), requestType, responseType) ? behavior.Definition : null;
    }

    TypeModel[] arguments;
    if (behavior.Parameters.Length == 2)
    {
      arguments = new[] { requestType, responseType };
    }
    else if (behavior.Parameters.Length == 1)
    {
      arguments = new[] { requestType };
    }
    else
    {
      return null;
    }

    if (!CanConstruct(behavior.Parameters, arguments))
    {
      return null;
    }

    return ImplementsPipeline(behavior, arguments, requestType, responseType)
        ? CloseGeneric(behavior, arguments)
        : null;
  }

  private static bool ImplementsPipeline(
      BehaviorFact behavior,
      TypeModel[] arguments,
      TypeModel requestType,
      TypeModel responseType)
  {
    foreach (PipelineSlot pipeline in behavior.Pipelines)
    {
      if (SlotMatches(pipeline.Request, requestType, arguments)
          && SlotMatches(pipeline.Response, responseType, arguments))
      {
        return true;
      }
    }

    return false;
  }

  private static bool SlotMatches(TypeArgSlot slot, TypeModel type, TypeModel[] arguments)
  {
    if (slot.ParameterIndex >= 0)
    {
      return slot.ParameterIndex < arguments.Length && arguments[slot.ParameterIndex].Key == type.Key;
    }

    if (slot.ArrayRank > 0)
    {
      if (type.ArrayRank != slot.ArrayRank || !TryArgument(slot.Arguments, 0, out TypeArgSlot element))
      {
        return false;
      }

      return TryTypeArgument(type.TypeArguments, 0, out TypeModel elementType)
          && SlotMatches(element, elementType, arguments);
    }

    if (slot.GenericDefinitionKey is not null)
    {
      if (type.DefinitionKey != slot.GenericDefinitionKey || type.TypeArguments.Length != slot.Arguments.Length)
      {
        return false;
      }

      int index = 0;
      foreach (TypeArgSlot child in slot.Arguments)
      {
        if (!TryTypeArgument(type.TypeArguments, index, out TypeModel childType) || !SlotMatches(child, childType, arguments))
        {
          return false;
        }

        index++;
      }

      return true;
    }

    return slot.ConcreteKey == type.Key;
  }

  private static bool TryArgument(EquatableArray<TypeArgSlot> slots, int index, out TypeArgSlot slot)
  {
    int current = 0;
    foreach (TypeArgSlot candidate in slots)
    {
      if (current == index)
      {
        slot = candidate;
        return true;
      }

      current++;
    }

    slot = null!;
    return false;
  }

  private static bool TryTypeArgument(EquatableArray<TypeModel> types, int index, out TypeModel type)
  {
    int current = 0;
    foreach (TypeModel candidate in types)
    {
      if (current == index)
      {
        type = candidate;
        return true;
      }

      current++;
    }

    type = null!;
    return false;
  }

  private static bool CanConstruct(EquatableArray<ParameterSlot> parameters, TypeModel[] arguments)
  {
    if (parameters.Length != arguments.Length)
    {
      return false;
    }

    for (int i = 0; i < parameters.Length; i++)
    {
      ParameterSlot parameter = parameters.ElementAt(i);
      TypeModel argument = arguments[i];
      if (parameter.HasTypeParameterConstraint)
      {
        return false;
      }

      if (parameter.ReferenceType && !argument.IsReferenceType)
      {
        return false;
      }

      if (parameter.ValueType && !argument.IsValueType)
      {
        return false;
      }

      if (parameter.Unmanaged && !argument.IsUnmanagedType)
      {
        return false;
      }

      foreach (ConstraintSlot constraint in parameter.Constraints)
      {
        if (!ImplementsOrInherits(argument, constraint))
        {
          return false;
        }
      }

      if (parameter.Constructor)
      {
        if (!argument.IsNamedType || argument.IsAbstract || !argument.HasPublicParameterlessConstructor)
        {
          return false;
        }
      }
    }

    return true;
  }

  private static bool ImplementsOrInherits(TypeModel type, ConstraintSlot constraint)
  {
    if (type.Key == constraint.ConcreteKey || type.DefinitionKey == constraint.DefinitionKey)
    {
      return true;
    }

    for (int i = 0; i < type.BaseKeys.Length; i++)
    {
      if (KeyAt(type.BaseKeys, i) == constraint.ConcreteKey || KeyAt(type.BaseDefinitionKeys, i) == constraint.DefinitionKey)
      {
        return true;
      }
    }

    for (int i = 0; i < type.InterfaceKeys.Length; i++)
    {
      if (KeyAt(type.InterfaceKeys, i) == constraint.ConcreteKey
          || KeyAt(type.InterfaceDefinitionKeys, i) == constraint.DefinitionKey)
      {
        return true;
      }
    }

    return false;
  }

  private static string KeyAt(EquatableArray<string> keys, int index)
  {
    int current = 0;
    foreach (string key in keys)
    {
      if (current == index)
      {
        return key;
      }

      current++;
    }

    return string.Empty;
  }

  private static TypeModel CloseGeneric(BehaviorFact behavior, TypeModel[] arguments)
  {
    string argumentsFullyQualified = string.Join(", ", arguments.Select(argument => argument.FullyQualified));
    string argumentsDisplay = string.Join(", ", arguments.Select(argument => argument.Display));
    string argumentsError = string.Join(", ", arguments.Select(argument => argument.ErrorDisplay));
    string key = behavior.Definition.AssemblyName + "|closed:" + behavior.Definition.Key + "<" + string.Join(",", arguments.Select(argument => argument.Key)) + ">";
    return behavior.Definition with
    {
      Key = key,
      FullyQualified = behavior.FullyQualifiedPrefix + "<" + argumentsFullyQualified + ">",
      Display = behavior.DisplayPrefix + "<" + argumentsDisplay + ">",
      ErrorDisplay = behavior.ErrorDisplayPrefix + "<" + argumentsError + ">",
      Arity = arguments.Length,
      ArrayRank = 0,
      TypeArguments = new EquatableArray<TypeModel>(arguments.ToImmutableArray()),
      Location = null,
      InaccessiblePart = null,
      Accessible = true,
    };
  }

  private static List<TypeFact> OrderFacts(HostInput host, ReferencedInput referenced)
  {
    List<(string AssemblyName, int Index, TypeFact Fact)> ordered = new();
    int index = 0;
    foreach (TypeFact fact in host.Types)
    {
      ordered.Add((fact.Type.AssemblyName, index, fact));
      index++;
    }

    foreach (TypeFact fact in referenced.Types)
    {
      ordered.Add((fact.Type.AssemblyName, index, fact));
      index++;
    }

    ordered.Sort(static (left, right) =>
    {
      int name = string.CompareOrdinal(left.AssemblyName, right.AssemblyName);
      if (name != 0)
      {
        return name;
      }

      return left.Index.CompareTo(right.Index);
    });

    List<TypeFact> facts = new();
    foreach ((string _, int _, TypeFact fact) in ordered)
    {
      facts.Add(fact);
    }

    return facts;
  }

  private static string? FirstMissingRuntime(EquatableArray<string> missing, EquatableArray<string> declared)
  {
    foreach (string name in missing)
    {
      if (!Contains(declared, name))
      {
        return name;
      }
    }

    return null;
  }

  private static bool Contains(EquatableArray<string> values, string candidate)
  {
    foreach (string value in values)
    {
      if (value == candidate)
      {
        return true;
      }
    }

    return false;
  }

  private static int CompareBehaviors(BehaviorFact left, BehaviorFact right)
  {
    int assembly = string.CompareOrdinal(left.AssemblyName, right.AssemblyName);
    if (assembly != 0)
    {
      return assembly;
    }

    int path = string.CompareOrdinal(left.Path, right.Path);
    if (path != 0)
    {
      return path;
    }

    return left.SpanStart.CompareTo(right.SpanStart);
  }

  private static int CompareOrdered(OrderedBehavior left, OrderedBehavior right)
  {
    int order = left.Behavior.Order.CompareTo(right.Behavior.Order);
    if (order != 0)
    {
      return order;
    }

    return left.SourceIndex.CompareTo(right.SourceIndex);
  }

  private static void SortByDisplay(List<TypeModel> types)
  {
    types.Sort((left, right) => string.CompareOrdinal(left.Display, right.Display));
  }

  private static DiagnosticInfo Info(DiagnosticDescriptor descriptor, LocationInfo? location, params object[] args)
  {
    Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, args);
    return DiagnosticInfo.From(diagnostic) with { Location = location };
  }

  private readonly struct OrderedBehavior
  {
    public OrderedBehavior(BehaviorFact behavior, int sourceIndex)
    {
      Behavior = behavior;
      SourceIndex = sourceIndex;
    }

    public BehaviorFact Behavior { get; }

    public int SourceIndex { get; }
  }

  private sealed class NotificationGroup
  {
    public NotificationGroup(TypeModel notification, TypeModel? scope)
    {
      Notification = notification;
      Scope = scope;
      Handlers = new List<TypeModel>();
    }

    public TypeModel Notification { get; }

    public TypeModel? Scope { get; }

    public List<TypeModel> Handlers { get; }
  }
}
