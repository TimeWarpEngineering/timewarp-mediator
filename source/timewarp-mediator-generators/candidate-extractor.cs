#region Purpose
// Copies one host declaration or one MediatorBehavior attribute into equatable facts.
#endregion

#region Design
// CreateSyntaxProvider, not ForAttributeWithMetadataName. The attribute API combines the full
// Compilation into its transform, so an unrelated edit re-runs it and the model step is Unchanged
// instead of Cached. The predicate ignores declarations with no base list and attributes whose
// name is not MediatorBehavior, so an unrelated file adds no candidate.
// Inherited handler bindings are left in the fact. The model step drops them once every member
// assembly's facts are visible, including a base handler that lives in a referenced assembly.
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
    if (node is not AttributeSyntax attribute)
    {
      return false;
    }

    return attribute.Name.ToString().Contains("MediatorBehavior");
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
    HashSet<string> bound = new(StringComparer.Ordinal);
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == RequestHandler2
          && iface.TypeArguments.Length == 2
          && iface.TypeArguments[0] is INamedTypeSymbol request
          && iface.TypeArguments[1] is ITypeSymbol response
          && bound.Add(TypeModels.KeyOf(request)))
      {
        matches.Add(new HandlerMatch(
            TypeModels.Create(request, compilation),
            TypeModels.Create(response, compilation),
            false,
            false));
      }
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == RequestHandler1
          && iface.TypeArguments.Length == 1
          && iface.TypeArguments[0] is INamedTypeSymbol request
          && bound.Add(TypeModels.KeyOf(request)))
      {
        TypeModel responseModel = unit is null
            ? TypeModels.Create(request, compilation)
            : TypeModels.Create(unit, compilation);
        matches.Add(new HandlerMatch(TypeModels.Create(request, compilation), responseModel, true, false));
      }
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == NotificationHandler1
          && iface.TypeArguments.Length == 1
          && iface.TypeArguments[0] is INamedTypeSymbol notification)
      {
        matches.Add(new HandlerMatch(TypeModels.Create(notification, compilation), null, false, true));
      }
    }

    return new EquatableArray<HandlerMatch>(matches.ToImmutableArray());
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
      foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
      {
        if (constraint is ITypeParameterSymbol)
        {
          continue;
        }

        constraints.Add(new ConstraintSlot(TypeModels.KeyOf(constraint), TypeModels.DefinitionKeyOf(constraint)));
      }

      parameters.Add(new ParameterSlot(
          parameter.HasReferenceTypeConstraint,
          parameter.HasValueTypeConstraint,
          parameter.HasUnmanagedTypeConstraint,
          parameter.HasConstructorConstraint,
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
      return new TypeArgSlot(parameter.Ordinal, null);
    }

    return new TypeArgSlot(-1, TypeModels.KeyOf(argument));
  }

  private static string Prefix(string display)
  {
    int angle = display.LastIndexOf('<');
    return angle < 0 ? display : display.Substring(0, angle);
  }
}
