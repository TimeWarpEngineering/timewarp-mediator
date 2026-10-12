#region Purpose
// Run a BenchmarkDotNet project and optionally record the result as the stored baseline
#endregion
#region Design
// Runs tests/<project> in Release through BenchmarkSwitcher, so --filter reaches
// BenchmarkDotNet unchanged. Results land in artifacts/benchmarks/<name>/ (not
// the BenchmarkDotNet.Artifacts/ default) so `dev clean` owns them. --record
// copies the GitHub-flavored report to documentation/benchmarks/<name>-baseline.md
// under a header with the commit SHA, a dirty-tree flag and the UTC date;
// BenchmarkDotNet's own block records the host (CPU, OS, runtime). Not part of
// `dev workflow`: shared CI runners are too noisy for a stored number.
#endregion

namespace DevCli.Commands;

[NuruRoute("benchmark", Description = "Run benchmarks (--record stores the baseline under documentation/benchmarks)")]
public sealed class BenchmarkCommand : ICommand<Unit>
{
  [Option("project", "p", Description = "Benchmark project: comparison (default) or legacy")]
  public string? Project { get; set; }

  [Option("filter", "f", Description = "BenchmarkDotNet --filter glob (default: *)")]
  public string? Filter { get; set; }

  [Option("record", "r", Description = "Write the report to documentation/benchmarks/<project>-baseline.md")]
  public bool Record { get; set; }

  public sealed class Handler : ICommandHandler<BenchmarkCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(BenchmarkCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string name = string.IsNullOrWhiteSpace(command.Project) ? "comparison" : command.Project.Trim();
      (string Name, string Project) match = RepoLayout.BenchmarkProjects
        .FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
      if (match.Project is null)
      {
        string known = string.Join(", ", RepoLayout.BenchmarkProjects.Select(candidate => candidate.Name));
        Terminal.WriteErrorLine($"Unknown benchmark project '{name}'. Known: {known}".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string artifactsPath = Path.Combine(repoRoot, "artifacts", "benchmarks", match.Name);
      if (Directory.Exists(artifactsPath))
      {
        Directory.Delete(artifactsPath, true);
      }

      string filter = string.IsNullOrWhiteSpace(command.Filter) ? "*" : command.Filter;
      Terminal.WriteLine($"Running {match.Project} (filter {filter})...");
      int exitCode = await DotNet.Run()
        .WithProject(Path.Combine(repoRoot, match.Project))
        .WithConfiguration("Release")
        .WithArguments("--filter", filter, "--exporters", "github", "--artifacts", artifactsPath)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Benchmark run failed!".Red());
        Environment.ExitCode = exitCode;
        return Unit.Value;
      }

      if (command.Record && !await RecordAsync(repoRoot, match.Name, match.Project, artifactsPath, ct))
      {
        return Unit.Value;
      }

      Terminal.WriteLine($"\nBenchmark results: {Path.GetRelativePath(repoRoot, artifactsPath)}".Green());
      return Unit.Value;
    }

    private async Task<bool> RecordAsync(
      string repoRoot,
      string name,
      string project,
      string artifactsPath,
      CancellationToken ct)
    {
      string resultsPath = Path.Combine(artifactsPath, "results");
      string[] reports = Directory.Exists(resultsPath)
        ? Directory.GetFiles(resultsPath, "*-report-github.md").OrderBy(path => path, StringComparer.Ordinal).ToArray()
        : [];
      if (reports.Length == 0)
      {
        Terminal.WriteErrorLine($"No *-report-github.md under {resultsPath}".Red());
        Environment.ExitCode = 1;
        return false;
      }

      CommandOutput sha = await Shell.Builder("git")
        .WithArguments("rev-parse", "HEAD")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      CommandOutput status = await Shell.Builder("git")
        .WithArguments("status", "--porcelain", "--untracked-files=no")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (sha.ExitCode != 0 || status.ExitCode != 0)
      {
        Terminal.WriteErrorLine("Could not read the commit SHA from git.".Red());
        Environment.ExitCode = 1;
        return false;
      }

      bool dirty = status.Stdout.Trim().Length > 0;
      System.Text.StringBuilder baseline = new();
      baseline.AppendLine($"# Benchmark baseline: {name}");
      baseline.AppendLine();
      baseline.AppendLine("Recorded by `./bin/dev benchmark --project " + name + " --record`. Do not edit by hand; re-record instead.");
      baseline.AppendLine();
      baseline.AppendLine($"- Project: `{project}`");
      baseline.AppendLine($"- Commit: `{sha.Stdout.Trim()}`{(dirty ? " (working tree had uncommitted changes)" : string.Empty)}");
      baseline.AppendLine($"- Date (UTC): {DateTime.UtcNow:yyyy-MM-dd}");
      baseline.AppendLine();

      foreach (string report in reports)
      {
        baseline.AppendLine($"## {Path.GetFileNameWithoutExtension(report).Replace("-report-github", string.Empty, StringComparison.Ordinal)}");
        baseline.AppendLine();
        baseline.AppendLine((await File.ReadAllTextAsync(report, ct)).Trim());
        baseline.AppendLine();
      }

      string outputDirectory = Path.Combine(repoRoot, "documentation", "benchmarks");
      Directory.CreateDirectory(outputDirectory);
      string outputPath = Path.Combine(outputDirectory, $"{name}-baseline.md");
      await File.WriteAllTextAsync(outputPath, baseline.ToString().ReplaceLineEndings("\n"), ct);
      Terminal.WriteLine($"Recorded {Path.GetRelativePath(repoRoot, outputPath)}");
      return true;
    }
  }
}
