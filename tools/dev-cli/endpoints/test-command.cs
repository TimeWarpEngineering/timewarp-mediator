#region Purpose
// Run the TimeWarp.Mediator test suite
#endregion
#region Design
// Names timewarp-mediator.slnx explicitly (MSB1011). Maps Build.ps1's
// `dotnet test $Solution -c Release`.
#endregion

namespace DevCli.Commands;

[NuruRoute("test", Description = "Run the test suite")]
internal sealed class TestCommand : ICommand<Unit>
{
  [Option("quiet", "q", Description = "Hide test output unless the command fails")]
  public bool Quiet { get; set; }

  [Option("no-build", Description = "Do not build before testing")]
  public bool NoBuild { get; set; }

  internal sealed class Handler : ICommandHandler<TestCommand, Unit>
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
        return Value;
      }

      if (!await TestAsync())
      {
        return Value;
      }

      Terminal.WriteLine("\nTests completed successfully!".Green());
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
      Terminal.WriteLine("Running test suite...");
      return true;
    }

    private async Task<bool> TestAsync()
    {
      string solutionFile = RepoLayout.SolutionPath(RepoRoot);
      DotNetTestBuilder testBuilder = DotNet.Test()
        .WithProject(solutionFile)
        .WithConfiguration("Release")
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

        return true;
      }

      int exitCode = await command.RunAsync(Ct);
      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Tests failed!".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }
  }
}
