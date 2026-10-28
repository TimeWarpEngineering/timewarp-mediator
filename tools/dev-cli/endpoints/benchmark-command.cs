#region Purpose
// Run a BenchmarkDotNet project and optionally record the result as the stored baseline
#endregion
#region Design
// Runs tests/<project> in Release through BenchmarkSwitcher, so --filter reaches
// BenchmarkDotNet unchanged (its default config already exports the GitHub
// markdown report). --framework selects the host TFM (default
// RepoLayout.GateTargetFramework, net10.0). Results land in
// artifacts/benchmarks/<name>/<framework>/ (not the BenchmarkDotNet.Artifacts/
// default) so `dev clean` owns them. --record on net10.0 copies the
// GitHub-flavored report to documentation/benchmarks/<name>-baseline.md.
// Any other framework writes <name>-baseline-<label>.md (net11.0 -> net11) and
// leaves the net10.0 gate file alone. The header carries the commit SHA, the
// framework, a dirty-tree flag and the UTC date; BenchmarkDotNet's own block
// records the host (CPU, OS, runtime). Not part of `dev workflow`: shared CI
// runners are too noisy for a stored number. --gate (comparison only) re-runs
// net10.0 and fails when a TimeWarp* row exceeds comparison-baseline.md.
// Tolerance and the 15.0 target table are written by --record; see BenchmarkGate.
// --aot publishes the comparison project with PublishAot (BenchmarkAot=true)
// and runs the TimeWarp* methods. BenchmarkDotNet is not in that compilation.
// The report is documentation/benchmarks/comparison-baseline-aot.md and is not gated.
// The header states that each row is invoked through a delegate. A failed publish or
// run assigns that process exit code directly.
#endregion

namespace DevCli.Commands;

[NuruRoute("benchmark", Description = "Run benchmarks (--record stores the baseline, --gate fails on regression)")]
public sealed class BenchmarkCommand : ICommand<Unit>
{
  [Option("project", "p", Description = "Benchmark project: comparison (default)")]
  public string? Project { get; set; }

  [Option("framework", Description = "Target framework: net10.0 (default) or net11.0. net11.0 --record writes comparison-baseline-net11.md")]
  public string? Framework { get; set; }

  [Option("filter", "f", Description = "BenchmarkDotNet --filter glob (default: *)")]
  public string? Filter { get; set; }

  [Option("record", "r", Description = "Write the report. net10.0 updates comparison-baseline.md; another TFM writes a separate file")]
  public bool Record { get; set; }

  [Option("gate", "g", Description = "Fail when a TimeWarp* comparison row exceeds the recorded baseline")]
  public bool Gate { get; set; }

  [Option("aot", Description = "Publish the comparison project with PublishAot and record comparison-baseline-aot.md (not gated)")]
  public bool Aot { get; set; }

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
      if ((command.Record ? 1 : 0) + (command.Gate ? 1 : 0) + (command.Aot ? 1 : 0) > 1)
      {
        Terminal.WriteErrorLine("Pass only one of --record, --gate, or --aot.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Gate && !string.Equals(name, "comparison", StringComparison.OrdinalIgnoreCase))
      {
        Terminal.WriteErrorLine("The regression gate applies to the comparison project only.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Gate && !string.IsNullOrWhiteSpace(command.Filter) && command.Filter.Trim() != "*")
      {
        Terminal.WriteErrorLine("--gate runs the full comparison set. Omit --filter.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string framework = string.IsNullOrWhiteSpace(command.Framework)
        ? RepoLayout.GateTargetFramework
        : command.Framework.Trim();
      if (!RepoLayout.LibTargetFrameworks.Contains(framework, StringComparer.Ordinal))
      {
        string knownFrameworks = string.Join(", ", RepoLayout.LibTargetFrameworks);
        Terminal.WriteErrorLine($"Unknown framework '{framework}'. Known: {knownFrameworks}".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Gate && !string.Equals(framework, RepoLayout.GateTargetFramework, StringComparison.Ordinal))
      {
        Terminal.WriteErrorLine(
          $"The regression gate reads documentation/benchmarks/comparison-baseline.md, the {RepoLayout.GateTargetFramework} record. Omit --framework or pass {RepoLayout.GateTargetFramework}.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Aot && !string.Equals(name, "comparison", StringComparison.OrdinalIgnoreCase))
      {
        Terminal.WriteErrorLine("The AOT lane applies to the comparison project only.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Aot && !string.IsNullOrWhiteSpace(command.Filter) && command.Filter.Trim() != "*")
      {
        Terminal.WriteErrorLine("--aot runs every TimeWarp* row. Omit --filter.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Aot && !string.Equals(framework, RepoLayout.GateTargetFramework, StringComparison.Ordinal))
      {
        Terminal.WriteErrorLine(
          $"--aot records documentation/benchmarks/comparison-baseline-aot.md on {RepoLayout.GateTargetFramework}. Omit --framework or pass {RepoLayout.GateTargetFramework}.".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      (string Name, string Project) match = RepoLayout.BenchmarkProjects
        .FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
      if (match.Project is null)
      {
        string known = string.Join(", ", RepoLayout.BenchmarkProjects.Select(candidate => candidate.Name));
        Terminal.WriteErrorLine($"Unknown benchmark project '{name}'. Known: {known}".Red());
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (command.Aot)
      {
        await RunAotAsync(repoRoot, match.Project, framework, ct);
        return Unit.Value;
      }

      string artifactsPath = Path.Combine(repoRoot, "artifacts", "benchmarks", match.Name, framework);
      if (Directory.Exists(artifactsPath))
      {
        Directory.Delete(artifactsPath, true);
      }

      string filter = string.IsNullOrWhiteSpace(command.Filter) ? "*" : command.Filter;
      Terminal.WriteLine($"Running {match.Project} ({framework}, filter {filter})...");
      int exitCode = await DotNet.Run()
        .WithProject(Path.Combine(repoRoot, match.Project))
        .WithConfiguration("Release")
        .WithFramework(framework)
        .WithArguments("--filter", filter, "--artifacts", artifactsPath)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Benchmark run failed!".Red());
        Environment.ExitCode = exitCode;
        return Unit.Value;
      }

      if (command.Record && !await RecordAsync(repoRoot, match.Name, match.Project, framework, artifactsPath, ct))
      {
        return Unit.Value;
      }

      if (command.Gate && !await GateAsync(repoRoot, artifactsPath, ct))
      {
        return Unit.Value;
      }

      Terminal.WriteLine($"\nBenchmark results: {Path.GetRelativePath(repoRoot, artifactsPath)}".Green());
      return Unit.Value;
    }

    private async Task RunAotAsync(string repoRoot, string project, string framework, CancellationToken ct)
    {
      string rid = HostRid();
      string output = Path.Combine(repoRoot, "artifacts", "benchmarks", "comparison", "aot-" + framework);
      if (Directory.Exists(output))
      {
        Directory.Delete(output, true);
      }

      Terminal.WriteLine($"Publishing {project} ({framework}, {rid}, PublishAot)...");
      CommandOutput publish = await Shell.Builder("dotnet")
        .WithArguments(
          "publish",
          Path.Combine(repoRoot, project),
          "-c", "Release",
          "-f", framework,
          "-r", rid,
          "-p:BenchmarkAot=true",
          "-o", output,
          "--nologo")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (publish.ExitCode != 0)
      {
        Terminal.WriteErrorLine(publish.Stdout);
        Terminal.WriteErrorLine(publish.Stderr);
        Terminal.WriteErrorLine("Native AOT publish failed.".Red());
        Environment.ExitCode = publish.ExitCode;
        return;
      }

      string exe = Path.Combine(output, "timewarp-mediator-benchmarks-comparison");
      if (OperatingSystem.IsWindows())
      {
        exe += ".exe";
      }

      CommandOutput run = await Shell.Builder(exe)
        .WithWorkingDirectory(output)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (run.ExitCode != 0)
      {
        Terminal.WriteErrorLine(run.Stdout);
        Terminal.WriteErrorLine(run.Stderr);
        Terminal.WriteErrorLine("Native AOT benchmark run failed.".Red());
        Environment.ExitCode = run.ExitCode;
        return;
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
        return;
      }

      bool dirty = status.Stdout.Trim().Length > 0;
      System.Text.StringBuilder baseline = new();
      baseline.AppendLine("# Benchmark baseline: comparison (Native AOT)");
      baseline.AppendLine();
      baseline.AppendLine("Recorded by `./bin/dev benchmark --project comparison --aot`. Do not edit by hand; re-record instead.");
      baseline.AppendLine();
      baseline.AppendLine($"- Project: `{project}`");
      baseline.AppendLine($"- Framework: `{framework}`");
      baseline.AppendLine($"- RID: `{rid}`");
      baseline.AppendLine($"- Commit: `{sha.Stdout.Trim()}`{(dirty ? " (working tree had uncommitted changes)" : string.Empty)}");
      baseline.AppendLine($"- Date (UTC): {DateTime.UtcNow:yyyy-MM-dd}");
      baseline.AppendLine();
      baseline.AppendLine("BenchmarkDotNet's NativeAotToolchain is not used. The BenchmarkDotNet package is omitted from this publish (`BenchmarkAot=true`), and a small runner calls the `TimeWarp*` methods. Each iteration invokes the row through a delegate, so the mean includes that call. The result is stored in a generic static field and is not boxed. These numbers are not gated.");
      baseline.AppendLine();
      baseline.AppendLine(run.Stdout.Trim());
      baseline.AppendLine();

      string outputDirectory = Path.Combine(repoRoot, "documentation", "benchmarks");
      Directory.CreateDirectory(outputDirectory);
      string outputPath = Path.Combine(outputDirectory, "comparison-baseline-aot.md");
      await File.WriteAllTextAsync(outputPath, baseline.ToString().ReplaceLineEndings("\n"), ct);
      Terminal.WriteLine($"Recorded {Path.GetRelativePath(repoRoot, outputPath)}".Green());
      Terminal.WriteLine(run.Stdout);
    }

    private static string HostRid()
    {
      string os = OperatingSystem.IsLinux() ? "linux"
        : OperatingSystem.IsMacOS() ? "osx"
        : OperatingSystem.IsWindows() ? "win"
        : throw new PlatformNotSupportedException("Native AOT publish needs linux, osx, or win.");
      string arch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture switch
      {
        System.Runtime.InteropServices.Architecture.X64 => "x64",
        System.Runtime.InteropServices.Architecture.Arm64 => "arm64",
        _ => throw new PlatformNotSupportedException("Native AOT publish needs x64 or arm64.")
      };
      return os + "-" + arch;
    }

    private async Task<bool> RecordAsync(
      string repoRoot,
      string name,
      string project,
      string framework,
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
      bool gateFile = string.Equals(framework, RepoLayout.GateTargetFramework, StringComparison.Ordinal);
      string recordCommand = gateFile
        ? $"./bin/dev benchmark --project {name} --record"
        : $"./bin/dev benchmark --project {name} --framework {framework} --record";
      System.Text.StringBuilder baseline = new();
      baseline.AppendLine($"# Benchmark baseline: {name}");
      baseline.AppendLine();
      baseline.AppendLine($"Recorded by `{recordCommand}`. Do not edit by hand; re-record instead.");
      baseline.AppendLine();
      baseline.AppendLine($"- Project: `{project}`");
      baseline.AppendLine($"- Framework: `{framework}`");
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

      if (string.Equals(name, "comparison", StringComparison.OrdinalIgnoreCase) && gateFile)
      {
        baseline.AppendLine(BenchmarkGate.BaselineSuffix().TrimEnd());
        baseline.AppendLine();
      }
      else if (string.Equals(name, "comparison", StringComparison.OrdinalIgnoreCase))
      {
        baseline.AppendLine(
          $"This file records `{framework}`. The regression gate reads `documentation/benchmarks/comparison-baseline.md` (`{RepoLayout.GateTargetFramework}`) and does not use these numbers.");
        baseline.AppendLine();
      }

      string outputDirectory = Path.Combine(repoRoot, "documentation", "benchmarks");
      Directory.CreateDirectory(outputDirectory);
      string outputPath = Path.Combine(outputDirectory, BaselineFileName(name, framework));
      await File.WriteAllTextAsync(outputPath, baseline.ToString().ReplaceLineEndings("\n"), ct);
      Terminal.WriteLine($"Recorded {Path.GetRelativePath(repoRoot, outputPath)}");
      return true;
    }

    private async Task<bool> GateAsync(string repoRoot, string artifactsPath, CancellationToken ct)
    {
      string baselinePath = Path.Combine(repoRoot, "documentation", "benchmarks", "comparison-baseline.md");
      if (!File.Exists(baselinePath))
      {
        Terminal.WriteErrorLine($"No baseline at {Path.GetRelativePath(repoRoot, baselinePath)}. Record one with --record.".Red());
        Environment.ExitCode = 1;
        return false;
      }

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

      List<string> documents = [];
      foreach (string report in reports)
      {
        documents.Add(await File.ReadAllTextAsync(report, ct));
      }

      BenchmarkTable baseline = BenchmarkGate.Parse(await File.ReadAllTextAsync(baselinePath, ct));
      BenchmarkTable measured = BenchmarkGate.ParseMany(documents);
      List<string> failures = BenchmarkGate.Compare(baseline, measured);
      if (failures.Count > 0)
      {
        Terminal.WriteErrorLine("Benchmark gate failed:".Red());
        foreach (string failure in failures)
        {
          Terminal.WriteErrorLine("  " + failure);
        }

        Environment.ExitCode = 1;
        return false;
      }

      Terminal.WriteLine("Benchmark gate passed.".Green());
      return true;
    }

    // net10.0 keeps comparison-baseline.md (the gate file). net11.0 writes
    // comparison-baseline-net11.md and does not replace that gate file.
    private static string BaselineFileName(string name, string framework)
    {
      if (string.Equals(framework, RepoLayout.GateTargetFramework, StringComparison.Ordinal))
      {
        return $"{name}-baseline.md";
      }

      int dot = framework.IndexOf('.');
      string label = dot < 0 ? framework : framework[..dot];
      return $"{name}-baseline-{label}.md";
    }
  }
}
