#region Purpose
// Resolves which assemblies participate in the compile-time MessageGraph.
#endregion

#region Design
// Current compilation is a member when it has [assembly: MediatorAssembly], a MediatorModule
// on a type, MediatorAssemblies listing, or MSBuild TimeWarpMediatorAssembly=true (set by the
// generator package for hosts). Referenced assemblies join only with [assembly: MediatorAssembly]
// or by being listed via MediatorAssembliesAttribute marker types. No marker → not linked.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TimeWarp.Mediator.Analyzers;

public sealed class Membership
{
    public const string AssemblyAttributeMetadataName = "TimeWarp.Mediator.MediatorAssemblyAttribute";
    public const string AssembliesAttributeMetadataName = "TimeWarp.Mediator.MediatorAssembliesAttribute";
    public const string ModuleAttributeMetadataName = "TimeWarp.Mediator.MediatorModuleAttribute";
    public const string BehaviorAttributeMetadataName = "TimeWarp.Mediator.MediatorBehaviorAttribute";
    public const string MsBuildPropertyName = "build_property.TimeWarpMediatorAssembly";
    public const string ProfilePropertyName = "build_property.TimeWarpMediatorProfile";
    public const string NamespacePropertyName = "build_property.TimeWarpMediatorNamespace";

    private Membership(
        IAssemblySymbol compilationAssembly,
        ImmutableHashSet<IAssemblySymbol> memberAssemblies,
        string profile,
        string generatedNamespace)
    {
        CompilationAssembly = compilationAssembly;
        MemberAssemblies = memberAssemblies;
        Profile = profile;
        GeneratedNamespace = generatedNamespace;
    }

    public IAssemblySymbol CompilationAssembly { get; }

    public ImmutableHashSet<IAssemblySymbol> MemberAssemblies { get; }

    public string Profile { get; }

    public string GeneratedNamespace { get; }

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
        AnalyzerConfigOptionsProvider? optionsProvider,
        bool currentCompilationIsHost,
        out Membership membership)
    {
        string profile = ReadProperty(optionsProvider, compilation, ProfilePropertyName) ?? "Host";
        string generatedNamespace = ReadProperty(optionsProvider, compilation, NamespacePropertyName)
            ?? "TimeWarp.Mediator.Generated";

        bool msBuildMember = IsTrue(ReadProperty(optionsProvider, compilation, MsBuildPropertyName));
        bool attributeMember = HasAssemblyAttribute(compilation.Assembly, AssemblyAttributeMetadataName)
            || HasAssemblyAttribute(compilation.Assembly, AssembliesAttributeMetadataName)
            || HasModuleAttribute(compilation.Assembly);

        if (!currentCompilationIsHost && !msBuildMember && !attributeMember)
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
            if (HasAssemblyAttribute(assembly, AssemblyAttributeMetadataName))
            {
                members.Add(assembly);
            }
        }

        foreach (INamedTypeSymbol marker in GetMediatorAssembliesMarkers(compilation.Assembly))
        {
            if (marker.ContainingAssembly is not null)
            {
                members.Add(marker.ContainingAssembly);
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
            profile,
            generatedNamespace);
        return true;
    }

    public static Membership ForGenerator(Compilation compilation, AnalyzerConfigOptionsProvider optionsProvider)
    {
        if (TryCreate(compilation, optionsProvider, currentCompilationIsHost: true, out Membership membership))
        {
            return membership;
        }

        throw new InvalidOperationException("Generator membership should always succeed for the current compilation.");
    }

    private static bool HasModuleAttribute(IAssemblySymbol assembly)
    {
        foreach (INamedTypeSymbol type in EnumerateTypes(assembly.GlobalNamespace))
        {
            foreach (AttributeData attribute in type.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == ModuleAttributeMetadataName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetMediatorAssembliesMarkers(IAssemblySymbol assembly)
    {
        foreach (AttributeData attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != AssembliesAttributeMetadataName)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            TypedConstant argument = attribute.ConstructorArguments[0];
            if (argument.Kind == TypedConstantKind.Array)
            {
                foreach (TypedConstant element in argument.Values)
                {
                    if (element.Value is INamedTypeSymbol marker)
                    {
                        yield return marker;
                    }
                }
            }
            else if (argument.Value is INamedTypeSymbol single)
            {
                yield return single;
            }
        }
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

    private static string? ReadProperty(
        AnalyzerConfigOptionsProvider? optionsProvider,
        Compilation compilation,
        string key)
    {
        if (optionsProvider is not null
            && optionsProvider.GlobalOptions.TryGetValue(key, out string? value)
            && !string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        // Syntax trees can carry the same MSBuild-visible properties.
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            if (optionsProvider is null)
            {
                break;
            }

            if (optionsProvider.GetOptions(tree).TryGetValue(key, out string? perTree)
                && !string.IsNullOrWhiteSpace(perTree))
            {
                return perTree.Trim();
            }
        }

        return null;
    }

    private static bool IsTrue(string? value)
    {
        return value is not null
            && (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || value == "1");
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
