#region Purpose
// Mode-aware CI/CD pipeline for TimeWarp.Mediator
#endregion
#region Design
// PR/merge: clean -> build -> test -> pack (analyzer layout gate; artifacts upload).
// Release:  clean -> build -> pack -> check-version -> push (no test gate).
// Handlers are invoked directly so CI does not need a pre-installed ./bin/dev.
// Push is skipped when no API key is supplied (local pack-only).
#endregion

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Execute full CI/CD pipeline (mode-aware)")]
internal sealed class WorkflowCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME)")]
  public string? Mode { get; set; }

  [Option("api-key", "k", Description = "NuGet API key for publishing (from OIDC Trusted Publishing)")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : ICommandHandler<WorkflowCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly IRepoCleanService RepoCleanService;
    private readonly NuGetVersionService NuGetVersionService;
    private readonly IRepoConfigService RepoConfigService;
    private readonly IPackableProjectService PackableProjectService;
    private CancellationToken Ct;
    private string RepoRoot = null!;

    public Handler
    (
      ITerminal terminal,
      IRepoCleanService repoCleanService,
      NuGetVersionService nuGetVersionService,
      IRepoConfigService repoConfigService,
      IPackableProjectService packableProjectService
    )
    {
      Terminal = terminal;
      RepoCleanService = repoCleanService;
      NuGetVersionService = nuGetVersionService;
      RepoConfigService = repoConfigService;
      PackableProjectService = packableProjectService;
    }

    public async ValueTask<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      Ct = ct;

      if (!FindRepoRoot())
      {
        return Value;
      }

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      CiMode mode = CiModeDetector.DetermineMode(command.Mode, eventName);
      if (string.IsNullOrEmpty(command.Mode))
      {
        string displayEventName = eventName ?? "(not set)";
        Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {displayEventName} -> Mode: {mode}");
      }

      Terminal.WriteLine($"\nCI/CD Pipeline — Mode: {mode}\n".Cyan());

      if (mode == CiMode.Release)
      {
        await RunReleaseAsync(command.ApiKey);
      }
      else
      {
        await RunPrAsync();
      }

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

    private async Task RunPrAsync()
    {
      Terminal.WriteLine("Pipeline: clean -> build -> test -> pack\n");
      Environment.ExitCode = 0;

      if (!await RunStepAsync("Clean", new CleanCommand.Handler(Terminal, RepoCleanService).Handle(new CleanCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Build", new BuildCommand.Handler(Terminal).Handle(new BuildCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Test", new TestCommand.Handler(Terminal).Handle(new TestCommand { NoBuild = true }, Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Pack", new PackCommand.Handler(Terminal).Handle(new PackCommand { NoBuild = true }, Ct)))
      {
        return;
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
    }

    private async Task RunReleaseAsync(string? apiKey)
    {
      Terminal.WriteLine("Pipeline: clean -> build -> pack -> check-version -> push\n");
      Environment.ExitCode = 0;

      if (!await RunStepAsync("Clean", new CleanCommand.Handler(Terminal, RepoCleanService).Handle(new CleanCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Build", new BuildCommand.Handler(Terminal).Handle(new BuildCommand(), Ct)))
      {
        return;
      }

      if (!await RunStepAsync("Pack", new PackCommand.Handler(Terminal).Handle(new PackCommand { NoBuild = true }, Ct)))
      {
        return;
      }

      CheckVersionCommand.Handler checkVersionHandler = new(
        Terminal,
        NuGetVersionService,
        RepoConfigService,
        PackableProjectService);
      if (!await RunStepAsync("Check Version", checkVersionHandler.Handle(new CheckVersionCommand(), Ct)))
      {
        return;
      }

      if (!await PushAsync(apiKey))
      {
        return;
      }

      Terminal.WriteLine("\nPipeline SUCCEEDED".Green());
    }

    private async Task<bool> PushAsync(string? apiKey)
    {
      apiKey ??= Environment.GetEnvironmentVariable("NUGET_API_KEY");
      if (string.IsNullOrWhiteSpace(apiKey))
      {
        Terminal.WriteLine("\nNo API key supplied — pack-only (skipping push).".Yellow());
        return true;
      }

      string glob = Path.Combine(RepoRoot, "artifacts", "packages", "*.nupkg");
      Terminal.WriteLine($"\nPushing {glob} to nuget.org...");

      int exitCode = await DotNet.NuGet()
        .Push(glob)
        .WithApiKey(apiKey)
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithSkipDuplicate()
        .RunAsync(Ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Push failed!".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> RunStepAsync(string stepName, ValueTask<Unit> step)
    {
      await step;

      if (Environment.ExitCode != 0)
      {
        Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} failed".Red());
        return false;
      }

      return true;
    }
  }
}
