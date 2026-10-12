#region Purpose
// Shared paths and packable project list for the TimeWarp.Mediator dev CLI
#endregion
#region Design
// Every clean/build/test names timewarp-mediator.slnx explicitly.
// Packable product projects live under source/ with kebab path basenames;
// PackageId/AssemblyName stay PascalCase (TimeWarp.Mediator*).
#endregion

namespace DevCli;

internal static class RepoLayout
{
  internal const string SolutionFileName = "timewarp-mediator.slnx";

  internal static readonly string[] PackableProjects =
  [
    "source/timewarp-mediator/timewarp-mediator.csproj",
    "source/timewarp-mediator-contracts/timewarp-mediator-contracts.csproj",
    "source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj",
    "source/timewarp-mediator-generators/timewarp-mediator-generators.csproj"
  ];

  // Every nupkg carries the Apache-2.0 notice for the MediatR-derived code and the Unlicense text.
  private static readonly string[] LicenseEntries = ["notice", "unlicense"];

  internal static readonly (string PackageId, string[] RequiredEntries)[] PackageLayout =
  [
    ("TimeWarp.Mediator", ["lib/net10.0/TimeWarp.Mediator.dll", .. LicenseEntries]),
    ("TimeWarp.Mediator.Contracts", ["lib/net10.0/TimeWarp.Mediator.Contracts.dll", .. LicenseEntries]),
    ("TimeWarp.Mediator.Analyzers", ["analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll", .. LicenseEntries]),
    ("TimeWarp.Mediator.Generators",
    [
      "analyzers/dotnet/cs/TimeWarp.Mediator.Generators.dll",
      "analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll",
      .. LicenseEntries
    ])
  ];

  internal static string SolutionPath(string repoRoot) =>
    Path.Combine(repoRoot, SolutionFileName);
}
