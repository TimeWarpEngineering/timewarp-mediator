#region Purpose
// Run the TimeWarp.Mediator test suite
#endregion
#region Design
// Names timewarp-mediator.slnx explicitly (MSB1011). Maps Build.ps1's
// `dotnet test $Solution -c Release`.
// The suites are xunit 2.9 on VSTest. Microsoft.Testing.Platform --coverage is
// not a runner they host, so collection is coverlet.collector via
// --collect:"XPlat Code Coverage" into artifacts/coverage. That directory is
// deleted first so a stale Cobertura file cannot satisfy the check. The command
// fails when the suites pass and no coverage.cobertura.xml was written.
#endregion

namespace DevCli.Commands;

[NuruRoute("test", Description = "Run the test suite")]
public sealed class TestCommand : ICommand<Unit>
{
  [Option("quiet", "q", Description = "Hide test output unless the command fails")]
  public bool Quiet { get; set; }

  [Option("no-build", Description = "Do not build before testing")]
  public bool NoBuild { get; set; }

  public sealed class Handler : IRequestHandler<TestCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private TestCommand Command = null!;
    private CancellationToken Ct;
    private string RepoRoot = null!;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(TestCommand command, CancellationToken ct)
    {
      Command = command;
      Ct = ct;

      if (!FindRepoRoot())
      {
        return Unit.Value;
      }

      if (!await TestAsync())
      {
        return Unit.Value;
      }

      Terminal.WriteLine("\nTests completed successfully!".Green());
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
      Terminal.WriteLine("Running test suite...");
      return true;
    }

    private async Task<bool> TestAsync()
    {
      string solutionFile = RepoLayout.SolutionPath(RepoRoot);
      string coverageDirectory = Path.Combine(RepoRoot, "artifacts", "coverage");
      if (Directory.Exists(coverageDirectory))
      {
        Directory.Delete(coverageDirectory, recursive: true);
      }

      Directory.CreateDirectory(coverageDirectory);

      DotNetTestBuilder testBuilder = DotNet.Test()
        .WithProject(solutionFile)
        .WithConfiguration("Release")
        .WithWorkingDirectory(RepoRoot)
        .WithCollect("XPlat Code Coverage")
        .WithResultsDirectory("artifacts/coverage")
        .WithNoValidation();

      if (Command.NoBuild)
      {
        testBuilder = testBuilder.WithNoBuild();
      }

      CommandResult command = testBuilder.Build();

      if (Command.Quiet)
      {
        CommandOutput result = await command.CaptureAsync(Ct);
        if (!result.Success)
        {
          Terminal.WriteErrorLine(result.Combined);
          Terminal.WriteErrorLine("Tests failed!".Red());
          Environment.ExitCode = 1;
          return false;
        }
      }
      else
      {
        int exitCode = await command.RunAsync(Ct);
        if (exitCode != 0)
        {
          Terminal.WriteErrorLine("Tests failed!".Red());
          Environment.ExitCode = exitCode;
          return false;
        }
      }

      if (!HasCobertura(coverageDirectory))
      {
        Terminal.WriteErrorLine("Tests passed but artifacts/coverage has no Cobertura file.".Red());
        Environment.ExitCode = 1;
        return false;
      }

      return true;
    }

    private static bool HasCobertura(string coverageDirectory) =>
      Directory.Exists(coverageDirectory)
      && Directory.EnumerateFiles(coverageDirectory, "coverage.cobertura.xml", SearchOption.AllDirectories).Any();
  }
}
