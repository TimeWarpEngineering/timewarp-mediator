#region Purpose
// Reads GeneratedRequestPatternAttribute applications and expands them once per contract.
#endregion

#region Design
// Shared by the generator (to emit Send arms) and the analyzer (to check the expansions after
// every generator has run). Both see the same expansion because both run this code.
// A template comes from a constructor parameter of the same name, including a default value, or
// from a const string field on the attribute class chain. Contracts come from a constructor
// parameter named contracts. AttributeData carries both for an attribute declared in a reference
// assembly; a getter body does not survive there, so properties are not read.
// Placeholders are {Host}, {Contract}, and {ContractType}. Anything else in braces fails.
// Request and handler names expand to dotted names. They are not resolved here: another generator
// emits them. The request interface is parsed as a type and resolved now, because its response
// decides the Send arm and its interfaces decide which behaviors close.
// Templates and contract reading: request-pattern.templates.cs. Type-name resolution:
// request-pattern.type-names.cs.
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal sealed class RequestPatternExpansion
{
  internal RequestPatternExpansion(
      INamedTypeSymbol host,
      INamedTypeSymbol contract,
      string requestName,
      string handlerName,
      INamedTypeSymbol requestInterface,
      ITypeSymbol? response,
      Location location)
  {
    Host = host;
    Contract = contract;
    RequestName = requestName;
    HandlerName = handlerName;
    RequestInterface = requestInterface;
    Response = response;
    Location = location;
  }

  internal INamedTypeSymbol Host { get; }

  internal INamedTypeSymbol Contract { get; }

  // Dotted display name without global::.
  internal string RequestName { get; }

  internal string HandlerName { get; }

  internal INamedTypeSymbol RequestInterface { get; }

  // Null for a void request (the interface is IRequest only).
  internal ITypeSymbol? Response { get; }

  internal Location Location { get; }
}

internal sealed class RequestPatternFailure
{
  internal RequestPatternFailure(DiagnosticDescriptor descriptor, Location location, object[] arguments)
  {
    Descriptor = descriptor;
    Location = location;
    Arguments = arguments;
  }

  internal DiagnosticDescriptor Descriptor { get; }

  internal Location Location { get; }

  internal object[] Arguments { get; }

  internal Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, Arguments);
}

internal static partial class RequestPatternReader
{
  internal const string MetadataName = "TimeWarp.Mediator.GeneratedRequestPatternAttribute";
  private const string Request0 = "TimeWarp.Mediator.IRequest";

  internal static bool IsPattern(INamedTypeSymbol? attributeClass, INamedTypeSymbol patternAttribute)
  {
    for (INamedTypeSymbol? current = attributeClass; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, patternAttribute))
      {
        return true;
      }
    }

    return false;
  }

  internal static bool HasPattern(INamedTypeSymbol type, INamedTypeSymbol patternAttribute)
  {
    foreach (AttributeData attribute in type.GetAttributes())
    {
      if (IsPattern(attribute.AttributeClass, patternAttribute))
      {
        return true;
      }
    }

    return false;
  }

  // Expands every pattern attribute on host that passes the filter (null takes all).
  internal static void Read(
      INamedTypeSymbol host,
      Compilation compilation,
      INamedTypeSymbol patternAttribute,
      Func<AttributeData, bool>? filter,
      List<RequestPatternExpansion> expansions,
      List<RequestPatternFailure> failures,
      CancellationToken cancellationToken)
  {
    foreach (AttributeData attribute in host.GetAttributes())
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!IsPattern(attribute.AttributeClass, patternAttribute) || (filter is not null && !filter(attribute)))
      {
        continue;
      }

      ReadOne(host, compilation, attribute, expansions, failures);
    }
  }

  private static void ReadOne(
      INamedTypeSymbol host,
      Compilation compilation,
      AttributeData attribute,
      List<RequestPatternExpansion> expansions,
      List<RequestPatternFailure> failures)
  {
    Location location = AttributeLocation(attribute, host);
    string hostDisplay = host.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    string attributeDisplay = attribute.AttributeClass?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
        ?? MetadataName;

    if (!TryReadTemplate(attribute, "requestTypeName", "RequestTypeName", out string requestTemplate))
    {
      failures.Add(Unreadable(location, hostDisplay, attributeDisplay, "requestTypeName", "RequestTypeName"));
      return;
    }

    if (!TryReadTemplate(attribute, "handlerTypeName", "HandlerTypeName", out string handlerTemplate))
    {
      failures.Add(Unreadable(location, hostDisplay, attributeDisplay, "handlerTypeName", "HandlerTypeName"));
      return;
    }

    if (!TryReadTemplate(attribute, "requestInterfaceName", "RequestInterfaceName", out string interfaceTemplate))
    {
      failures.Add(Unreadable(location, hostDisplay, attributeDisplay, "requestInterfaceName", "RequestInterfaceName"));
      return;
    }

    if (!TryReadContracts(attribute, out ImmutableArray<TypedConstant> contracts))
    {
      failures.Add(Unreadable(location, hostDisplay, attributeDisplay, "contracts", "contracts"));
      return;
    }

    string? hostName = DottedName(host);
    foreach (TypedConstant contractConstant in contracts)
    {
      if (contractConstant.Value is not INamedTypeSymbol contract
          || contract.TypeKind == TypeKind.Error
          || DottedName(contract) is not string contractName)
      {
        string shown = contractConstant.Value is ITypeSymbol shownType
            ? shownType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            : "null";
        failures.Add(Invalid(location, hostDisplay, attributeDisplay, "contract '" + shown + "' is not a non-generic named type"));
        continue;
      }

      string? problem = null;
      string handlerName = string.Empty;
      string interfaceName = string.Empty;
      if (!TryExpand(requestTemplate, hostName, contract.Name, contractName, out string requestName, out problem)
          || !TryExpand(handlerTemplate, hostName, contract.Name, contractName, out handlerName, out problem)
          || !TryExpand(interfaceTemplate, hostName, contract.Name, contractName, out interfaceName, out problem))
      {
        failures.Add(Invalid(location, hostDisplay, attributeDisplay, problem ?? "a template does not expand"));
        return;
      }

      if (!IsDottedTypeName(requestName))
      {
        failures.Add(Invalid(location, hostDisplay, attributeDisplay, "request name '" + requestName + "' is not a dotted type name"));
        continue;
      }

      if (!IsDottedTypeName(handlerName))
      {
        failures.Add(Invalid(location, hostDisplay, attributeDisplay, "handler name '" + handlerName + "' is not a dotted type name"));
        continue;
      }

      INamedTypeSymbol? requestInterface = ResolveTypeName(compilation, interfaceName) as INamedTypeSymbol;
      if (requestInterface is null || requestInterface.TypeKind != TypeKind.Interface)
      {
        failures.Add(Invalid(location, hostDisplay, attributeDisplay, "request interface '" + interfaceName + "' does not resolve to an interface"));
        continue;
      }

      if (!TryReadResponse(requestInterface, out ITypeSymbol? response))
      {
        failures.Add(Invalid(location, hostDisplay, attributeDisplay, "request interface '" + interfaceName + "' is not an IRequest or IRequest<T>"));
        continue;
      }

      expansions.Add(new RequestPatternExpansion(
          host,
          contract,
          StripGlobal(requestName),
          StripGlobal(handlerName),
          requestInterface,
          response,
          location));
    }
  }

  private static RequestPatternFailure Unreadable(Location location, string host, string attribute, string parameter, string field)
  {
    return new RequestPatternFailure(
        DiagnosticDescriptors.RequestPatternUnreadable,
        location,
        new object[] { host, attribute, parameter, field });
  }

  private static RequestPatternFailure Invalid(Location location, string host, string attribute, string detail)
  {
    return new RequestPatternFailure(
        DiagnosticDescriptors.RequestPatternInvalid,
        location,
        new object[] { host, attribute, detail });
  }

  private static Location AttributeLocation(AttributeData attribute, INamedTypeSymbol host)
  {
    SyntaxReference? reference = attribute.ApplicationSyntaxReference;
    if (reference is not null)
    {
      SyntaxNode node = reference.GetSyntax();
      if (node is AttributeSyntax syntax)
      {
        return syntax.Name.GetLocation();
      }

      return node.GetLocation();
    }

    foreach (Location location in host.Locations)
    {
      if (location.IsInSource)
      {
        return location;
      }
    }

    return Location.None;
  }
}
