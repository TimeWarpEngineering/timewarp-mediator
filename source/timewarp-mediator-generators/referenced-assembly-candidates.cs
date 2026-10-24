#region Purpose
// Reads referenced member assemblies from metadata references, not from the host Compilation.
#endregion

#region Design
// MetadataReferencesProvider is cached when the host edits a file and leaves references alone.
// The host assembly name comes from the compiler-visible TimeWarpMediatorAssemblyName property,
// also cached. Accessibility is IsSymbolAccessibleWithin against a scratch compilation of that
// name, so InternalsVisibleTo matches the host the same way the real compilation does.
// Only assemblies with [assembly: MediatorAssembly] are walked. The host assembly is not one of
// the references; its declarations come from the syntax providers.
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

  internal static ReferencedInput Read(
      ImmutableArray<MetadataReference> references,
      MembershipOptions options,
      CancellationToken cancellationToken)
  {
    CSharpCompilation scratch = CSharpCompilation.Create(
        options.HostAssemblyName.Length == 0 ? "TimeWarpMediatorMetadata" : options.HostAssemblyName,
        references: references,
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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
    foreach (MetadataReference reference in references)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (scratch.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
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
        new EquatableArray<string>(missing.ToImmutableArray()));
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
