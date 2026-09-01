#region Purpose
// Shared paths and packable project list for the TimeWarp.Mediator dev CLI
#endregion
#region Design
// Root has both TimeWarp.Mediator.sln and timewarp-mediator.slnx. Unadorned
// `dotnet build` hits MSB1011, so every clean/build/test names the slnx.
// Packable projects stay under src/ until 006-003 kebab-renames the tree.
#endregion

namespace DevCli;

internal static class RepoLayout
{
  internal const string SolutionFileName = "timewarp-mediator.slnx";

  internal static readonly string[] PackableProjects =
  [
    "src/TimeWarp.Mediator/TimeWarp.Mediator.csproj",
    "src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj",
    "src/TimeWarp.Mediator.Analyzers/TimeWarp.Mediator.Analyzers.csproj",
    "src/TimeWarp.Mediator.Generators/TimeWarp.Mediator.Generators.csproj"
  ];

  internal static readonly (string PackageId, string[] RequiredEntries)[] PackageLayout =
  [
    ("TimeWarp.Mediator.Analyzers", ["analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll"]),
    ("TimeWarp.Mediator.Generators",
    [
      "analyzers/dotnet/cs/TimeWarp.Mediator.Generators.dll",
      "analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll"
    ])
  ];

  internal static string SolutionPath(string repoRoot) =>
    Path.Combine(repoRoot, SolutionFileName);
}
