#region Purpose
// Pack the TimeWarp.Mediator NuGet packages and assert analyzer payload
#endregion
#region Design
// Maps Build.ps1 pack + Assert-NupkgContains. Output is artifacts/packages
// (repository.props), not the former Artifacts/. PackAsync deletes that folder
// first so FindNupkg / push only see this run's nupkgs. Layout gate uses
// NupkgLayoutCheck so a hollow Analyzers/Generators nupkg, or any nupkg missing
// notice/unlicense, fails the command
// the same way the PowerShell zip asserts did. NupkgReadmeCheck then requires
// the package's own readme.md with only absolute links. The runtime packages
// must ship exactly one lib/ TFM folder (RepoLayout.LibTargetFramework).
// Contracts is the only lib package.
// Last, the package smoke project restores Contracts + Generators from
// artifacts/packages into a throwaway packages folder (the version is not
// bumped per build, so the global cache could hold a stale nupkg of the same
// version), then builds and runs as an outside consumer would.
#endregion

namespace DevCli.Commands;

[NuruRoute("pack", Description = "Pack NuGet packages and assert analyzer payload")]
public sealed class PackCommand : ICommand<Unit>
{
  [Option("no-build", Description = "Do not build before packing")]
  public bool NoBuild { get; set; }

  public sealed class Handler : ICommandHandler<PackCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private PackCommand Command = null!;
    private CancellationToken Ct;
    private string RepoRoot = null!;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(PackCommand command, CancellationToken ct)
    {
      Command = command;
      Ct = ct;

      if (!FindRepoRoot())
      {
        return Unit.Value;
      }

      if (!await PackAsync())
      {
        return Unit.Value;
      }

      if (!AssertPackageLayout())
      {
        return Unit.Value;
      }

      if (!await RunPackageSmokeAsync())
      {
        return Unit.Value;
      }

      Terminal.WriteLine("\nPack completed successfully!".Green());
      return Unit.Value;
    }

    private bool FindRepoRoot()
    {
      string? root = Git.FindRoot();
      if (root is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return false;
      }

      RepoRoot = root;
      return true;
    }

    private async Task<bool> PackAsync()
    {
      string outputDir = Path.Combine(RepoRoot, "artifacts", "packages");
      if (Directory.Exists(outputDir))
      {
        Directory.Delete(outputDir, true);
      }

      Directory.CreateDirectory(outputDir);

      foreach (string relativeProject in RepoLayout.PackableProjects)
      {
        Terminal.WriteLine($"\nPacking {relativeProject}...");
        DotNetPackBuilder packBuilder = DotNet.Pack(Path.Combine(RepoRoot, relativeProject))
          .WithConfiguration("Release")
          .WithOutput(outputDir)
          .WithNoValidation();

        if (Command.NoBuild)
        {
          packBuilder = packBuilder.WithNoBuild();
        }

        int exitCode = await packBuilder.RunAsync(Ct);
        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"Pack failed for {relativeProject}!".Red());
          Environment.ExitCode = exitCode;
          return false;
        }
      }

      return true;
    }

    private bool AssertPackageLayout()
    {
      string outputDir = Path.Combine(RepoRoot, "artifacts", "packages");

      foreach ((string packageId, string[] requiredEntries) in RepoLayout.PackageLayout)
      {
        string? nupkgPath = FindNupkg(outputDir, packageId);
        if (nupkgPath is null)
        {
          Terminal.WriteErrorLine($"No nupkg for {packageId} under {outputDir}".Red());
          Environment.ExitCode = 1;
          return false;
        }

        IReadOnlyList<string> missing = NupkgLayoutCheck.FindMissing(nupkgPath, requiredEntries);
        if (missing.Count > 0)
        {
          Terminal.WriteErrorLine($"{Path.GetFileName(nupkgPath)} is missing: {string.Join(", ", missing)}".Red());
          Environment.ExitCode = 1;
          return false;
        }

        IReadOnlyList<string> readmeProblems = NupkgReadmeCheck.FindProblems(nupkgPath, packageId);
        if (readmeProblems.Count > 0)
        {
          Terminal.WriteErrorLine($"{Path.GetFileName(nupkgPath)}: {string.Join("; ", readmeProblems)}".Red());
          Environment.ExitCode = 1;
          return false;
        }

        if (RepoLayout.LibPackageIds.Contains(packageId, StringComparer.Ordinal))
        {
          string[] libFolders = FindLibTargetFrameworks(nupkgPath);
          if (libFolders is not [RepoLayout.LibTargetFramework])
          {
            Terminal.WriteErrorLine(
              $"{Path.GetFileName(nupkgPath)} lib/ folders are [{string.Join(", ", libFolders)}], expected [{RepoLayout.LibTargetFramework}]".Red());
            Environment.ExitCode = 1;
            return false;
          }
        }

        Terminal.WriteLine($"Package layout verified: {Path.GetFileName(nupkgPath)}");
      }

      return true;
    }

    private static string[] FindLibTargetFrameworks(string nupkgPath)
    {
      using ZipArchive archive = ZipFile.OpenRead(nupkgPath);
      return archive.Entries
        .Select(entry => entry.FullName.Split('/'))
        .Where(parts => parts.Length > 2 && parts[0] == "lib")
        .Select(parts => parts[1])
        .Distinct(StringComparer.Ordinal)
        .OrderBy(folder => folder, StringComparer.Ordinal)
        .ToArray();
    }

    private async Task<bool> RunPackageSmokeAsync()
    {
      string? version = RepoLayout.ReadVersion(RepoRoot);
      if (version is null)
      {
        Terminal.WriteErrorLine("Could not read <Version> from source/Directory.Build.props.".Red());
        Environment.ExitCode = 1;
        return false;
      }

      string packagesPath = Path.Combine(RepoRoot, "artifacts", "package-smoke", "packages");
      if (Directory.Exists(packagesPath))
      {
        Directory.Delete(packagesPath, true);
      }

      Terminal.WriteLine($"\nRunning package smoke against {version} from artifacts/packages...");
      CommandOutput output = await DotNet.Run()
        .WithProject(Path.Combine(RepoRoot, RepoLayout.PackageSmokeProject))
        .WithConfiguration("Release")
        .WithProperty("TimeWarpMediatorVersion", version)
        .WithProperty("RestorePackagesPath", packagesPath)
        .WithNoValidation()
        .CaptureAsync(Ct);

      if (output.ExitCode != 0)
      {
        Terminal.WriteErrorLine(output.Combined);
        Terminal.WriteErrorLine("Package smoke failed: the packed nupkgs do not build or run in a clean consumer.".Red());
        Environment.ExitCode = 1;
        return false;
      }

      Terminal.WriteLine(output.Stdout.Trim());
      return true;
    }

    private static string? FindNupkg(string outputDir, string packageId)
    {
      if (!Directory.Exists(outputDir))
      {
        return null;
      }

      Regex namePattern = new(
        $"^{Regex.Escape(packageId)}\\.[0-9].*\\.nupkg$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

      return Directory.GetFiles(outputDir, "*.nupkg")
        .Where(path =>
          !path.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase)
          && namePattern.IsMatch(Path.GetFileName(path)))
        .OrderBy(path => path, StringComparer.Ordinal)
        .FirstOrDefault();
    }
  }
}
