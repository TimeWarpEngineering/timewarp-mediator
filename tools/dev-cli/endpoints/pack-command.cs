#region Purpose
// Pack the four TimeWarp.Mediator NuGet packages and assert analyzer payload
#endregion
#region Design
// Maps Build.ps1 pack + Assert-NupkgContains. Output is artifacts/packages
// (repository.props), not the former Artifacts/. Layout gate uses NupkgLayoutCheck
// so a hollow Analyzers/Generators nupkg fails the command the same way the
// PowerShell zip asserts did.
#endregion

namespace DevCli.Commands;

[NuruRoute("pack", Description = "Pack NuGet packages and assert analyzer payload")]
internal sealed class PackCommand : ICommand<Unit>
{
  [Option("no-build", Description = "Do not build before packing")]
  public bool NoBuild { get; set; }

  internal sealed class Handler : ICommandHandler<PackCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private PackCommand Command = null!;
    private CancellationToken Ct;
    private string RepoRoot = null!;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(PackCommand command, CancellationToken ct)
    {
      Command = command;
      Ct = ct;

      if (!FindRepoRoot())
      {
        return Value;
      }

      if (!await PackAsync())
      {
        return Value;
      }

      if (!AssertPackageLayout())
      {
        return Value;
      }

      Terminal.WriteLine("\nPack completed successfully!".Green());
      return Value;
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

        Terminal.WriteLine($"Package layout verified: {Path.GetFileName(nupkgPath)}");
      }

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
