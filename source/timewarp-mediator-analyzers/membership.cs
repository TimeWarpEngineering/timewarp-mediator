#region Purpose
// Resolves which assemblies participate in the compile-time MessageGraph.
#endregion

#region Design
// Current compilation is a member when it has [assembly: MediatorAssembly] or MSBuild
// TimeWarpMediatorAssembly=true (set by the generator package for hosts). Referenced assemblies
// join only with [assembly: MediatorAssembly]. No marker → not linked.
// Pipeline assignment is MediatorScopeAttribute (marker type).
// IsHost is the generator itself or TimeWarpMediatorAssembly=true; an attribute-only library is
// not a host, so whole-program checks (TWM004 on interface-typed or unscoped sends) stay off there.
// Build properties come only from GlobalOptions (MembershipOptions): CompilerVisibleProperty values
// are global, so scanning per-tree options cost a loop over every syntax tree for nothing.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public sealed class Membership
{
  public const string AssemblyAttributeMetadataName = "TimeWarp.Mediator.MediatorAssemblyAttribute";
  public const string BehaviorAttributeMetadataName = "TimeWarp.Mediator.MediatorBehaviorAttribute";
  public const string ScopeAttributeMetadataName = "TimeWarp.Mediator.MediatorScopeAttribute";
  public const string MsBuildPropertyName = "build_property.TimeWarpMediatorAssembly";
  public const string NamespacePropertyName = "build_property.TimeWarpMediatorNamespace";
  public const string GeneratorPropertyName = "build_property.TimeWarpMediatorGenerator";

  private Membership(
      IAssemblySymbol compilationAssembly,
      ImmutableHashSet<IAssemblySymbol> memberAssemblies,
      string generatedNamespace,
      bool isHost)
  {
    CompilationAssembly = compilationAssembly;
    IsHost = isHost;
    MemberAssemblies = memberAssemblies;
    GeneratedNamespace = generatedNamespace;
  }

  public IAssemblySymbol CompilationAssembly { get; }

  public ImmutableHashSet<IAssemblySymbol> MemberAssemblies { get; }

  public string GeneratedNamespace { get; }

  /// <summary>
  /// True when this compilation runs the generator (the generator itself, or MSBuild
  /// TimeWarpMediatorAssembly=true from the generator package), so the graph is the whole
  /// program and the unscoped ISender is the generated Mediator.
  /// </summary>
  public bool IsHost { get; }

  public bool Includes(ISymbol symbol)
  {
    IAssemblySymbol? assembly = symbol.ContainingAssembly;
    if (assembly is null)
    {
      return false;
    }

    foreach (IAssemblySymbol member in MemberAssemblies)
    {
      if (SymbolEqualityComparer.Default.Equals(member, assembly))
      {
        return true;
      }
    }

    return false;
  }

  public static bool TryCreate(
      Compilation compilation,
      MembershipOptions options,
      bool currentCompilationIsHost,
      CancellationToken cancellationToken,
      out Membership membership)
  {
    bool attributeMember = HasAssemblyAttribute(compilation.Assembly, AssemblyAttributeMetadataName);

    if (!currentCompilationIsHost && !options.IsMsBuildMember && !attributeMember)
    {
      membership = null!;
      return false;
    }

    HashSet<IAssemblySymbol> members = new(AssemblySymbolComparer.Instance)
        {
            compilation.Assembly
        };

    foreach (IAssemblySymbol assembly in GetReferencedAssemblies(compilation))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (HasAssemblyAttribute(assembly, AssemblyAttributeMetadataName))
      {
        members.Add(assembly);
      }
    }

    ImmutableHashSet<IAssemblySymbol>.Builder memberBuilder = ImmutableHashSet.CreateBuilder(AssemblySymbolComparer.Instance);
    foreach (IAssemblySymbol member in members)
    {
      memberBuilder.Add(member);
    }

    membership = new Membership(
        compilation.Assembly,
        memberBuilder.ToImmutable(),
        options.GeneratedNamespace,
        currentCompilationIsHost || options.IsMsBuildMember);
    return true;
  }

  public static Membership ForGenerator(Compilation compilation, MembershipOptions options, CancellationToken cancellationToken)
  {
    if (TryCreate(compilation, options, currentCompilationIsHost: true, cancellationToken, out Membership membership))
    {
      return membership;
    }

    throw new InvalidOperationException("Generator membership should always succeed for the current compilation.");
  }

  private static bool HasAssemblyAttribute(IAssemblySymbol assembly, string metadataName)
  {
    foreach (AttributeData attribute in assembly.GetAttributes())
    {
      if (attribute.AttributeClass?.ToDisplayString() == metadataName)
      {
        return true;
      }
    }

    return false;
  }

  private static IEnumerable<IAssemblySymbol> GetReferencedAssemblies(Compilation compilation)
  {
    foreach (MetadataReference reference in compilation.References)
    {
      if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
      {
        yield return assembly;
      }
    }
  }

  public static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol namespaceSymbol)
  {
    foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
    {
      foreach (INamedTypeSymbol nested in EnumerateTypeAndNested(type))
      {
        yield return nested;
      }
    }

    foreach (INamespaceSymbol child in namespaceSymbol.GetNamespaceMembers())
    {
      foreach (INamedTypeSymbol type in EnumerateTypes(child))
      {
        yield return type;
      }
    }
  }

  private static IEnumerable<INamedTypeSymbol> EnumerateTypeAndNested(INamedTypeSymbol type)
  {
    yield return type;
    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      foreach (INamedTypeSymbol child in EnumerateTypeAndNested(nested))
      {
        yield return child;
      }
    }
  }

  private sealed class AssemblySymbolComparer : IEqualityComparer<IAssemblySymbol>
  {
    public static readonly AssemblySymbolComparer Instance = new();

    public bool Equals(IAssemblySymbol? x, IAssemblySymbol? y)
    {
      return SymbolEqualityComparer.Default.Equals(x, y);
    }

    public int GetHashCode(IAssemblySymbol obj)
    {
      return SymbolEqualityComparer.Default.GetHashCode(obj);
    }
  }
}