#region Purpose
// Reads a pattern's templates and contracts and expands {Host}, {Contract}, and {ContractType}.
#endregion

#region Design
// A template is a constructor argument of the same name or a const string field on the attribute
// class chain. The interface's IRequest<T> argument is the response; IRequest alone is void.
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal static partial class RequestPatternReader
{
  // Namespace, containing types, and name, dotted. Null when any part is generic.
  private static string? DottedName(INamedTypeSymbol type)
  {
    List<string> parts = new();
    for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
    {
      if (current.Arity > 0 || current.IsUnboundGenericType)
      {
        return null;
      }

      parts.Add(current.Name);
    }

    parts.Reverse();
    string typePart = string.Join(".", parts);
    return type.ContainingNamespace is { IsGlobalNamespace: false } containing
        ? containing.ToDisplayString() + "." + typePart
        : typePart;
  }

  private static bool TryExpand(
      string template,
      string? host,
      string contract,
      string contractType,
      out string expanded,
      out string? problem)
  {
    StringBuilder builder = new();
    problem = null;
    int index = 0;
    while (index < template.Length)
    {
      char character = template[index];
      if (character == '}')
      {
        expanded = string.Empty;
        problem = "'" + template + "' has an unmatched '}'";
        return false;
      }

      if (character != '{')
      {
        builder.Append(character);
        index++;
        continue;
      }

      int close = template.IndexOf('}', index + 1);
      if (close < 0)
      {
        expanded = string.Empty;
        problem = "'" + template + "' has an unmatched '{'";
        return false;
      }

      string placeholder = template.Substring(index + 1, close - index - 1);
      switch (placeholder)
      {
        case "Host":
          if (host is null)
          {
            expanded = string.Empty;
            problem = "{Host} is a generic type";
            return false;
          }

          builder.Append(host);
          break;
        case "Contract":
          builder.Append(contract);
          break;
        case "ContractType":
          builder.Append(contractType);
          break;
        default:
          expanded = string.Empty;
          problem = "unknown placeholder '{" + placeholder + "}'; use {Host}, {Contract}, or {ContractType}";
          return false;
      }

      index = close + 1;
    }

    expanded = builder.ToString();
    return true;
  }

  private static bool TryReadTemplate(AttributeData attribute, string parameterName, string fieldName, out string value)
  {
    value = string.Empty;
    IMethodSymbol? constructor = attribute.AttributeConstructor;
    if (constructor is not null)
    {
      ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
      ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;
      int count = Math.Min(parameters.Length, arguments.Length);
      for (int index = 0; index < count; index++)
      {
        if (!string.Equals(parameters[index].Name, parameterName, StringComparison.Ordinal))
        {
          continue;
        }

        TypedConstant argument = arguments[index];
        if (argument.Kind == TypedConstantKind.Primitive && argument.Value is string text && text.Trim().Length > 0)
        {
          value = text;
          return true;
        }

        return false;
      }
    }

    for (INamedTypeSymbol? current = attribute.AttributeClass; current is not null; current = current.BaseType)
    {
      foreach (ISymbol member in current.GetMembers(fieldName))
      {
        if (member is IFieldSymbol { IsConst: true, ConstantValue: string text } && text.Trim().Length > 0)
        {
          value = text;
          return true;
        }
      }
    }

    return false;
  }

  private static bool TryReadContracts(AttributeData attribute, out ImmutableArray<TypedConstant> contracts)
  {
    contracts = ImmutableArray<TypedConstant>.Empty;
    IMethodSymbol? constructor = attribute.AttributeConstructor;
    if (constructor is null)
    {
      return false;
    }

    ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
    ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;
    int count = Math.Min(parameters.Length, arguments.Length);
    for (int index = 0; index < count; index++)
    {
      if (!string.Equals(parameters[index].Name, "contracts", StringComparison.Ordinal))
      {
        continue;
      }

      TypedConstant argument = arguments[index];
      if (argument.Kind == TypedConstantKind.Array && !argument.IsNull)
      {
        contracts = argument.Values;
        return true;
      }

      return false;
    }

    return false;
  }

  private static bool TryReadResponse(INamedTypeSymbol requestInterface, out ITypeSymbol? response)
  {
    response = null;
    List<INamedTypeSymbol> interfaces = new() { requestInterface };
    interfaces.AddRange(requestInterface.AllInterfaces);
    foreach (INamedTypeSymbol iface in interfaces)
    {
      if (iface.OriginalDefinition.ToDisplayString() == "TimeWarp.Mediator.IRequest<TResponse>"
          && iface.TypeArguments.Length == 1)
      {
        response = iface.TypeArguments[0];
        return true;
      }
    }

    foreach (INamedTypeSymbol iface in interfaces)
    {
      if (iface.Arity == 0 && iface.ToDisplayString() == Request0)
      {
        return true;
      }
    }

    return false;
  }
}
