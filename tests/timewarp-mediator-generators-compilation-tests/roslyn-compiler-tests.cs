#region Purpose
// The Microsoft.CodeAnalysis version the shipped generator references must not be newer
// than the compiler that loads it.
#endregion

#region Design
// The host compiler is the SDK selected by global.json. Its Microsoft.CodeAnalysis.dll
// is $(RoslynTargetsPath)/bincore/Microsoft.CodeAnalysis.dll, baked into this assembly as
// metadata SdkRoslynCompilerAssembly. AssemblyName.GetAssemblyName reads that file's
// AssemblyVersion without loading it into the test process.
// typeof(CSharpCompilation).Assembly is the central Microsoft.CodeAnalysis.CSharp package
// this project references, so comparing the generator reference to it would not fail when
// the pin outruns the SDK.
// The informational version carries an SDK build label (5.12.0-1.<build>) and the file
// version uses the 5.MM00 numbering (5.1200.*). Roslyn refuses a newer analyzer by
// assembly version (CS8032, CS9057), so this test compares assembly versions.
// The analyzer ships in TimeWarp.Mediator and is held to the same pin.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class RoslynCompilerTests
{
  [Fact]
  public void ReferencedCompiler_IsNotNewerThanTheSdkCompiler()
  {
    Version host = SdkCompilerVersion();
    ReferencedCompiler(typeof(MediatorGenerator).Assembly).ShouldBeLessThanOrEqualTo(host);
    ReferencedCompiler(typeof(TimeWarp.Mediator.Analyzers.RequestHandlerAnalyzer).Assembly)
        .ShouldBeLessThanOrEqualTo(host);
  }

  // SdkRoslynCompilerAssembly is the SDK compiler, not the NuGet pin loaded in this process.
  private static Version SdkCompilerVersion()
  {
    string? path = typeof(RoslynCompilerTests).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == "SdkRoslynCompilerAssembly")
        .Value;
    path.ShouldNotBeNullOrWhiteSpace();
    string fullPath = Path.GetFullPath(path);
    File.Exists(fullPath).ShouldBeTrue(fullPath);
    AssemblyName host = AssemblyName.GetAssemblyName(fullPath);
    host.Name.ShouldBe("Microsoft.CodeAnalysis");
    Version? version = host.Version;
    version.ShouldNotBeNull();
    return version;
  }

  private static Version ReferencedCompiler(Assembly assembly)
  {
    AssemblyName reference = assembly.GetReferencedAssemblies()
        .Single(name => name.Name == "Microsoft.CodeAnalysis");
    Version? version = reference.Version;
    version.ShouldNotBeNull();
    return version;
  }
}
