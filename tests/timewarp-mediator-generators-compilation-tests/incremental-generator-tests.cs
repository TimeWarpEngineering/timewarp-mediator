#region Purpose
// The generator pipeline is incremental and TWM001–TWM003 have one reporter in a host (013-011).
#endregion

#region Design
// Cached means the step did not run. An unrelated file matches no syntax predicate and does not
// change metadata references, so host candidates, referenced assemblies, and the model stay Cached.
// A new handler matches the type predicate, so those steps are Modified.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed class IncrementalGeneratorTests
{
  [Fact]
  public void UnrelatedEdit_LeavesModelCached_AndSourcesCached()
  {
    CSharpCompilation compilation = CreateCompilation(HostSource);
    GeneratorDriver driver = HostCompiler.CreateDriver(trackSteps: true).RunGenerators(compilation);
    string[] firstTexts = GeneratedTexts(driver);

    CSharpCompilation edited = compilation.AddSyntaxTrees(
        CSharpSyntaxTree.ParseText(UnrelatedSource, HostCompiler.ParseOptions, path: "Unrelated.cs"));
    driver = driver.RunGenerators(edited);

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    Reasons(result, TrackingNames.Options).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    Reasons(result, TrackingNames.HostCandidates).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    Reasons(result, TrackingNames.ReferencedAssemblies).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    Reasons(result, TrackingNames.Model).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    Reasons(result, TrackingNames.Sources).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    IncrementalStepRunReason[] emitReasons = result.TrackedOutputSteps.SelectMany(pair => pair.Value)
        .Where(step => step.Inputs.Any(input => input.Source.Name == TrackingNames.Sources))
        .SelectMany(step => step.Outputs)
        .Select(output => output.Reason)
        .ToArray();
    emitReasons.ShouldNotBeEmpty();
    emitReasons.ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratedTexts(driver).ShouldBe(firstTexts);
  }

  [Fact]
  public void NewHandler_ModifiesModel_AndReEmits()
  {
    CSharpCompilation compilation = CreateCompilation(HostSource);
    GeneratorDriver driver = HostCompiler.CreateDriver(trackSteps: true).RunGenerators(compilation);

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(
        CSharpSyntaxTree.ParseText(NewHandlerSource, HostCompiler.ParseOptions, path: "Pong.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    Reasons(result, TrackingNames.HostCandidates).ShouldAllBe(reason => reason == IncrementalStepRunReason.Modified);
    Reasons(result, TrackingNames.ReferencedAssemblies).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    Reasons(result, TrackingNames.Model).ShouldAllBe(reason => reason == IncrementalStepRunReason.Modified);
    Reasons(result, TrackingNames.Sources).ShouldAllBe(reason => reason == IncrementalStepRunReason.Modified);
    result.GeneratedSources.ShouldContain(source => source.SourceText.ToString().Contains("App.PongHandler"));
  }

  [Fact]
  public void RequestWithNoHandler_IsReportedOnce_WithGeneratorAndAnalyzer()
  {
    HostCompilation host = HostCompiler.Compile("SingleReporter", HostSource + UnhandledSource);

    Diagnostic twm001 = host.Diagnostics.Where(d => d.Id == "TWM001").ShouldHaveSingleItem();
    twm001.GetMessage().ShouldContain("App.Orphan");
    twm001.Location.SourceTree.ShouldNotBeNull();
    twm001.Location.SourceTree!.GetText().ToString(twm001.Location.SourceSpan).ShouldBe("Orphan");
  }

  private static CSharpCompilation CreateCompilation(string source)
  {
    return CSharpCompilation.Create(
        "IncrementalHost",
        syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, HostCompiler.ParseOptions, path: "Host.cs") },
        references: HostCompiler.GetBaseReferences(),
        options: HostCompiler.CompilationOptions);
  }

  private static string[] GeneratedTexts(GeneratorDriver driver)
  {
    return driver.GetRunResult().GeneratedTrees.Select(tree => tree.ToString()).ToArray();
  }

  private static IEnumerable<IncrementalStepRunReason> Reasons(GeneratorRunResult result, string stepName)
  {
    result.TrackedSteps.ShouldContainKey(stepName);
    return result.TrackedSteps[stepName].SelectMany(step => step.Outputs).Select(output => output.Reason);
  }

  private const string HostSource = """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;

        namespace App
        {
            public sealed class Ping : IRequest<string> { }

            public sealed class PingHandler : IRequestHandler<Ping, string>
            {
                public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken) => ValueTask.FromResult("pong");
            }
        }

        """;

  private const string UnrelatedSource = """
        namespace App
        {
            public static class Unrelated
            {
                public static int Answer => 42;
            }
        }
        """;

  private const string NewHandlerSource = """
        using System.Threading;
        using System.Threading.Tasks;
        using TimeWarp.Mediator;

        namespace App
        {
            public sealed class Pong : IRequest<int> { }

            public sealed class PongHandler : IRequestHandler<Pong, int>
            {
                public ValueTask<int> Handle(Pong request, CancellationToken cancellationToken) => ValueTask.FromResult(1);
            }
        }
        """;

  private const string UnhandledSource = """
        namespace App
        {
            public sealed class Orphan : TimeWarp.Mediator.IRequest<int> { }
        }
        """;
}