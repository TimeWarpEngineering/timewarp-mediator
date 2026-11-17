#region Purpose
// Reads GeneratedRequestPatternAttribute expansions off one attributed type declaration.
#endregion

#region Design
// The predicate is any class, struct, or record declaration with an attribute list: the attributed
// type may have no base list, so the TypeFact predicate would miss it. A partial type is read once
// per declaration, and each declaration keeps only the attributes written on it.
// A request or handler that this compilation already contains is copied from its symbol. Otherwise
// it is a declared name: another generator emits it. The declared request carries the request
// interface and every interface it extends, so behavior constraints close the way they would on
// the emitted type. Its location is the attribute, so TWM002 and TWM007 point there.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class CandidateExtractor
{
  internal static bool IsPatternCandidate(SyntaxNode node, CancellationToken cancellationToken)
  {
    return node is TypeDeclarationSyntax { AttributeLists.Count: > 0 } declaration
        && declaration.Kind() is SyntaxKind.ClassDeclaration
            or SyntaxKind.StructDeclaration
            or SyntaxKind.RecordDeclaration
            or SyntaxKind.RecordStructDeclaration;
  }

  internal static RequestPatternFact? ExtractPatterns(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.Node is not TypeDeclarationSyntax declaration)
    {
      return null;
    }

    Compilation compilation = context.SemanticModel.Compilation;
    INamedTypeSymbol? patternAttribute = compilation.GetTypeByMetadataName(RequestPatternReader.MetadataName);
    if (patternAttribute is null
        || context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol host
        || !RequestPatternReader.HasPattern(host, patternAttribute))
    {
      return null;
    }

    List<RequestPatternExpansion> expansions = new();
    List<RequestPatternFailure> failures = new();
    RequestPatternReader.Read(
        host,
        compilation,
        patternAttribute,
        attribute => IsWrittenOn(attribute, declaration),
        expansions,
        failures,
        cancellationToken);
    if (expansions.Count == 0 && failures.Count == 0)
    {
      return null;
    }

    string assemblyName = compilation.Assembly.Name;
    INamedTypeSymbol? unit = compilation.GetTypeByMetadataName(UnitTypeName);
    List<DeclaredRequestFact> requests = new();
    foreach (RequestPatternExpansion expansion in expansions)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LocationInfo? location = LocationInfo.From(expansion.Location);
      INamedTypeSymbol? existingRequest = RequestPatternReader.ResolveDotted(compilation, expansion.RequestName);
      INamedTypeSymbol? existingHandler = RequestPatternReader.ResolveDotted(compilation, expansion.HandlerName);
      TypeModel request = existingRequest is null
          ? DeclareRequest(expansion, assemblyName, location)
          : TypeModels.Create(existingRequest, compilation);
      TypeModel handler = existingHandler is null
          ? TypeModels.Declare(expansion.HandlerName, assemblyName, location)
          : TypeModels.Create(existingHandler, compilation);
      ITypeSymbol? responseSymbol = expansion.Response ?? unit;
      if (responseSymbol is null)
      {
        continue;
      }

      INamedTypeSymbol? scope = RequestPatternReader.ResolveScope(compilation, expansion.RequestName);
      requests.Add(new DeclaredRequestFact(
          request,
          handler,
          TypeModels.Create(responseSymbol, compilation),
          expansion.Response is null,
          scope is null ? null : TypeModels.Create(scope, compilation)));
    }

    List<DiagnosticInfo> diagnostics = new();
    foreach (RequestPatternFailure failure in failures)
    {
      diagnostics.Add(DiagnosticInfo.From(failure.ToDiagnostic()));
    }

    return new RequestPatternFact(
        new EquatableArray<DeclaredRequestFact>(requests.ToImmutableArray()),
        new EquatableArray<DiagnosticInfo>(diagnostics.ToImmutableArray()));
  }

  private static bool IsWrittenOn(AttributeData attribute, TypeDeclarationSyntax declaration)
  {
    SyntaxReference? reference = attribute.ApplicationSyntaxReference;
    return reference is not null
        && reference.SyntaxTree == declaration.SyntaxTree
        && declaration.Span.Contains(reference.Span);
  }

  private static TypeModel DeclareRequest(RequestPatternExpansion expansion, string assemblyName, LocationInfo? location)
  {
    List<string> keys = new();
    List<string> definitionKeys = new();
    List<string> mediatorNames = new();
    List<INamedTypeSymbol> interfaces = new() { expansion.RequestInterface };
    interfaces.AddRange(expansion.RequestInterface.AllInterfaces);
    foreach (INamedTypeSymbol iface in interfaces)
    {
      keys.Add(TypeModels.KeyOf(iface));
      definitionKeys.Add(TypeModels.DefinitionKeyOf(iface));
      if (iface.ContainingNamespace?.ToDisplayString() == "TimeWarp.Mediator")
      {
        mediatorNames.Add(iface.OriginalDefinition.Name);
      }
    }

    return TypeModels.Declare(expansion.RequestName, assemblyName, location) with
    {
      InterfaceKeys = new EquatableArray<string>(keys.ToImmutableArray()),
      InterfaceDefinitionKeys = new EquatableArray<string>(definitionKeys.ToImmutableArray()),
      MediatorInterfaceNames = new EquatableArray<string>(mediatorNames.ToImmutableArray()),
    };
  }
}
