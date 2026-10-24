#region Purpose
// Guards the Roslyn 5.0 floor: the compilation host and the shipped components must reference
// that compiler, not whatever a central package bump resolves to.
#endregion

#region Design
// 5.0.0 is the compiler in the oldest .NET 10 SDK (10.0.100). A newer Microsoft.CodeAnalysis
// reference fails to load there (CS9057). The loaded CSharpCompilation assembly tracks the
// central pin. The shipped analyzer and generator reference Microsoft.CodeAnalysis, which the
// Microsoft.CodeAnalysis.CSharp package brings in, so their metadata version tracks VersionOverride.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class RoslynFloorTests
{
  [Fact]
  public void Compiler_IsTheDeclaredFloor()
  {
    Version? loaded = typeof(CSharpCompilation).Assembly.GetName().Version;

    loaded.ShouldNotBeNull();
    new Version(loaded!.Major, loaded.Minor).ShouldBe(new Version(5, 0));
    ReferencedCompiler(typeof(MediatorGenerator).Assembly).ShouldBe(new Version(5, 0));
    ReferencedCompiler(typeof(TimeWarp.Mediator.Analyzers.RequestHandlerAnalyzer).Assembly)
        .ShouldBe(new Version(5, 0));
  }

  private static Version ReferencedCompiler(Assembly assembly)
  {
    AssemblyName reference = assembly.GetReferencedAssemblies()
        .Single(name => name.Name == "Microsoft.CodeAnalysis");
    reference.Version.ShouldNotBeNull();
    return new Version(reference.Version!.Major, reference.Version.Minor);
  }
}
