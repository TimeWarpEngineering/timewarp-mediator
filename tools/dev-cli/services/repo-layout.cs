#region Purpose
// Shared paths and packable project list for the TimeWarp.Mediator dev CLI
#endregion
#region Design
// Every clean/build/test names timewarp-mediator.slnx explicitly.
// Packable product projects live under source/ with kebab path basenames;
// PackageId/AssemblyName stay PascalCase (TimeWarp.Mediator*).
// The package smoke project sits outside the slnx because it restores the
// packed nupkgs from artifacts/packages, so it can only build after pack.
// The reflection runtime is not packed. Contracts is the only lib package.
#endregion

namespace DevCli;

internal static class RepoLayout
{
  internal const string SolutionFileName = "timewarp-mediator.slnx";

  internal static readonly string[] PackableProjects =
  [
    "source/timewarp-mediator-contracts/timewarp-mediator-contracts.csproj",
    "source/timewarp-mediator-analyzers/timewarp-mediator-analyzers.csproj",
    "source/timewarp-mediator-generators/timewarp-mediator-generators.csproj"
  ];

  // Every nupkg carries the Apache-2.0 notice for the MediatR-derived code and the Unlicense text.
  private static readonly string[] LicenseEntries = ["notice", "unlicense"];

  internal static readonly (string PackageId, string[] RequiredEntries)[] PackageLayout =
  [
    ("TimeWarp.Mediator.Contracts", ["lib/net10.0/TimeWarp.Mediator.Contracts.dll", .. LicenseEntries]),
    ("TimeWarp.Mediator.Analyzers", ["analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll", .. LicenseEntries]),
    ("TimeWarp.Mediator.Generators",
    [
      "analyzers/dotnet/cs/TimeWarp.Mediator.Generators.dll",
      "analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll",
      "buildTransitive/TimeWarp.Mediator.Generators.props",
      .. LicenseEntries
    ])
  ];

  // The only lib/ TFM folder Contracts may ship (net10.0 floor). A stray
  // or missing folder changes what consumers on other TFMs resolve.
  internal const string LibTargetFramework = "net10.0";

  internal static readonly string[] LibPackageIds = ["TimeWarp.Mediator.Contracts"];

  internal const string PackageSmokeProject =
    "tests/timewarp-mediator-package-smoke/timewarp-mediator-package-smoke.csproj";

  // Stdout a sample must print besides exiting 0; samples not listed only need exit code 0.
  internal static readonly Dictionary<string, string> SampleExpectedOutput = new(StringComparer.Ordinal)
  {
    ["timewarp-mediator-examples-aot"] = "aot-pong",
    ["timewarp-mediator-examples-named-pipelines"] = "server:hello"
  };

  internal static readonly (string Name, string Project)[] BenchmarkProjects =
  [
    ("comparison", "tests/timewarp-mediator-benchmarks-comparison/timewarp-mediator-benchmarks-comparison.csproj")
  ];

  /// <summary>Reads the single <c>&lt;Version&gt;</c> from source/Directory.Build.props.</summary>
  internal static string? ReadVersion(string repoRoot)
  {
    string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
    if (!File.Exists(propsPath))
    {
      return null;
    }

    return XDocument.Load(propsPath)
      .Descendants("Version")
      .Select(element => element.Value.Trim())
      .FirstOrDefault(value => value.Length > 0);
  }

  internal static string SolutionPath(string repoRoot) =>
    Path.Combine(repoRoot, SolutionFileName);
}
