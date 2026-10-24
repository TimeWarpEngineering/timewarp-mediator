#region Purpose
// Reads referenced member assemblies from metadata references, not from the host Compilation.
#endregion

#region Design
// MetadataReferencesProvider is cached when the host edits a file and leaves references alone.
// HostIdentity is the compilation name and public key, an equal value on an unrelated edit, so
// this step stays cached. The scratch compilation uses that name and WithCryptoPublicKey, so a
// strong-named InternalsVisibleTo grant matches the host.
// Duplicate metadata references of one assembly are skipped. Only assemblies with
// [assembly: MediatorAssembly] are walked. The host assembly is not one of the references.
// TWM008 checks these references, then the model step also accepts a host declaration of the
// same metadata name, so a required type written in the host is not reported missing.
#endregion

namespace TimeWarp.Mediator.Generators;

internal static class ReferencedAssemblyCandidates
{
  private static readonly string[] RequiredTypes =
  {
    "TimeWarp.Mediator.IMediator",
    "Microsoft.Extensions.DependencyInjection.IServiceCollection",
    "Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions",
    "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions",
  };

  internal static bool IsRequiredRuntime(string displayName)
  {
    foreach (string metadataName in RequiredTypes)
    {
      if (metadataName == displayName)
      {
        return true;
      }
    }

    return false;
  }

  internal static ReferencedInput Read(
      ImmutableArray<MetadataReference> references,
      HostIdentity identity,
      CancellationToken cancellationToken)
  {
    string assemblyName = identity.AssemblyName.Length == 0 ? "TimeWarpMediatorMetadata" : identity.AssemblyName;
    CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary);
    ImmutableArray<byte> publicKey = CopyKey(identity.PublicKey);
    if (publicKey.Length > 0)
    {
      options = options.WithCryptoPublicKey(publicKey);
    }

    CSharpCompilation scratch = CSharpCompilation.Create(
        assemblyName,
        references: references,
        options: options);

    List<string> missing = new();
    foreach (string metadataName in RequiredTypes)
    {
      if (scratch.GetTypeByMetadataName(metadataName) is null)
      {
        missing.Add(metadataName);
      }
    }

    List<TypeFact> types = new();
    List<BehaviorFact> behaviors = new();
    List<string> members = new();
    HashSet<IAssemblySymbol> seen = new(SymbolEqualityComparer.Default);
    foreach (MetadataReference reference in references)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (scratch.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
      {
        continue;
      }

      if (!seen.Add(assembly))
      {
        continue;
      }

      if (!HasMediatorAssembly(assembly))
      {
        continue;
      }

      members.Add(assembly.Name);
      ReadAssembly(assembly, scratch, types, behaviors, cancellationToken);
    }

    members.Sort(StringComparer.Ordinal);
    return new ReferencedInput(
        new EquatableArray<TypeFact>(types.ToImmutableArray()),
        new EquatableArray<BehaviorFact>(behaviors.ToImmutableArray()),
        new EquatableArray<string>(members.ToImmutableArray()),
        new EquatableArray<string>(missing.ToImmutableArray()),
        identity.AssemblyName);
  }

  private static ImmutableArray<byte> CopyKey(EquatableArray<byte> key)
  {
    if (key.Length == 0)
    {
      return ImmutableArray<byte>.Empty;
    }

    byte[] bytes = new byte[key.Length];
    int index = 0;
    foreach (byte item in key)
    {
      bytes[index] = item;
      index++;
    }

    return ImmutableArray.Create(bytes);
  }

  private static void ReadAssembly(
      IAssemblySymbol assembly,
      Compilation accessCompilation,
      List<TypeFact> types,
      List<BehaviorFact> behaviors,
      CancellationToken cancellationToken)
  {
    foreach (INamedTypeSymbol type in Membership.EnumerateTypes(assembly.GlobalNamespace))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (!TypeModels.IsDiscoverable(type))
      {
        continue;
      }

      TypeFact? fact = CandidateExtractor.ReadType(type, accessCompilation);
      if (fact is not null)
      {
        types.Add(fact);
      }
    }

    int index = 0;
    foreach (AttributeData attribute in assembly.GetAttributes())
    {
      if (attribute.AttributeClass?.ToDisplayString() != Membership.BehaviorAttributeMetadataName)
      {
        index++;
        continue;
      }

      BehaviorFact? behavior = CandidateExtractor.ReadBehavior(
          attribute,
          accessCompilation,
          assembly.Name,
          string.Empty,
          index);
      if (behavior is not null)
      {
        behaviors.Add(behavior);
      }

      index++;
    }
  }

  private static bool HasMediatorAssembly(IAssemblySymbol assembly)
  {
    foreach (AttributeData attribute in assembly.GetAttributes())
    {
      if (attribute.AttributeClass?.ToDisplayString() == Membership.AssemblyAttributeMetadataName)
      {
        return true;
      }
    }

    return false;
  }
}
