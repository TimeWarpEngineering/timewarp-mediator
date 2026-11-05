#region Purpose
// Reads HandlerTypeName from a GeneratedHandlerAttribute and resolves it against the request.
#endregion

#region Design
// The value is a constant. An attribute declared in another assembly is read from a constructor
// parameter named handlerTypeName, including a default value, because AttributeData carries that
// argument for a reference assembly. A reference assembly's getter body is not the constant, so
// the body is not a contract and is not read. In this compilation a getter that returns a string
// literal is still read from syntax. Any other string constructor parameter is ignored.
// A failure becomes TWM012 rather than a crash or a silent TWM001.
// Dots in the relative name are nested type segments. Metadata names use '+' between those
// segments. Display names use dots, which is what FullyQualifiedFormat prints.
#endregion

namespace TimeWarp.Mediator.Analyzers;

internal enum DeclaredHandlerNameProblem
{
  Unreadable,
  NotATypeName,
  Missing,
  NotAHandler,
}

internal readonly struct HandlerNameFailure
{
  internal HandlerNameFailure(DeclaredHandlerNameProblem problem, string detail)
  {
    Problem = problem;
    Detail = detail;
  }

  internal DeclaredHandlerNameProblem Problem { get; }

  internal string Detail { get; }

  internal static HandlerNameFailure Unreadable(INamedTypeSymbol? attributeClass)
  {
    string owner = attributeClass is null
        ? HandlerTypeNameReader.MetadataName
        : attributeClass.ToDisplayString();
    return new HandlerNameFailure(DeclaredHandlerNameProblem.Unreadable, owner);
  }
}

internal static class HandlerTypeNameReader
{
  internal const string MetadataName = "TimeWarp.Mediator.GeneratedHandlerAttribute";

  internal static bool IsGeneratedHandler(INamedTypeSymbol? attributeClass, INamedTypeSymbol generatedHandlerAttribute)
  {
    for (INamedTypeSymbol? current = attributeClass; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, generatedHandlerAttribute))
      {
        return true;
      }
    }

    return false;
  }

  internal static bool TryRead(
      AttributeData attribute,
      out string name,
      out HandlerNameFailure failure)
  {
    name = string.Empty;
    INamedTypeSymbol? attributeClass = attribute.AttributeClass;
    failure = HandlerNameFailure.Unreadable(attributeClass);

    if (TryReadConstructorArgument(attribute, out string? fromConstructor))
    {
      if (fromConstructor is null)
      {
        return false;
      }

      name = fromConstructor;
      return true;
    }

    IMethodSymbol? getter = FindGetter(attributeClass);
    if (getter is null)
    {
      return false;
    }

    string? literal = ReadSyntax(getter);
    if (literal is null || literal.Length == 0)
    {
      return false;
    }

    name = literal;
    return true;
  }

  internal static bool IsRequestHandler(
      INamedTypeSymbol handler,
      INamedTypeSymbol request,
      INamedTypeSymbol? handler1,
      INamedTypeSymbol? handler2,
      INamedTypeSymbol? request1,
      out string expectedInterface)
  {
    string requestName = request.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    ITypeSymbol? response = RequestResponse(request, request1);
    expectedInterface = response is null
        ? "IRequestHandler<" + requestName + ">"
        : "IRequestHandler<" + requestName + ", " + response.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ">";

    foreach (INamedTypeSymbol iface in handler.AllInterfaces)
    {
      if (response is not null
          && handler2 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler2)
          && iface.TypeArguments.Length == 2
          && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], request)
          && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[1], response))
      {
        return true;
      }

      if (response is null
          && handler1 is not null
          && SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1)
          && iface.TypeArguments.Length == 1
          && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], request))
      {
        return true;
      }
    }

    return false;
  }

  internal static bool TryResolve(
      INamedTypeSymbol request,
      string relativeName,
      out string metadataName,
      out string displayName)
  {
    metadataName = string.Empty;
    displayName = string.Empty;
    if (string.IsNullOrWhiteSpace(relativeName)
        || relativeName.Contains('+')
        || relativeName.Contains(':')
        || relativeName.Contains(' '))
    {
      return false;
    }

    string[] segments = relativeName.Split('.');
    if (segments.Length == 0)
    {
      return false;
    }

    foreach (string segment in segments)
    {
      if (!SyntaxFacts.IsValidIdentifier(segment))
      {
        return false;
      }
    }

    StringBuilder metadata = new();
    StringBuilder display = new();
    if (request.ContainingNamespace is { IsGlobalNamespace: false } containerNamespace)
    {
      string namespaceName = containerNamespace.ToDisplayString();
      metadata.Append(namespaceName);
      display.Append(namespaceName);
    }

    bool nested = false;
    List<string> containers = new();
    for (INamedTypeSymbol? current = request.ContainingType; current is not null; current = current.ContainingType)
    {
      containers.Add(current.Name);
    }

    containers.Reverse();
    foreach (string container in containers)
    {
      AppendTypeName(metadata, display, container, ref nested);
    }

    foreach (string segment in segments)
    {
      AppendTypeName(metadata, display, segment, ref nested);
    }

    metadataName = metadata.ToString();
    displayName = display.ToString();
    return metadataName.Length > 0;
  }

  private static void AppendTypeName(StringBuilder metadata, StringBuilder display, string typeName, ref bool nested)
  {
    if (metadata.Length > 0)
    {
      metadata.Append(nested ? '+' : '.');
      display.Append('.');
    }

    metadata.Append(typeName);
    display.Append(typeName);
    nested = true;
  }

  private static IMethodSymbol? FindGetter(INamedTypeSymbol? attributeClass)
  {
    for (INamedTypeSymbol? current = attributeClass; current is not null; current = current.BaseType)
    {
      foreach (ISymbol member in current.GetMembers("HandlerTypeName"))
      {
        if (member is IPropertySymbol property
            && property.GetMethod is IMethodSymbol getter
            && !getter.IsAbstract)
        {
          return getter;
        }
      }
    }

    return null;
  }

  private static string? ReadSyntax(IMethodSymbol getter)
  {
    foreach (SyntaxReference reference in getter.DeclaringSyntaxReferences)
    {
      string? literal = LiteralFrom(reference.GetSyntax());
      if (literal is not null)
      {
        return literal;
      }
    }

    return null;
  }

  private static string? LiteralFrom(SyntaxNode node)
  {
    ExpressionSyntax? expression = node switch
    {
      PropertyDeclarationSyntax { ExpressionBody: { } body } => body.Expression,
      AccessorDeclarationSyntax { ExpressionBody: { } body } => body.Expression,
      AccessorDeclarationSyntax { Body: { } block } => SingleReturn(block),
      _ => null,
    };

    if (expression is null && node.Parent is not null)
    {
      expression = node.Parent switch
      {
        PropertyDeclarationSyntax { ExpressionBody: { } body } => body.Expression,
        AccessorDeclarationSyntax { ExpressionBody: { } body } => body.Expression,
        _ => null,
      };
    }

    return StringLiteral(expression);
  }

  private static ExpressionSyntax? SingleReturn(BlockSyntax block)
  {
    ReturnStatementSyntax? found = null;
    foreach (StatementSyntax statement in block.Statements)
    {
      if (statement is not ReturnStatementSyntax returned || returned.Expression is null)
      {
        return null;
      }

      if (found is not null)
      {
        return null;
      }

      found = returned;
    }

    return found?.Expression;
  }

  private static string? StringLiteral(ExpressionSyntax? expression)
  {
    while (expression is ParenthesizedExpressionSyntax parenthesized)
    {
      expression = parenthesized.Expression;
    }

    return expression is LiteralExpressionSyntax { Token.Value: string text } ? text : null;
  }

  // A true result with a null name means handlerTypeName is present and is not a non-empty string.
  // A false result means the constructor has no parameter by that name.
  private static bool TryReadConstructorArgument(AttributeData attribute, out string? name)
  {
    name = null;
    IMethodSymbol? constructor = attribute.AttributeConstructor;
    if (constructor is null)
    {
      return false;
    }

    ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
    ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;
    int count = Math.Min(parameters.Length, arguments.Length);
    for (int index = 0; index < count; index++)
    {
      if (!string.Equals(parameters[index].Name, "handlerTypeName", StringComparison.Ordinal))
      {
        continue;
      }

      TypedConstant argument = arguments[index];
      if (argument.Kind == TypedConstantKind.Primitive
          && argument.Value is string text
          && text.Length > 0)
      {
        name = text;
        return true;
      }

      return true;
    }

    return false;
  }

  private static ITypeSymbol? RequestResponse(INamedTypeSymbol request, INamedTypeSymbol? request1)
  {
    if (request1 is null)
    {
      return null;
    }

    foreach (INamedTypeSymbol iface in request.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, request1)
          && iface.TypeArguments.Length == 1)
      {
        return iface.TypeArguments[0];
      }
    }

    return null;
  }

}
