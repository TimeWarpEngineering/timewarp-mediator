#region Purpose
// Verify sample projects compile and run
#endregion
#region Design
// Samples are also in timewarp-mediator.slnx, so `dev build` already compiles
// them; `dev workflow` passes --no-build and only runs them. Standalone, the
// command builds each samples/**/*.csproj first. Every Exe sample then runs
// with `dotnet run --no-build` and must exit 0; RepoLayout.SampleExpectedOutput
// adds a stdout check for the generated-stack samples (aot, named-pipelines).
// A sample that hangs fails after SampleTimeout.
#endregion

namespace DevCli.Commands;

[NuruRoute("verify-samples", Description = "Verify code samples compile and run")]
public sealed class VerifySamplesCommand : ICommand<Unit>
{
  [Option("no-build", Description = "Run the samples without building them first")]
  public bool NoBuild { get; set; }

  public sealed class Handler : ICommandHandler<VerifySamplesCommand, Unit>
  {
    private static readonly TimeSpan SampleTimeout = TimeSpan.FromMinutes(2);

    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(VerifySamplesCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string samplesDirectory = Path.Combine(repoRoot, "samples");
      if (!Directory.Exists(samplesDirectory))
      {
        Terminal.WriteLine("No samples/ directory — nothing to verify.");
        return Unit.Value;
      }

      string[] projects = Directory
        .GetFiles(samplesDirectory, "*.csproj", SearchOption.AllDirectories)
        .Where(path =>
          !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
          && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

      if (projects.Length == 0)
      {
        Terminal.WriteLine("No sample projects found.");
        return Unit.Value;
      }

      if (!command.NoBuild && !await BuildAsync(repoRoot, projects, ct))
      {
        return Unit.Value;
      }

      string[] runnable = projects.Where(IsExecutable).ToArray();
      Terminal.WriteLine($"\nRunning {runnable.Length} executable sample(s)...");

      foreach (string project in runnable)
      {
        if (!await RunAsync(repoRoot, project, ct))
        {
          return Unit.Value;
        }
      }

      Terminal.WriteLine("\nSamples verified successfully!".Green());
      return Unit.Value;
    }

    private async Task<bool> BuildAsync(string repoRoot, string[] projects, CancellationToken ct)
    {
      Terminal.WriteLine($"Building {projects.Length} sample project(s)...");

      foreach (string project in projects)
      {
        string relativePath = Path.GetRelativePath(repoRoot, project);
        Terminal.WriteLine($"\nBuilding {relativePath}...");
        int exitCode = await DotNet.Build()
          .WithProject(project)
          .WithConfiguration("Release")
          .WithNoValidation()
          .RunAsync(ct);

        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"Sample failed to build: {relativePath}".Red());
          Environment.ExitCode = exitCode;
          return false;
        }
      }

      return true;
    }

    private async Task<bool> RunAsync(string repoRoot, string project, CancellationToken ct)
    {
      string relativePath = Path.GetRelativePath(repoRoot, project);
      string sampleName = Path.GetFileNameWithoutExtension(project);
      Terminal.WriteLine($"Running {relativePath}...");

      using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
      timeout.CancelAfter(SampleTimeout);

      CommandOutput output;
      try
      {
        output = await DotNet.Run()
          .WithProject(project)
          .WithConfiguration("Release")
          .WithNoBuild()
          .WithNoValidation()
          .CaptureAsync(timeout.Token);
      }
      catch (OperationCanceledException) when (!ct.IsCancellationRequested)
      {
        Terminal.WriteErrorLine($"Sample timed out after {SampleTimeout.TotalSeconds:0}s: {relativePath}".Red());
        Environment.ExitCode = 1;
        return false;
      }

      if (output.ExitCode != 0)
      {
        Terminal.WriteErrorLine(output.Combined);
        Terminal.WriteErrorLine($"Sample exited {output.ExitCode}: {relativePath}".Red());
        Environment.ExitCode = 1;
        return false;
      }

      if (RepoLayout.SampleExpectedOutput.TryGetValue(sampleName, out string? expected)
        && !output.Stdout.Contains(expected, StringComparison.Ordinal))
      {
        Terminal.WriteErrorLine(output.Combined);
        Terminal.WriteErrorLine($"Sample output is missing '{expected}': {relativePath}".Red());
        Environment.ExitCode = 1;
        return false;
      }

      return true;
    }

    private static bool IsExecutable(string project)
    {
      return XDocument.Load(project)
        .Descendants("OutputType")
        .Any(element => string.Equals(element.Value.Trim(), "Exe", StringComparison.OrdinalIgnoreCase));
    }
  }
}
