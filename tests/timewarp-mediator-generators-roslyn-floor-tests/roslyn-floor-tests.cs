#region Purpose
// Guards the floor leg itself: the linked compilation tests must run on Roslyn 4.8.0, not whatever
// a central package bump resolves to (013-012, G-24).
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class RoslynFloorTests
{
  [Fact]
  public void Compiler_IsTheDeclaredFloor()
  {
    Version? version = typeof(CSharpCompilation).Assembly.GetName().Version;

    version.ShouldNotBeNull();
    new Version(version!.Major, version.Minor).ShouldBe(new Version(4, 8));
  }
}