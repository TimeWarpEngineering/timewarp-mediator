#region Purpose
// Reads GeneratedHandlerAttribute facts off a concrete request.
#endregion

#region Design
// The name is a constant relative to the request. A missing type is still a fact; the analyzer reports it after every generator has run.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static partial class CandidateExtractor
{
  private static void ReadDeclaredHandlers(
      INamedTypeSymbol type,
      Compilation compilation,
      out EquatableArray<DeclaredHandlerFact> handlers,
      out EquatableArray<DeclaredHandlerFailure> failures)
  {
    List<DeclaredHandlerFact> found = new();
    List<DeclaredHandlerFailure> broken = new();
    INamedTypeSymbol? attributeType = compilation.GetTypeByMetadataName(HandlerTypeNameReader.MetadataName);
    if (attributeType is null || !ReadRequestShape(type, compilation, out TypeModel response, out bool isVoid))
    {
      handlers = EquatableArray<DeclaredHandlerFact>.Empty;
      failures = EquatableArray<DeclaredHandlerFailure>.Empty;
      return;
    }

    foreach (AttributeData attribute in type.GetAttributes())
    {
      if (!HandlerTypeNameReader.IsGeneratedHandler(attribute.AttributeClass, attributeType))
      {
        continue;
      }

      LocationInfo? location = null;
      if (attribute.ApplicationSyntaxReference is not null)
      {
        location = LocationInfo.From(attribute.ApplicationSyntaxReference.GetSyntax().GetLocation());
      }

      if (!HandlerTypeNameReader.TryRead(attribute, out string relative, out HandlerNameFailure failure))
      {
        broken.Add(new DeclaredHandlerFailure(failure.Problem, failure.Detail, location));
        continue;
      }

      if (!HandlerTypeNameReader.TryResolve(type, relative, out string metadataName, out string displayName))
      {
        broken.Add(new DeclaredHandlerFailure(DeclaredHandlerNameProblem.NotATypeName, relative, location));
        continue;
      }

      INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(metadataName);
      string assemblyName = type.ContainingAssembly?.Name ?? string.Empty;
      TypeModel handler = symbol is null
          ? TypeModels.Declare(displayName, assemblyName, location)
          : TypeModels.Create(symbol, compilation);
      found.Add(new DeclaredHandlerFact(handler, response, isVoid));
    }

    handlers = new EquatableArray<DeclaredHandlerFact>(found.ToImmutableArray());
    failures = new EquatableArray<DeclaredHandlerFailure>(broken.ToImmutableArray());
  }

  private static bool ReadRequestShape(
      INamedTypeSymbol type,
      Compilation compilation,
      out TypeModel response,
      out bool isVoid)
  {
    INamedTypeSymbol? unit = compilation.GetTypeByMetadataName(UnitTypeName);
    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == Request1
          && iface.TypeArguments.Length == 1)
      {
        response = TypeModels.Create(iface.TypeArguments[0], compilation);
        isVoid = false;
        return true;
      }
    }

    foreach (INamedTypeSymbol iface in type.AllInterfaces)
    {
      if (iface.OriginalDefinition.GetDocumentationCommentId() == Request0)
      {
        response = unit is null
            ? TypeModels.Create(type, compilation)
            : TypeModels.Create(unit, compilation);
        isVoid = true;
        return true;
      }
    }

    response = TypeModels.Create(type, compilation);
    isVoid = false;
    return false;
  }

}
