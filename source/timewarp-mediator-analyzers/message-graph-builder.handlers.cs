#region Purpose
// Collects declared handlers, request handlers, and behavior registrations.
#endregion

#region Design
// A declared name and an IRequestHandler for the same symbol are one handler. Behavior order is the Order argument, then source index.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public static partial class MessageGraphBuilder
{
  private static void CollectDeclaredHandlers(
      Compilation compilation,
      INamedTypeSymbol type,
      INamedTypeSymbol? request0,
      INamedTypeSymbol? request1,
      INamedTypeSymbol? unitType,
      INamedTypeSymbol? generatedHandlerAttribute,
      Membership membership,
      AccessGuard access,
      bool reportUnreadable,
      Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> handlersByRequest,
      Dictionary<INamedTypeSymbol, ITypeSymbol> responseByRequest,
      HashSet<INamedTypeSymbol> voidRequests,
      HashSet<INamedTypeSymbol> requests,
      HashSet<INamedTypeSymbol> droppedMessages,
      List<DeclaredHandler> declaredHandlers,
      List<Diagnostic> declaredDiagnostics,
      HashSet<INamedTypeSymbol> declaredCovered,
      Dictionary<INamedTypeSymbol, List<string>> declaredNames)
  {
    if (generatedHandlerAttribute is null
        || type.TypeKind is not (TypeKind.Class or TypeKind.Struct)
        || type.IsAbstract
        || type.Arity > 0
        || !IsDiscoverableType(type)
        || !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly)
        || (!Implements(type, request0) && !ImplementsGeneric(type, request1)))
    {
      return;
    }

    foreach (AttributeData attribute in type.GetAttributes())
    {
      if (!HandlerTypeNameReader.IsGeneratedHandler(attribute.AttributeClass, generatedHandlerAttribute))
      {
        continue;
      }

      Location location = GetAttributeLocation(compilation, attribute);
      declaredCovered.Add(type);
      requests.Add(type);
      if (!HandlerTypeNameReader.TryRead(attribute, compilation, out string relative, out string failureDetail))
      {
        if (reportUnreadable)
        {
          declaredDiagnostics.Add(DeclaredHandlerDiagnostic(location, type, failureDetail));
        }

        continue;
      }

      if (!HandlerTypeNameReader.TryResolve(type, relative, out string metadataName, out string displayName))
      {
        if (reportUnreadable)
        {
          declaredDiagnostics.Add(DeclaredHandlerDiagnostic(location, type, HandlerTypeNameReader.NotATypeName(relative)));
        }

        continue;
      }

      AddName(declaredNames, type, displayName);
      INamedTypeSymbol? handler = compilation.GetTypeByMetadataName(metadataName);
      declaredHandlers.Add(new DeclaredHandler(type, relative, displayName, handler, location));
      if (handler is null)
      {
        if (membership.IsHost)
        {
          declaredDiagnostics.Add(DeclaredHandlerDiagnostic(location, type, displayName));
        }

        continue;
      }

      if (!access.Allows(handler) || !access.Allows(type))
      {
        droppedMessages.Add(type);
        continue;
      }

      if (!handlersByRequest.TryGetValue(type, out List<INamedTypeSymbol>? list))
      {
        list = new List<INamedTypeSymbol>();
        handlersByRequest[type] = list;
      }

      bool same = ContainsSymbol(list, handler);

      if (same)
      {
        continue;
      }

      if (!responseByRequest.ContainsKey(type)
          && TryReadResponse(type, request0, request1, unitType, out ITypeSymbol response, out bool isVoid))
      {
        responseByRequest[type] = response;
        if (isVoid)
        {
          voidRequests.Add(type);
        }
      }

      if (responseByRequest.ContainsKey(type))
      {
        list.Add(handler);
      }
    }
  }

  private static bool ContainsSymbol(List<INamedTypeSymbol> list, INamedTypeSymbol candidate)
  {
    foreach (INamedTypeSymbol existing in list)
    {
      if (SymbolEqualityComparer.Default.Equals(existing, candidate))
      {
        return true;
      }

      if (existing.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
          == candidate.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
      {
        return true;
      }
    }

    return false;
  }

  private static bool TryReadResponse(
      INamedTypeSymbol type,
      INamedTypeSymbol? request0,
      INamedTypeSymbol? request1,
      INamedTypeSymbol? unitType,
      out ITypeSymbol response,
      out bool isVoid)
  {
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (request1 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, request1)
          && iface.TypeArguments.Length == 1)
      {
        response = iface.TypeArguments[0];
        isVoid = false;
        return true;
      }
    }

    if (Implements(type, request0))
    {
      response = unitType ?? type;
      isVoid = true;
      return true;
    }

    response = type;
    isVoid = false;
    return false;
  }

  private static void AddName(Dictionary<INamedTypeSymbol, List<string>> names, INamedTypeSymbol request, string displayName)
  {
    if (!names.TryGetValue(request, out List<string>? list))
    {
      list = new List<string>();
      names.Add(request, list);
    }

    list.Add(displayName);
  }

  private static int DistinctCount(List<string> names)
  {
    HashSet<string> distinct = new(StringComparer.Ordinal);
    foreach (string name in names)
    {
      distinct.Add(name);
    }

    return distinct.Count;
  }

  private static Diagnostic DuplicateDeclared(Compilation compilation, INamedTypeSymbol request, List<string> names)
  {
    HashSet<string> distinct = new(names, StringComparer.Ordinal);
    List<string> ordered = new(distinct);
    ordered.Sort(System.StringComparer.Ordinal);
    return Diagnostic.Create(
        DiagnosticDescriptors.DuplicateHandler,
        GetSourceLocation(compilation, request),
        request.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        string.Join(", ", ordered));
  }

  private static Diagnostic DeclaredHandlerDiagnostic(Location location, INamedTypeSymbol request, string detail)
  {
    return Diagnostic.Create(
        DiagnosticDescriptors.DeclaredHandlerDoesNotExist,
        location,
        request.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        detail);
  }

  private static void ReportScopeMismatch(
      Compilation compilation,
      INamedTypeSymbol handlerType,
      INamedTypeSymbol? handlerScope,
      INamedTypeSymbol messageType,
      INamedTypeSymbol? messageScope,
      List<Diagnostic> diagnostics)
  {
    if (handlerScope is null
        || messageScope is null
        || SymbolEqualityComparer.Default.Equals(handlerScope, messageScope))
    {
      return;
    }

    diagnostics.Add(
        Diagnostic.Create(
            DiagnosticDescriptors.ScopeMismatch,
            GetSourceLocation(compilation, handlerType),
            handlerType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            handlerScope.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            messageType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            messageScope.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
  }

  // The emitted pipeline names this marker. An inaccessible marker is TWM007 and is left unbound.
  private static bool PipelineScopeAllowed(AccessGuard access, INamedTypeSymbol? scopeType)
  {
    return scopeType is null || access.Allows(scopeType);
  }

  private static void CollectRequest(
      Compilation compilation,
      INamedTypeSymbol type,
      INamedTypeSymbol? request0,
      INamedTypeSymbol? request1,
      HashSet<INamedTypeSymbol> requests)
  {
    if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || type.IsAbstract || type.Arity > 0)
    {
      return;
    }

    // An inaccessible request cannot appear in generated code; its handler reports TWM007.
    if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
    {
      return;
    }

    if (Implements(type, request0) || ImplementsGeneric(type, request1))
    {
      requests.Add(type);
    }
  }

  private static List<RequestHandlerMatch> GetRequestHandlers(
      INamedTypeSymbol type,
      INamedTypeSymbol? handler1,
      INamedTypeSymbol? handler2,
      INamedTypeSymbol? unitType,
      Membership membership)
  {
    List<RequestHandlerMatch> matches = new();
    if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
    {
      return matches;
    }

    HashSet<INamedTypeSymbol> bound = new(SymbolEqualityComparer.Default);
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (handler2 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler2)
          && iface.TypeArguments.Length == 2
          && iface.TypeArguments[0] is INamedTypeSymbol request2
          && iface.TypeArguments[1] is ITypeSymbol response2
          && !IsBoundByBaseHandler(type, iface, membership)
          && bound.Add(request2))
      {
        matches.Add(new RequestHandlerMatch(request2, response2, false));
      }
    }

    // A two-arity binding wins over IRequestHandler<T> for the same request on one class.
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (handler1 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
          && iface.TypeArguments.Length == 1
          && iface.TypeArguments[0] is INamedTypeSymbol request1
          && !IsBoundByBaseHandler(type, iface, membership)
          && bound.Add(request1))
      {
        matches.Add(new RequestHandlerMatch(request1, unitType ?? request1, true));
      }
    }

    return matches;
  }

  private static List<INamedTypeSymbol> GetNotificationHandlers(
      INamedTypeSymbol type,
      INamedTypeSymbol? handler1,
      Membership membership)
  {
    List<INamedTypeSymbol> notifications = new();
    if (handler1 is null || type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
    {
      return notifications;
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
          && iface.TypeArguments.Length == 1
          && iface.TypeArguments[0] is INamedTypeSymbol notification
          && !IsBoundByBaseHandler(type, iface, membership))
      {
        notifications.Add(notification);
      }
    }

    return notifications;
  }

  // True when a concrete base class that discovery already binds implements the same closed
  // interface: the derived class inherits that binding instead of introducing a second one.
  private static bool IsBoundByBaseHandler(INamedTypeSymbol type, INamedTypeSymbol iface, Membership membership)
  {
    for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (current.TypeKind == TypeKind.Class
          && !current.IsAbstract
          && current.Arity == 0
          && IsDiscoverableType(current)
          && membership.Includes(current)
          && current.AllInterfaces.Contains(iface, SymbolEqualityComparer.Default))
      {
        return true;
      }
    }

    return false;
  }

  private static void SortByDisplayName(List<INamedTypeSymbol> handlers)
  {
    handlers.Sort((left, right) => System.StringComparer.Ordinal.Compare(left.ToDisplayString(), right.ToDisplayString()));
  }

  private static ImmutableArray<BehaviorRegistration> DiscoverBehaviors(
      Compilation compilation,
      Membership membership,
      INamedTypeSymbol? pipelineBehavior2,
      AccessGuard access,
      List<Diagnostic> generatorDiagnostics)
  {
    List<BehaviorRegistration> list = new();
    int sourceIndex = 0;

    IEnumerable<IAssemblySymbol> assemblies = membership.MemberAssemblies
        .OrderBy(a => a.Name, System.StringComparer.Ordinal);
    foreach (IAssemblySymbol assembly in assemblies)
    {
      foreach (AttributeData attribute in assembly.GetAttributes())
      {
        if (attribute.AttributeClass?.ToDisplayString() != Membership.BehaviorAttributeMetadataName)
        {
          continue;
        }

        if (attribute.ConstructorArguments.Length == 0
            || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol behaviorType)
        {
          continue;
        }

        int order = 0;
        if (attribute.ConstructorArguments.Length > 1
            && attribute.ConstructorArguments[1].Value is int orderValue)
        {
          order = orderValue;
        }

        INamedTypeSymbol? scopeType = null;
        foreach (KeyValuePair<string, TypedConstant> named in attribute.NamedArguments)
        {
          if (named.Key == "Scope" && named.Value.Value is INamedTypeSymbol namedScope)
          {
            scopeType = namedScope;
          }
        }

        Location location = GetAttributeLocation(compilation, attribute);
        INamedTypeSymbol definition = behaviorType.OriginalDefinition;
        if (pipelineBehavior2 is not null && !ImplementsGeneric(behaviorType, pipelineBehavior2)
            && !ImplementsGeneric(definition, pipelineBehavior2))
        {
          generatorDiagnostics.Add(
              Diagnostic.Create(
                  DiagnosticDescriptors.InvalidBehaviorRegistration,
                  location,
                  definition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
          continue;
        }

        if (!access.Allows(definition))
        {
          continue;
        }

        list.Add(new BehaviorRegistration(behaviorType, order, sourceIndex, scopeType, location));
        sourceIndex++;
      }
    }

    return list
        .OrderBy(b => b.Order)
        .ThenBy(b => b.SourceIndex)
        .ToImmutableArray();
  }
}
