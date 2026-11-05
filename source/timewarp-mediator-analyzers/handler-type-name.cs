#region Purpose
// Reads HandlerTypeName from a GeneratedHandlerAttribute and resolves it against the request.
#endregion

#region Design
// The value is a constant. A constructor string, including a default parameter, wins, because
// AttributeData exposes it for both source and referenced assemblies. Otherwise the most derived
// HandlerTypeName getter must return a string literal. Source getters are read from syntax.
// A referenced assembly has no syntax here; its getter body is read from the metadata image
// already loaded for that reference. Roslyn's public metadata API does not return method IL, and
// an analyzer cannot open the file itself, so the body is reached through the loaded
// ModuleMetadata. A failure becomes TWM012 rather than a crash or a silent TWM001.
// Dots in the relative name are nested type segments. Metadata names use '+' between those
// segments. Display names use dots, which is what FullyQualifiedFormat prints.
#endregion

namespace TimeWarp.Mediator.Analyzers;

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

  internal static string Unreadable(INamedTypeSymbol? attributeClass)
  {
    string owner = attributeClass is null
        ? MetadataName
        : attributeClass.ToDisplayString();
    return owner + ".HandlerTypeName (not a constant string)";
  }

  internal static string NotATypeName(string relativeName)
  {
    return relativeName + " (not a type name relative to the containing type)";
  }

  internal static bool TryRead(
      AttributeData attribute,
      Compilation compilation,
      out string name,
      out string failureDetail)
  {
    name = string.Empty;
    INamedTypeSymbol? attributeClass = attribute.AttributeClass;
    failureDetail = Unreadable(attributeClass);

    foreach (KeyValuePair<string, TypedConstant> named in attribute.NamedArguments)
    {
      if (named.Key == "HandlerTypeName"
          && named.Value.Kind == TypedConstantKind.Primitive
          && named.Value.Value is string namedText
          && namedText.Length > 0)
      {
        name = namedText;
        return true;
      }
    }

    foreach (TypedConstant argument in attribute.ConstructorArguments)
    {
      if (argument.Kind == TypedConstantKind.Primitive
          && argument.Value is string text
          && text.Length > 0)
      {
        name = text;
        return true;
      }
    }

    IMethodSymbol? getter = FindGetter(attributeClass);
    if (getter is null)
    {
      return false;
    }

    string? literal = ReadSyntax(getter);
    if (literal is null)
    {
      literal = ReadMetadataBody(getter, compilation);
    }

    if (literal is null || literal.Length == 0)
    {
      return false;
    }

    name = literal;
    return true;
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

  private static string? ReadMetadataBody(IMethodSymbol getter, Compilation compilation)
  {
    try
    {
      IAssemblySymbol? assembly = getter.ContainingAssembly;
      if (assembly is null
          || compilation.GetMetadataReference(assembly) is not PortableExecutableReference reference)
      {
        return null;
      }

      ModuleMetadata? module = ModuleOf(reference.GetMetadata());
      if (module is null)
      {
        return null;
      }

      MetadataReader reader = module.GetMetadataReader();
      MethodDefinitionHandle handle = FindMethod(reader, getter);
      if (handle.IsNil)
      {
        return null;
      }

      MethodDefinition definition = reader.GetMethodDefinition(handle);
      if (definition.RelativeVirtualAddress == 0)
      {
        return null;
      }

      if (MethodBody(module, handle) is not MethodBodyBlock body)
      {
        return null;
      }

      return DecodeStringReturn(body.GetILContent(), reader);
    }
    catch (Exception exception) when (exception is InvalidOperationException
        or BadImageFormatException
        or ArgumentException
        or TargetInvocationException
        or MissingMethodException)
    {
      return null;
    }
  }

  private static ModuleMetadata? ModuleOf(Metadata? metadata)
  {
    if (metadata is ModuleMetadata module)
    {
      return module;
    }

    if (metadata is not AssemblyMetadata assembly)
    {
      return null;
    }

    ImmutableArray<ModuleMetadata> modules = assembly.GetModules();
    return modules.Length == 0 ? null : modules[0];
  }

  private static MethodDefinitionHandle FindMethod(MetadataReader reader, IMethodSymbol getter)
  {
    MethodDefinitionHandle found = default;
    int count = 0;
    foreach (TypeDefinitionHandle typeHandle in reader.TypeDefinitions)
    {
      TypeDefinition typeDefinition = reader.GetTypeDefinition(typeHandle);
      if (!TypeMatches(reader, typeDefinition, getter.ContainingType))
      {
        continue;
      }

      foreach (MethodDefinitionHandle methodHandle in typeDefinition.GetMethods())
      {
        MethodDefinition method = reader.GetMethodDefinition(methodHandle);
        if (reader.GetString(method.Name) == "get_HandlerTypeName")
        {
          found = methodHandle;
          count++;
        }
      }
    }

    return count == 1 ? found : default;
  }

  private static bool TypeMatches(MetadataReader reader, TypeDefinition typeDefinition, INamedTypeSymbol symbol)
  {
    if (reader.GetString(typeDefinition.Name) != symbol.MetadataName)
    {
      return false;
    }

    if (symbol.ContainingType is INamedTypeSymbol outer)
    {
      if (typeDefinition.GetDeclaringType().IsNil)
      {
        return false;
      }

      return TypeMatches(reader, reader.GetTypeDefinition(typeDefinition.GetDeclaringType()), outer);
    }

    if (!typeDefinition.GetDeclaringType().IsNil)
    {
      return false;
    }

    string namespaceName = symbol.ContainingNamespace is { IsGlobalNamespace: false } container
        ? container.ToDisplayString()
        : string.Empty;
    return reader.GetString(typeDefinition.Namespace) == namespaceName;
  }

  private static object? MethodBody(ModuleMetadata module, MethodDefinitionHandle handle)
  {
    object? peModule = module.GetType().GetProperty(
            "Module",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?.GetValue(module);
    if (peModule is null)
    {
      return null;
    }

    MethodInfo? getBody = peModule.GetType().GetMethod(
        "GetMethodBodyOrThrow",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        null,
        new[] { typeof(MethodDefinitionHandle) },
        null);
    if (getBody is null)
    {
      return null;
    }

    return getBody.Invoke(peModule, new object[] { handle });
  }

  private static string? DecodeStringReturn(ImmutableArray<byte> il, MetadataReader reader)
  {
    if (il.IsDefaultOrEmpty)
    {
      return null;
    }

    string? found = null;
    int index = 0;
    while (index < il.Length)
    {
      byte operation = il[index];
      index++;
      if (operation == 0x72)
      {
        if (index + 4 > il.Length)
        {
          return null;
        }

        int token = il[index]
            | (il[index + 1] << 8)
            | (il[index + 2] << 16)
            | (il[index + 3] << 24);
        index += 4;
        if ((token & 0xFF000000) != 0x70000000)
        {
          return null;
        }

        string text = reader.GetUserString(MetadataTokens.UserStringHandle(token & 0x00FFFFFF));
        if (found is not null && !string.Equals(found, text, StringComparison.Ordinal))
        {
          return null;
        }

        found = text;
        continue;
      }

      int operand = OperandSize(operation);
      if (operand < 0 || index + operand > il.Length)
      {
        return null;
      }

      index += operand;
    }

    return found;
  }

  // Sizes for the opcodes a string-literal getter emits. Anything else is not a constant.
  private static int OperandSize(byte operation)
  {
    return operation switch
    {
      0x00 or 0x06 or 0x07 or 0x08 or 0x09 or 0x0A or 0x0B or 0x0C or 0x0D or 0x2A => 0,
      0x11 or 0x13 or 0x2B or 0xDE => 1,
      _ => -1,
    };
  }
}
