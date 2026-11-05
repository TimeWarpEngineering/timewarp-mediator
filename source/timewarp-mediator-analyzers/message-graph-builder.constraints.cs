#region Purpose
// Closes behaviors and answers type, scope, and location questions for the symbol walk.
#endregion

#region Design
// Accessibility uses this compilation. Diagnostic locations stay on syntax trees this compilation contains.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public static partial class MessageGraphBuilder
{
  private static ImmutableArray<INamedTypeSymbol> CloseBehaviors(
      ImmutableArray<BehaviorRegistration> behaviors,
      INamedTypeSymbol requestType,
      ITypeSymbol responseType,
      INamedTypeSymbol? pipelineBehavior2,
      INamedTypeSymbol? requestScope,
      HashSet<BehaviorRegistration> wovenBehaviors)
  {
    ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
    foreach (BehaviorRegistration registration in behaviors)
    {
      if (!SymbolEqualityComparer.Default.Equals(registration.ScopeType, requestScope))
      {
        continue;
      }

      INamedTypeSymbol? closed = TryCloseBehavior(registration.BehaviorType, requestType, responseType, pipelineBehavior2);
      if (closed is not null)
      {
        builder.Add(closed);
        wovenBehaviors.Add(registration);
      }
    }

    return builder.ToImmutable();
  }

  internal static INamedTypeSymbol? ResolveScope(INamedTypeSymbol type)
  {
    for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
    {
      INamedTypeSymbol? typeScope = GetScopeFromAttributes(current.GetAttributes());
      if (typeScope is not null)
      {
        return typeScope;
      }
    }

    return type.ContainingAssembly is null
        ? null
        : GetScopeFromAttributes(type.ContainingAssembly.GetAttributes());
  }

  private static INamedTypeSymbol? GetScopeFromAttributes(System.Collections.Immutable.ImmutableArray<AttributeData> attributes)
  {
    foreach (AttributeData attribute in attributes)
    {
      if (attribute.AttributeClass?.ToDisplayString() != Membership.ScopeAttributeMetadataName)
      {
        continue;
      }

      if (attribute.ConstructorArguments.Length > 0
          && attribute.ConstructorArguments[0].Value is INamedTypeSymbol scopeType)
      {
        return scopeType;
      }
    }

    return null;
  }

  private static INamedTypeSymbol? TryCloseBehavior(
      INamedTypeSymbol behaviorType,
      INamedTypeSymbol requestType,
      ITypeSymbol responseType,
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
    return ImplementsPipeline(constructed, requestType, responseType, pipelineBehavior2) ? constructed : null;
  }

  private static bool ImplementsPipeline(
      INamedTypeSymbol behaviorType,
      INamedTypeSymbol requestType,
      ITypeSymbol responseType,
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

  private static Location GetSourceLocation(Compilation compilation, ISymbol symbol)
  {
    foreach (Location location in symbol.Locations)
    {
      if (location.IsInSource && compilation.ContainsSyntaxTree(location.SourceTree!))
      {
        return location;
      }
    }

    return Location.None;
  }

  private static Location GetAttributeLocation(Compilation compilation, AttributeData attribute)
  {
    SyntaxReference? reference = attribute.ApplicationSyntaxReference;
    return reference is not null && compilation.ContainsSyntaxTree(reference.SyntaxTree)
        ? reference.GetSyntax().GetLocation()
        : Location.None;
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

  private readonly struct RequestHandlerMatch
  {
    public RequestHandlerMatch(INamedTypeSymbol requestType, ITypeSymbol responseType, bool isVoid)
    {
      RequestType = requestType;
      ResponseType = responseType;
      IsVoid = isVoid;
    }

    public INamedTypeSymbol RequestType { get; }

    public ITypeSymbol ResponseType { get; }

    public bool IsVoid { get; }
  }

  // Reports TWM007 once per inaccessible type and says whether a type may appear in generated code.
  private sealed class AccessGuard
  {
    private readonly Compilation Compilation;
    private readonly List<Diagnostic> Diagnostics;
    private readonly HashSet<ITypeSymbol> Reported = new(SymbolEqualityComparer.Default);

    public AccessGuard(Compilation compilation, List<Diagnostic> diagnostics)
    {
      Compilation = compilation;
      Diagnostics = diagnostics;
    }

    public bool Allows(ITypeSymbol type)
    {
      ITypeSymbol? inaccessible = FindInaccessible(type);
      if (inaccessible is null)
      {
        return true;
      }

      if (Reported.Add(inaccessible))
      {
        Diagnostics.Add(
            Diagnostic.Create(
                DiagnosticDescriptors.InaccessibleMemberType,
                GetSourceLocation(Compilation, inaccessible),
                inaccessible.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                inaccessible.ContainingAssembly?.Name ?? string.Empty,
                Compilation.AssemblyName ?? string.Empty));
      }

      return false;
    }

    private ITypeSymbol? FindInaccessible(ITypeSymbol type)
    {
      if (type is IArrayTypeSymbol array)
      {
        return FindInaccessible(array.ElementType);
      }

      if (type is INamedTypeSymbol named)
      {
        foreach (ITypeSymbol argument in named.TypeArguments)
        {
          ITypeSymbol? inner = FindInaccessible(argument);
          if (inner is not null)
          {
            return inner;
          }
        }

        INamedTypeSymbol definition = named.OriginalDefinition;
        return Compilation.IsSymbolAccessibleWithin(definition, Compilation.Assembly) ? null : definition;
      }

      return type is ITypeParameterSymbol || Compilation.IsSymbolAccessibleWithin(type, Compilation.Assembly)
          ? null
          : type;
    }
  }

  private sealed class NotificationGroup
  {
    public NotificationGroup(INamedTypeSymbol notification, INamedTypeSymbol? scope)
    {
      Notification = notification;
      Scope = scope;
      Handlers = new List<INamedTypeSymbol>();
    }

    public INamedTypeSymbol Notification { get; }

    public INamedTypeSymbol? Scope { get; }

    public List<INamedTypeSymbol> Handlers { get; }
  }
}
