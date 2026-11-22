#region Purpose
// Resolves dotted type names and parsed type syntax to symbols for request patterns.
#endregion

#region Design
// A dotted name is resolved by trying each split between namespace and nested types, longest
// namespace first. Display names stay dotted, which is what FullyQualifiedFormat prints. A type
// such as TimeWarp.Mediator.IRequest<App.Ping.Response> is parsed with SyntaxFactory and resolved
// part by part; only the last name segment may be generic.
// Contracts, the request interface, and response arguments often live in referenced assemblies.
// GetTypeByMetadataName returns null when two references define the same metadata name, even
// when only one is accessible to the host, so a miss falls back to GetTypesByMetadataName and
// takes the single type the host assembly can see. Two accessible types stay unresolved, as the
// C# compiler would report CS0433 for them (task 044).
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal static partial class RequestPatternReader
{
  // Resolves a dotted type name (with an optional global:: prefix) to a type in the compilation.
  internal static INamedTypeSymbol? ResolveDotted(Compilation compilation, string dottedName)
  {
    string name = StripGlobal(dottedName);
    if (name.Length == 0)
    {
      return null;
    }

    string[] segments = name.Split('.');
    for (int split = segments.Length - 1; split >= 0; split--)
    {
      StringBuilder metadata = new();
      for (int index = 0; index < split; index++)
      {
        if (index > 0)
        {
          metadata.Append('.');
        }

        metadata.Append(segments[index]);
      }

      for (int index = split; index < segments.Length; index++)
      {
        if (metadata.Length > 0)
        {
          metadata.Append(index == split ? '.' : '+');
        }

        metadata.Append(segments[index]);
      }

      INamedTypeSymbol? found = ByMetadataName(compilation, metadata.ToString());
      if (found is not null)
      {
        return found;
      }
    }

    return null;
  }

  // The type with this metadata name; among duplicates, the only one accessible to the host.
  private static INamedTypeSymbol? ByMetadataName(Compilation compilation, string metadataName)
  {
    INamedTypeSymbol? found = compilation.GetTypeByMetadataName(metadataName);
    if (found is not null)
    {
      return found;
    }

    INamedTypeSymbol? accessible = null;
    foreach (INamedTypeSymbol candidate in compilation.GetTypesByMetadataName(metadataName))
    {
      if (!compilation.IsSymbolAccessibleWithin(candidate, compilation.Assembly))
      {
        continue;
      }

      if (accessible is not null)
      {
        return null;
      }

      accessible = candidate;
    }

    return accessible;
  }

  // Scope of a declared request: the nearest existing containing type, then the host assembly.
  internal static INamedTypeSymbol? ResolveScope(Compilation compilation, string requestName)
  {
    INamedTypeSymbol? request = ResolveDotted(compilation, requestName);
    if (request is not null)
    {
      return MessageGraphBuilder.ResolveScope(request);
    }

    string name = StripGlobal(requestName);
    int lastDot = name.LastIndexOf('.');
    while (lastDot > 0)
    {
      name = name.Substring(0, lastDot);
      INamedTypeSymbol? container = ResolveDotted(compilation, name);
      if (container is not null && SymbolEqualityComparer.Default.Equals(container.ContainingAssembly, compilation.Assembly))
      {
        return MessageGraphBuilder.ResolveScope(container);
      }

      lastDot = name.LastIndexOf('.');
    }

    return MessageGraphBuilder.GetScopeFromAttributes(compilation.Assembly.GetAttributes());
  }

  internal static bool IsDottedTypeName(string name)
  {
    string stripped = StripGlobal(name);
    if (stripped.Length == 0)
    {
      return false;
    }

    foreach (string segment in stripped.Split('.'))
    {
      if (!SyntaxFacts.IsValidIdentifier(segment))
      {
        return false;
      }
    }

    return true;
  }

  internal static string StripGlobal(string name)
  {
    string trimmed = name.Trim();
    return trimmed.StartsWith("global::", StringComparison.Ordinal) ? trimmed.Substring("global::".Length) : trimmed;
  }

  // Parses a type name such as TimeWarp.Mediator.IRequest<App.Ping.Response> and resolves it.
  internal static ITypeSymbol? ResolveTypeName(Compilation compilation, string text)
  {
    TypeSyntax syntax = SyntaxFactory.ParseTypeName(text);
    if (syntax.ContainsDiagnostics)
    {
      return null;
    }

    return Resolve(compilation, syntax);
  }

  private static ITypeSymbol? Resolve(Compilation compilation, TypeSyntax syntax)
  {
    switch (syntax)
    {
      case PredefinedTypeSyntax predefined:
        SpecialType special = Special(predefined.Keyword.Kind());
        return special == SpecialType.None ? null : compilation.GetSpecialType(special);
      case ArrayTypeSyntax array:
        ITypeSymbol? element = Resolve(compilation, array.ElementType);
        if (element is null)
        {
          return null;
        }

        for (int index = array.RankSpecifiers.Count - 1; index >= 0; index--)
        {
          element = compilation.CreateArrayTypeSymbol(element, array.RankSpecifiers[index].Rank);
        }

        return element;
      case NullableTypeSyntax nullable:
        ITypeSymbol? inner = Resolve(compilation, nullable.ElementType);
        if (inner is null)
        {
          return null;
        }

        return inner.IsValueType
            ? compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(inner)
            : inner;
      case AliasQualifiedNameSyntax alias when alias.Alias.Identifier.ValueText == "global":
        return Resolve(compilation, alias.Name);
      case NameSyntax name:
        return ResolveName(compilation, name);
      default:
        return null;
    }
  }

  private static ITypeSymbol? ResolveName(Compilation compilation, NameSyntax name)
  {
    List<SimpleNameSyntax> parts = new();
    NameSyntax? current = name;
    while (current is not null)
    {
      switch (current)
      {
        case QualifiedNameSyntax qualified:
          parts.Add(qualified.Right);
          current = qualified.Left;
          break;
        case AliasQualifiedNameSyntax alias when alias.Alias.Identifier.ValueText == "global":
          parts.Add(alias.Name);
          current = null;
          break;
        case SimpleNameSyntax simple:
          parts.Add(simple);
          current = null;
          break;
        default:
          return null;
      }
    }

    parts.Reverse();
    StringBuilder dotted = new();
    for (int index = 0; index < parts.Count; index++)
    {
      SimpleNameSyntax part = parts[index];
      bool last = index == parts.Count - 1;
      if (part is GenericNameSyntax && !last)
      {
        return null;
      }

      if (dotted.Length > 0)
      {
        dotted.Append('.');
      }

      dotted.Append(part.Identifier.ValueText);
      if (part is GenericNameSyntax generic)
      {
        dotted.Append('`').Append(generic.TypeArgumentList.Arguments.Count);
      }
    }

    INamedTypeSymbol? definition = ResolveDotted(compilation, dotted.ToString());
    if (definition is null)
    {
      return null;
    }

    if (parts[parts.Count - 1] is not GenericNameSyntax closed)
    {
      return definition;
    }

    List<ITypeSymbol> arguments = new();
    foreach (TypeSyntax argument in closed.TypeArgumentList.Arguments)
    {
      ITypeSymbol? resolved = Resolve(compilation, argument);
      if (resolved is null)
      {
        return null;
      }

      arguments.Add(resolved);
    }

    return definition.Construct(arguments.ToArray());
  }

  private static SpecialType Special(SyntaxKind keyword)
  {
    return keyword switch
    {
      SyntaxKind.BoolKeyword => SpecialType.System_Boolean,
      SyntaxKind.ByteKeyword => SpecialType.System_Byte,
      SyntaxKind.SByteKeyword => SpecialType.System_SByte,
      SyntaxKind.ShortKeyword => SpecialType.System_Int16,
      SyntaxKind.UShortKeyword => SpecialType.System_UInt16,
      SyntaxKind.IntKeyword => SpecialType.System_Int32,
      SyntaxKind.UIntKeyword => SpecialType.System_UInt32,
      SyntaxKind.LongKeyword => SpecialType.System_Int64,
      SyntaxKind.ULongKeyword => SpecialType.System_UInt64,
      SyntaxKind.CharKeyword => SpecialType.System_Char,
      SyntaxKind.FloatKeyword => SpecialType.System_Single,
      SyntaxKind.DoubleKeyword => SpecialType.System_Double,
      SyntaxKind.DecimalKeyword => SpecialType.System_Decimal,
      SyntaxKind.StringKeyword => SpecialType.System_String,
      SyntaxKind.ObjectKeyword => SpecialType.System_Object,
      _ => SpecialType.None,
    };
  }
}
