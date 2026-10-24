#region Purpose
// Value-equatable view of a type: display strings, hierarchy keys, and accessibility. No symbols.
#endregion

#region Design
// The model step cannot take a Compilation, or an unrelated edit re-runs it (Unchanged, not Cached).
// Syntax and metadata steps copy everything emission and diagnostics need into this record.
// Key is assembly name plus documentation id, so two assemblies can share a display name.
// BaseKeys walk named types only. InterfaceKeys come from AllInterfaces on every ITypeSymbol,
// so an array response still satisfies an interface constraint. TypeArguments keep the nested
// shape a pipeline slot compares, such as Result<TRequest>.
// Display is parameterless ToDisplayString; ErrorDisplay is CSharpErrorMessageFormat; FullyQualified
// is FullyQualifiedFormat. Closed behaviors reuse those prefixes with the request and response substituted.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed record TypeModel(
    string Key,
    string DefinitionKey,
    string FullyQualified,
    string Display,
    string ErrorDisplay,
    string Name,
    string AssemblyName,
    bool IsValueType,
    bool IsReferenceType,
    bool IsUnmanagedType,
    bool IsAbstract,
    bool IsClassOrStruct,
    bool IsNamedType,
    int Arity,
    bool IsDiscoverable,
    bool HasPublicParameterlessConstructor,
    bool Accessible,
    TypeModel? InaccessiblePart,
    EquatableArray<string> BaseKeys,
    EquatableArray<string> BaseDefinitionKeys,
    EquatableArray<string> InterfaceKeys,
    EquatableArray<string> InterfaceDefinitionKeys,
    EquatableArray<string> MediatorInterfaceNames,
    int ArrayRank,
    EquatableArray<TypeModel> TypeArguments,
    LocationInfo? Location) : IEquatable<TypeModel>;

internal static class TypeModels
{
  internal static bool Same(TypeModel? left, TypeModel? right)
  {
    if (left is null || right is null)
    {
      return left is null && right is null;
    }

    return left.Key == right.Key;
  }

  internal static TypeModel Create(ITypeSymbol type, Compilation accessCompilation)
  {
    ITypeSymbol? inaccessible = FindInaccessible(type, accessCompilation);
    TypeModel? part = inaccessible is null
        ? null
        : CreateCore(inaccessible, accessCompilation, accessible: false, inaccessiblePart: null);
    return CreateCore(type, accessCompilation, part is null, part);
  }

  internal static string KeyOf(ITypeSymbol type)
  {
    if (type is IArrayTypeSymbol array)
    {
      return "a" + array.Rank + ":" + KeyOf(array.ElementType);
    }

    if (type is INamedTypeSymbol named)
    {
      string assembly = named.ContainingAssembly?.Name ?? string.Empty;
      string id = named.GetDocumentationCommentId()
          ?? named.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      return assembly + "|" + id;
    }

    if (type is ITypeParameterSymbol parameter)
    {
      return "p:" + parameter.Ordinal + ":" + parameter.Name;
    }

    return "o:" + type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
  }

  internal static string DefinitionKeyOf(ITypeSymbol type)
  {
    return type is INamedTypeSymbol named ? KeyOf(named.OriginalDefinition) : KeyOf(type);
  }

  private static TypeModel CreateCore(
      ITypeSymbol type,
      Compilation accessCompilation,
      bool accessible,
      TypeModel? inaccessiblePart)
  {
    List<string> baseKeys = new();
    List<string> baseDefinitionKeys = new();
    List<string> interfaceKeys = new();
    List<string> interfaceDefinitionKeys = new();
    List<string> mediatorNames = new();
    List<TypeModel> typeArguments = new();
    int arrayRank = 0;
    if (type is INamedTypeSymbol named)
    {
      for (INamedTypeSymbol? current = named.BaseType; current is not null; current = current.BaseType)
      {
        baseKeys.Add(KeyOf(current));
        baseDefinitionKeys.Add(DefinitionKeyOf(current));
      }

      foreach (ITypeSymbol argument in named.TypeArguments)
      {
        typeArguments.Add(Create(argument, accessCompilation));
      }
    }
    else if (type is IArrayTypeSymbol array)
    {
      arrayRank = array.Rank;
      typeArguments.Add(Create(array.ElementType, accessCompilation));
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      interfaceKeys.Add(KeyOf(iface));
      interfaceDefinitionKeys.Add(DefinitionKeyOf(iface));
      if (iface.ContainingNamespace?.ToDisplayString() == "TimeWarp.Mediator")
      {
        mediatorNames.Add(iface.OriginalDefinition.Name);
      }
    }

    bool isNamed = type is INamedTypeSymbol;
    INamedTypeSymbol? namedType = type as INamedTypeSymbol;
    return new TypeModel(
        KeyOf(type),
        DefinitionKeyOf(type),
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        type.ToDisplayString(),
        type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        namedType?.Name ?? type.Name,
        namedType?.ContainingAssembly?.Name ?? string.Empty,
        type.IsValueType,
        type.IsReferenceType,
        type.IsUnmanagedType,
        namedType?.IsAbstract ?? false,
        namedType?.TypeKind is TypeKind.Class or TypeKind.Struct,
        isNamed,
        namedType?.Arity ?? 0,
        namedType is not null && IsDiscoverable(namedType),
        HasPublicParameterlessConstructor(namedType),
        accessible,
        inaccessiblePart,
        new EquatableArray<string>(baseKeys.ToImmutableArray()),
        new EquatableArray<string>(baseDefinitionKeys.ToImmutableArray()),
        new EquatableArray<string>(interfaceKeys.ToImmutableArray()),
        new EquatableArray<string>(interfaceDefinitionKeys.ToImmutableArray()),
        new EquatableArray<string>(mediatorNames.ToImmutableArray()),
        arrayRank,
        new EquatableArray<TypeModel>(typeArguments.ToImmutableArray()),
        LocationOf(type, accessCompilation));
  }

  internal static bool IsDiscoverable(INamedTypeSymbol type)
  {
    if (type.IsStatic || type.TypeKind is TypeKind.Interface or TypeKind.Delegate or TypeKind.Enum)
    {
      return false;
    }

    for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
    {
      if (current.DeclaredAccessibility is Accessibility.Private
          or Accessibility.Protected
          or Accessibility.ProtectedAndInternal)
      {
        return false;
      }
    }

    return true;
  }

  private static bool HasPublicParameterlessConstructor(INamedTypeSymbol? type)
  {
    if (type is null)
    {
      return false;
    }

    foreach (IMethodSymbol constructor in type.InstanceConstructors)
    {
      if (constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility == Accessibility.Public)
      {
        return true;
      }
    }

    return false;
  }

  private static ITypeSymbol? FindInaccessible(ITypeSymbol type, Compilation compilation)
  {
    if (type is IArrayTypeSymbol array)
    {
      return FindInaccessible(array.ElementType, compilation);
    }

    if (type is INamedTypeSymbol named)
    {
      foreach (ITypeSymbol argument in named.TypeArguments)
      {
        ITypeSymbol? inner = FindInaccessible(argument, compilation);
        if (inner is not null)
        {
          return inner;
        }
      }

      INamedTypeSymbol definition = named.OriginalDefinition;
      return compilation.IsSymbolAccessibleWithin(definition, compilation.Assembly) ? null : definition;
    }

    return type is ITypeParameterSymbol || compilation.IsSymbolAccessibleWithin(type, compilation.Assembly)
        ? null
        : type;
  }

  private static LocationInfo? LocationOf(ISymbol symbol, Compilation compilation)
  {
    foreach (Location location in symbol.Locations)
    {
      if (location.IsInSource && location.SourceTree is not null && compilation.ContainsSyntaxTree(location.SourceTree))
      {
        return LocationInfo.From(location);
      }
    }

    return null;
  }
}

// Dictionary and hash-set identity for a type. Record equality includes location, which differs
// when the same type is copied from a handler and from its declaration.
internal sealed class TypeModelKeyComparer : IEqualityComparer<TypeModel>
{
  public static readonly TypeModelKeyComparer Instance = new();

  public bool Equals(TypeModel? left, TypeModel? right)
  {
    return TypeModels.Same(left, right);
  }

  public int GetHashCode(TypeModel type)
  {
    return StringComparer.Ordinal.GetHashCode(type.Key);
  }
}
