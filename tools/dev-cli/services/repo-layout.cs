#region Purpose
// Shared paths and packable project list for the TimeWarp.Mediator dev CLI
#endregion
#region Design
// Every clean/build/test names timewarp-mediator.slnx explicitly.
// Packable product projects live under source/ with kebab path basenames;
// PackageId/AssemblyName stay PascalCase (TimeWarp.Mediator*).
// TryReadVersion mirrors CheckVersionCommand.GetVersionFromSource (xmlns then
// bare Version) so release can assert root vs source/ Directory.Build.props
// stay aligned.
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

  // Reads <Version> from Directory.Build.props (MSBuild xmlns or bare). Null if missing.
  internal static string? TryReadVersion(string directoryBuildPropsPath)
  {
    if (!File.Exists(directoryBuildPropsPath))
    {
      return null;
    }

    string xml = File.ReadAllText(directoryBuildPropsPath);
    XDocument document = XDocument.Parse(xml);
    XNamespace msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

    XElement? versionElement = document.Descendants(msbuildNamespace + "Version").FirstOrDefault();
    return (versionElement ?? document.Descendants("Version").FirstOrDefault())?.Value;
  }
}
