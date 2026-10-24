#region Purpose
// Copies one host declaration or one MediatorBehavior attribute into equatable facts.
#endregion

#region Design
// CreateSyntaxProvider, not ForAttributeWithMetadataName. The attribute API combines the full
// Compilation into its transform, so an unrelated edit re-runs it and the model step is Unchanged
// rather than Cached. The type predicate ignores declarations with no base list.
// The behavior predicate is every assembly-targeted attribute. ExtractBehavior keeps a fact only
// when the symbol is MediatorBehavior, so a using-alias is kept.
// Inherited handler bindings stay in the fact. CandidateGraph drops them once every member
// assembly is visible, then dedupes one binding per request.
// A type-parameter constraint is unsatisfiable: the symbol walk compares the concrete argument
// to that type parameter and rejects the close.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static class CandidateExtractor
{
  private const string RequestHandler1 = "T:TimeWarp.Mediator.IRequestHandler`1";
  private const string RequestHandler2 = "T:TimeWarp.Mediator.IRequestHandler`2";
  private const string NotificationHandler1 = "T:TimeWarp.Mediator.INotificationHandler`1";
  private const string Request1 = "T:TimeWarp.Mediator.IRequest`1";
  private const string Request0 = "T:TimeWarp.Mediator.IRequest";
  private const string PipelineBehavior2 = "T:TimeWarp.Mediator.IPipelineBehavior`2";
  private const string UnitTypeName = "TimeWarp.Mediator.Unit";

  internal static bool IsTypeCandidate(SyntaxNode node, CancellationToken cancellationToken)
  {
    return node is TypeDeclarationSyntax { BaseList: not null };
  }

  internal static bool IsBehaviorAttribute(SyntaxNode node, CancellationToken cancellationToken)
  {
    if (node is not AttributeSyntax attribute || attribute.Parent is not AttributeListSyntax list)
    {
      return false;
    }

    AttributeTargetSpecifierSyntax? target = list.Target;
    return target is not null && target.Identifier.IsKind(SyntaxKind.AssemblyKeyword);
  }

  internal static bool IsRuntimeDeclaration(SyntaxNode node, CancellationToken cancellationToken)
  {
    if (node is not BaseTypeDeclarationSyntax declaration)
    {
      return false;
    }

    string name = declaration.Identifier.ValueText;
    return name is "IMediator"
        or "IServiceCollection"
        or "ServiceCollectionServiceExtensions"
        or "ServiceProviderServiceExtensions";
  }

  internal static string? ExtractRuntimeName(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.Node is not BaseTypeDeclarationSyntax declaration)
    {
      return null;
    }

    if (context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
    {
      return null;
    }

    string display = type.ToDisplayString();
    return ReferencedAssemblyCandidates.IsRequiredRuntime(display) ? display : null;
  }

  internal static TypeFact? ExtractType(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.Node is not TypeDeclarationSyntax declaration)
    {
      return null;
    }

    if (context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
    {
      return null;
    }

    if (!TypeModels.IsDiscoverable(type))
    {
      return null;
    }

    return ReadType(type, context.SemanticModel.Compilation);
  }

  internal static BehaviorFact? ExtractBehavior(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.Node is not AttributeSyntax attributeSyntax)
    {
      return null;
    }

    Compilation compilation = context.SemanticModel.Compilation;
    int index = 0;
    foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) != attributeSyntax)
      {
        index++;
        continue;
      }

      if (attribute.AttributeClass?.ToDisplayString() != Membership.BehaviorAttributeMetadataName)
      {
        return null;
      }

      // Index in Assembly.GetAttributes, which is the source order the symbol walk uses.
      return ReadBehavior(
          attribute,
          compilation,
          compilation.Assembly.Name,
          string.Empty,
          index);
    }

    return null;
  }

  internal static BehaviorFact? ReadBehavior(
      AttributeData attribute,
      Compilation accessCompilation,
      string assemblyName,
      string path,
      int spanStart)
  {
    if (attribute.ConstructorArguments.Length == 0
        || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol behaviorType)
    {
      return null;
    }

    int order = 0;
    if (attribute.ConstructorArguments.Length > 1
        && attribute.ConstructorArguments[1].Value is int orderValue)
    {
      order = orderValue;
    }

    INamedTypeSymbol? scopeSymbol = null;
    foreach (KeyValuePair<string, TypedConstant> named in attribute.NamedArguments)
    {
      if (named.Key == "Scope" && named.Value.Value is INamedTypeSymbol namedScope)
      {
        scopeSymbol = namedScope;
      }
    }

    INamedTypeSymbol definition = behaviorType.OriginalDefinition;
    TypeModel definitionModel = TypeModels.Create(definition, accessCompilation);
    string fullyQualified = definition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    string display = definition.ToDisplayString();
    string errorDisplay = definition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    LocationInfo? location = null;
    if (attribute.ApplicationSyntaxReference is not null)
    {
      location = LocationInfo.From(attribute.ApplicationSyntaxReference.GetSyntax().GetLocation());
    }

    return new BehaviorFact(
        assemblyName,
        path,
        spanStart,
        order,
        definitionModel,
        Prefix(fullyQualified),
        Prefix(display),
        Prefix(errorDisplay),
        ReadParameters(definition),
        ReadPipelines(definition),
        scopeSymbol is null ? null : TypeModels.Create(scopeSymbol, accessCompilation),
        location);
  }

  internal static TypeFact? ReadType(INamedTypeSymbol type, Compilation accessCompilation)
  {
    if (!TypeModels.IsDiscoverable(type))
    {
      return null;
    }

    TypeModel model = TypeModels.Create(type, accessCompilation);
    EquatableArray<HandlerMatch> matches = ReadMatches(type, accessCompilation);
    bool isConcreteRequest = IsConcreteRequest(type, model, accessCompilation);
    if (matches.Length == 0 && !isConcreteRequest)
    {
      return null;
    }

    INamedTypeSymbol? scope = MessageGraphBuilder.ResolveScope(type);
    return new TypeFact(
        model,
        matches,
        isConcreteRequest,
        scope is null ? null : TypeModels.Create(scope, accessCompilation));
  }

  private static EquatableArray<HandlerMatch> ReadMatches(INamedTypeSymbol type, Compilation compilation)
  {
    List<HandlerMatch> matches = new();
    if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.Arity > 0)
    {
      return new EquatableArray<HandlerMatch>(matches.ToImmutableArray());
    }

    INamedTypeSymbol? unit = compilation.GetTypeByMetadataName(UnitTypeName);
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == RequestHandler2
          && iface.TypeArguments.Length == 2
          && iface.TypeArguments[0] is INamedTypeSymbol request
          && iface.TypeArguments[1] is ITypeSymbol response)
      {
        matches.Add(new HandlerMatch(
            TypeModels.Create(request, compilation),
            TypeModels.Create(response, compilation),
            false,
            false,
            ScopeModel(request, compilation)));
      }
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == RequestHandler1
          && iface.TypeArguments.Length == 1
          && iface.TypeArguments[0] is INamedTypeSymbol request)
      {
        TypeModel responseModel = unit is null
            ? TypeModels.Create(request, compilation)
            : TypeModels.Create(unit, compilation);
        matches.Add(new HandlerMatch(
            TypeModels.Create(request, compilation),
            responseModel,
            true,
            false,
            ScopeModel(request, compilation)));
      }
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == NotificationHandler1
          && iface.TypeArguments.Length == 1
          && iface.TypeArguments[0] is INamedTypeSymbol notification)
      {
        matches.Add(new HandlerMatch(
            TypeModels.Create(notification, compilation),
            null,
            false,
            true,
            ScopeModel(notification, compilation)));
      }
    }

    return new EquatableArray<HandlerMatch>(matches.ToImmutableArray());
  }

  private static TypeModel? ScopeModel(INamedTypeSymbol message, Compilation compilation)
  {
    INamedTypeSymbol? scope = MessageGraphBuilder.ResolveScope(message);
    return scope is null ? null : TypeModels.Create(scope, compilation);
  }

  private static bool IsConcreteRequest(INamedTypeSymbol type, TypeModel model, Compilation compilation)
  {
    if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || type.IsAbstract || type.Arity > 0)
    {
      return false;
    }

    if (!model.Accessible || !model.IsDiscoverable)
    {
      return false;
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      string? id = iface.OriginalDefinition.GetDocumentationCommentId();
      if (id is Request0 or Request1)
      {
        return compilation.IsSymbolAccessibleWithin(type, compilation.Assembly);
      }
    }

    return false;
  }

  private static EquatableArray<ParameterSlot> ReadParameters(INamedTypeSymbol definition)
  {
    List<ParameterSlot> parameters = new();
    foreach (ITypeParameterSymbol parameter in definition.TypeParameters)
    {
      List<ConstraintSlot> constraints = new();
      bool hasTypeParameterConstraint = false;
      foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
      {
        if (constraint is ITypeParameterSymbol)
        {
          // where TRes : TReq. A concrete argument is not that type parameter, so the slot cannot close.
          hasTypeParameterConstraint = true;
          continue;
        }

        constraints.Add(new ConstraintSlot(TypeModels.KeyOf(constraint), TypeModels.DefinitionKeyOf(constraint)));
      }

      parameters.Add(new ParameterSlot(
          parameter.HasReferenceTypeConstraint,
          parameter.HasValueTypeConstraint,
          parameter.HasUnmanagedTypeConstraint,
          parameter.HasConstructorConstraint,
          hasTypeParameterConstraint,
          new EquatableArray<ConstraintSlot>(constraints.ToImmutableArray())));
    }

    return new EquatableArray<ParameterSlot>(parameters.ToImmutableArray());
  }

  private static EquatableArray<PipelineSlot> ReadPipelines(INamedTypeSymbol definition)
  {
    List<PipelineSlot> pipelines = new();
    foreach (INamedTypeSymbol iface in definition.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() != PipelineBehavior2
          || iface.TypeArguments.Length != 2)
      {
        continue;
      }

      pipelines.Add(new PipelineSlot(Slot(iface.TypeArguments[0], definition), Slot(iface.TypeArguments[1], definition)));
    }

    return new EquatableArray<PipelineSlot>(pipelines.ToImmutableArray());
  }

  private static TypeArgSlot Slot(ITypeSymbol argument, INamedTypeSymbol definition)
  {
    if (argument is ITypeParameterSymbol parameter
        && SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, definition))
    {
      return new TypeArgSlot(parameter.Ordinal, null, null, 0, EquatableArray<TypeArgSlot>.Empty);
    }

    if (argument is IArrayTypeSymbol array)
    {
      return new TypeArgSlot(
          -1,
          null,
          null,
          array.Rank,
          new EquatableArray<TypeArgSlot>(ImmutableArray.Create(Slot(array.ElementType, definition))));
    }

    if (argument is INamedTypeSymbol named && named.TypeArguments.Length > 0 && ContainsBehaviorParameter(named, definition))
    {
      List<TypeArgSlot> arguments = new();
      foreach (ITypeSymbol typeArgument in named.TypeArguments)
      {
        arguments.Add(Slot(typeArgument, definition));
      }

      return new TypeArgSlot(
          -1,
          null,
          TypeModels.DefinitionKeyOf(named),
          0,
          new EquatableArray<TypeArgSlot>(arguments.ToImmutableArray()));
    }

    return new TypeArgSlot(-1, TypeModels.KeyOf(argument), null, 0, EquatableArray<TypeArgSlot>.Empty);
  }

  private static bool ContainsBehaviorParameter(ITypeSymbol type, INamedTypeSymbol definition)
  {
    if (type is ITypeParameterSymbol parameter)
    {
      return SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, definition);
    }

    if (type is IArrayTypeSymbol array)
    {
      return ContainsBehaviorParameter(array.ElementType, definition);
    }

    if (type is INamedTypeSymbol named)
    {
      foreach (ITypeSymbol argument in named.TypeArguments)
      {
        if (ContainsBehaviorParameter(argument, definition))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static string Prefix(string display)
  {
    int angle = display.LastIndexOf('<');
    return angle < 0 ? display : display.Substring(0, angle);
  }
}
