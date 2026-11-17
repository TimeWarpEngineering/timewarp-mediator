#region Purpose
// TWM013 after every generator has run: each GeneratedRequestPatternAttribute expansion exists and matches.
#endregion

#region Design
// Analyzer-only. The generator emits a Send arm from each expansion without seeing the request or
// handler. This compilation sees every generator's output, so it is where drift shows. Checks, in
// order: the request exists, the handler exists, the request implements the declared interface
// (which fixes the response), and the handler implements IRequestHandler for that request and
// response. A template the generator could not read or expand is TWM013 from the generator, so
// this walk does not report it again. Only the host's own types are walked: a pattern matters only
// where the generator runs.
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal static class RequestPatternAnalysis
{
  internal static bool HostDeclaresPattern(Compilation compilation, CancellationToken cancellationToken)
  {
    INamedTypeSymbol? pattern = compilation.GetTypeByMetadataName(RequestPatternReader.MetadataName);
    if (pattern is null)
    {
      return false;
    }

    foreach (INamedTypeSymbol type in Membership.EnumerateTypes(compilation.Assembly.GlobalNamespace))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (RequestPatternReader.HasPattern(type, pattern))
      {
        return true;
      }
    }

    return false;
  }

  internal static List<Diagnostic> Check(Compilation compilation, CancellationToken cancellationToken)
  {
    List<Diagnostic> diagnostics = new();
    INamedTypeSymbol? pattern = compilation.GetTypeByMetadataName(RequestPatternReader.MetadataName);
    if (pattern is null)
    {
      return diagnostics;
    }

    INamedTypeSymbol? handler1 = compilation.GetTypeByMetadataName("TimeWarp.Mediator.IRequestHandler`1");
    INamedTypeSymbol? handler2 = compilation.GetTypeByMetadataName("TimeWarp.Mediator.IRequestHandler`2");
    List<RequestPatternExpansion> expansions = new();
    List<RequestPatternFailure> failures = new();
    foreach (INamedTypeSymbol type in Membership.EnumerateTypes(compilation.Assembly.GlobalNamespace))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (RequestPatternReader.HasPattern(type, pattern))
      {
        RequestPatternReader.Read(type, compilation, pattern, null, expansions, failures, cancellationToken);
      }
    }

    foreach (RequestPatternExpansion expansion in expansions)
    {
      cancellationToken.ThrowIfCancellationRequested();
      string host = expansion.Host.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
      INamedTypeSymbol? request = RequestPatternReader.ResolveDotted(compilation, expansion.RequestName);
      INamedTypeSymbol? handler = RequestPatternReader.ResolveDotted(compilation, expansion.HandlerName);
      if (request is null)
      {
        diagnostics.Add(Diagnostic.Create(
            DiagnosticDescriptors.RequestPatternRequestMissing,
            expansion.Location,
            host,
            expansion.RequestName,
            expansion.Contract.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
      }

      if (handler is null)
      {
        diagnostics.Add(Diagnostic.Create(
            DiagnosticDescriptors.RequestPatternHandlerMissing,
            expansion.Location,
            host,
            expansion.HandlerName,
            expansion.RequestName));
      }

      if (request is null)
      {
        continue;
      }

      if (!request.AllInterfaces.Contains(expansion.RequestInterface, SymbolEqualityComparer.Default))
      {
        diagnostics.Add(Diagnostic.Create(
            DiagnosticDescriptors.RequestPatternInterfaceMismatch,
            expansion.Location,
            host,
            expansion.RequestName,
            expansion.RequestInterface.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        continue;
      }

      if (handler is not null && !IsHandler(handler, request, expansion.Response, handler1, handler2, out string expected))
      {
        diagnostics.Add(Diagnostic.Create(
            DiagnosticDescriptors.RequestPatternNotAHandler,
            expansion.Location,
            host,
            expansion.HandlerName,
            expansion.RequestName,
            expected));
      }
    }

    return diagnostics;
  }

  private static bool IsHandler(
      INamedTypeSymbol handler,
      INamedTypeSymbol request,
      ITypeSymbol? response,
      INamedTypeSymbol? handler1,
      INamedTypeSymbol? handler2,
      out string expected)
  {
    string requestName = request.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    expected = response is null
        ? "IRequestHandler<" + requestName + ">"
        : "IRequestHandler<" + requestName + ", " + response.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ">";
    foreach (INamedTypeSymbol iface in handler.AllInterfaces)
    {
      if (response is not null
          && handler2 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler2)
          && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], request)
          && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[1], response))
      {
        return true;
      }

      if (response is null
          && handler1 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
          && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], request))
      {
        return true;
      }
    }

    return false;
  }
}
