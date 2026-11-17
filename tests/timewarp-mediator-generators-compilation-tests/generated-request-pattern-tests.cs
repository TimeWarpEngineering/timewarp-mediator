#region Purpose
// A source type declares requests a sibling generator emits; Send dispatches them, and drift is TWM013.
#endregion

#region Design
// The sibling generator uses RegisterSourceOutput, so the Mediator generator cannot see its
// output. That is the case GeneratedRequestPatternAttribute exists for. The final compilation
// holds both, so Send runs and the analyzer checks each expansion. Behaviors are asserted through
// a static log in the host, read by reflection from the collectible load context.
// The metadata-only test is the reference assembly a ProjectReference passes to csc: a derived
// attribute there carries its templates as const fields and its contracts as a params parameter.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed partial class GeneratedRequestPatternTests
{
  [Fact]
  public async Task Pattern_RequestsFromSiblingGenerator_DispatchThroughWovenBehavior()
  {
    HostCompilation host = HostCompiler.Compile(
        "PatternDispatch",
        Join(ContractsSource, BehaviorSource, PatternHost),
        new SiblingGenerator(RoleActionSets));

    Problems(host).ShouldBeEmpty();
    string mediator = MediatorSource(host);
    mediator.ShouldContain("case global::App.RoleState.CreateRoleActionSet.Action");
    mediator.ShouldContain("case global::App.RoleState.UpdateRoleActionSet.Action");
    mediator.ShouldContain("global::App.Tag<global::App.RoleState.CreateRoleActionSet.Action, global::App.CreateRole.Response>");
    Assembly assembly = Load(host);
    (await Send(assembly, "App.RoleState+CreateRoleActionSet+Action")).ShouldBe("created");
    (await Send(assembly, "App.RoleState+UpdateRoleActionSet+Action")).ShouldBe("updated");
    Log(assembly).ShouldBe(new[] { "Action:App.CreateRole+Response", "Action:App.UpdateRole+Response" });
  }

  [Fact]
  public async Task Pattern_VoidActions_JoinTheHostPipeline_AndCloseAnIActionBehavior()
  {
    HostCompilation host = HostCompiler.Compile(
        "PatternScoped",
        Join(ContractsSource, ScopedHost),
        new SiblingGenerator(CounterActionSets));

    Problems(host).ShouldBeEmpty();
    host.Compilation.SyntaxTrees
        .Where(tree => tree.FilePath.Contains("Sender_ClientPipeline.g.cs", StringComparison.Ordinal))
        .ShouldHaveSingleItem()
        .GetText()
        .ToString()
        .ShouldContain("case global::App.CounterState.CreateRoleActionSet.Action");
    Assembly assembly = Load(host);
    Type scope = assembly.GetType("App.ClientPipeline", true)!;
    ServiceCollection services = new();
    assembly
        .GetType(HostCompiler.DefaultNamespace + ".TimeWarpMediatorGeneratedServiceCollectionExtensions", true)!
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == "AddGeneratedMediator" && method.IsGenericMethodDefinition)
        .MakeGenericMethod(scope)
        .Invoke(null, new object[] { services });
    await using ServiceProvider provider = services.BuildServiceProvider();
    using IServiceScope serviceScope = provider.CreateScope();
    ISender sender = (ISender)serviceScope.ServiceProvider.GetRequiredService(typeof(ISender<>).MakeGenericType(scope));
    object action = Activator.CreateInstance(assembly.GetType("App.CounterState+CreateRoleActionSet+Action", true)!)!;

    await sender.Send(action);

    Log(assembly).ShouldBe(new[] { "before:Action", "handled:CreateRole" });
  }

  [Fact]
  public async Task DerivedAttribute_FromMetadataOnlyReference_IsReadFromConstFields()
  {
    byte[] referenceImage = HostCompiler.CompileLibraryImage("StateLib", DerivedLibrary, metadataOnly: true);
    byte[] runtimeImage = HostCompiler.CompileLibraryImage("StateLib", DerivedLibrary);
    HostCompilation host = HostCompiler.Compile(
        "PatternDerived",
        Join(ContractsSource, DerivedHost),
        new SiblingGenerator(RoleActionSets),
        MetadataReference.CreateFromImage(referenceImage));

    Problems(host).ShouldBeEmpty();
    Assembly assembly = Load(host, runtimeImage);
    (await Send(assembly, "App.RoleState+CreateRoleActionSet+Action")).ShouldBe("created");
    (await Send(assembly, "App.RoleState+UpdateRoleActionSet+Action")).ShouldBe("updated");
  }

  [Fact]
  public async Task DerivedAttribute_InSource_WithDefaultConstructorParameters_IsRead()
  {
    HostCompilation host = HostCompiler.Compile(
        "PatternDerivedSource",
        Join(ContractsSource, DerivedSourceHost),
        new SiblingGenerator(RoleActionSets));

    Problems(host).ShouldBeEmpty();
    Assembly assembly = Load(host);
    (await Send(assembly, "App.RoleState+CreateRoleActionSet+Action")).ShouldBe("created");
  }

  [Fact]
  public void MissingRequestAndHandler_AreTwm013_OnTheAttribute()
  {
    HostCompilation host = HostCompiler.Compile("PatternMissing", Join(ContractsSource, PatternHost));

    Diagnostic[] twm013 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM013").ToArray();
    twm013.Select(diagnostic => diagnostic.GetMessage()).ShouldBe(
        new[]
        {
          "'App.RoleState' declares generated request 'App.RoleState.CreateRoleActionSet.Action' for contract 'App.CreateRole', which does not exist after generation",
          "'App.RoleState' declares generated handler 'App.RoleState.CreateRoleActionSet.Handler' for request 'App.RoleState.CreateRoleActionSet.Action', which does not exist after generation",
          "'App.RoleState' declares generated request 'App.RoleState.UpdateRoleActionSet.Action' for contract 'App.UpdateRole', which does not exist after generation",
          "'App.RoleState' declares generated handler 'App.RoleState.UpdateRoleActionSet.Handler' for request 'App.RoleState.UpdateRoleActionSet.Action', which does not exist after generation",
        },
        ignoreOrder: true);
    twm013.ShouldAllBe(diagnostic => Span(diagnostic) == "GeneratedRequestPattern");
    host.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "TWM001" || diagnostic.Id == "TWM002");
  }

  [Fact]
  public void EmittedRequestWithAnotherResponse_IsTwm013()
  {
    HostCompilation host = HostCompiler.Compile(
        "PatternResponseMismatch",
        Join(ContractsSource, PatternHost),
        new SiblingGenerator(StringActionSets));

    Diagnostic[] twm013 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM013").ToArray();
    twm013.Select(diagnostic => diagnostic.GetMessage()).ShouldContain(
        "'App.RoleState' declares that generated request 'App.RoleState.CreateRoleActionSet.Action' implements 'TimeWarp.Mediator.IRequest<App.CreateRole.Response>', and it does not");
  }

  [Fact]
  public void EmittedHandlerThatIsNotARequestHandler_IsTwm013()
  {
    HostCompilation host = HostCompiler.Compile(
        "PatternNotAHandler",
        Join(ContractsSource, PatternHost),
        new SiblingGenerator(NotAHandlerActionSets));

    Diagnostic[] twm013 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM013").ToArray();
    twm013.Select(diagnostic => diagnostic.GetMessage()).ShouldContain(
        "'App.RoleState' declares 'App.RoleState.CreateRoleActionSet.Handler' as the handler of 'App.RoleState.CreateRoleActionSet.Action', which is not an IRequestHandler<App.RoleState.CreateRoleActionSet.Action, App.CreateRole.Response>");
  }

  [Theory]
  [InlineData("\"{Host}.{Bogus}Action\", \"{Host}.Handler\", \"TimeWarp.Mediator.IAction\"", "unknown placeholder '{Bogus}'; use {Host}, {Contract}, or {ContractType}")]
  [InlineData("\"{Host}.{Contract}Action\", \"{Host}.Handler\", \"App.NoSuchInterface\"", "request interface 'App.NoSuchInterface' does not resolve to an interface")]
  [InlineData("\"{Host}.{Contract}Action\", \"{Host}.Handler\", \"System.IDisposable\"", "request interface 'System.IDisposable' is not an IRequest or IRequest<T>")]
  [InlineData("\"{Host}.{Contract} Action\", \"{Host}.Handler\", \"TimeWarp.Mediator.IAction\"", "request name 'App.RoleState.CreateRole Action' is not a dotted type name")]
  public void InvalidPattern_IsTwm013_FromTheGenerator(string templates, string detail)
  {
    string source = Join(ContractsSource, """

        namespace App
        {
            [TimeWarp.Mediator.GeneratedRequestPattern(
        """ + templates + """
        , typeof(CreateRole))]
            public partial class RoleState { }
        }
        """);
    HostCompilation host = HostCompiler.Compile("PatternInvalid", source);

    Diagnostic twm013 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM013").ShouldHaveSingleItem();
    twm013.GetMessage().ShouldBe(
        "Request pattern 'TimeWarp.Mediator.GeneratedRequestPatternAttribute' on 'App.RoleState' cannot be expanded: " + detail);
    MediatorSource(host).ShouldNotContain("RoleState");
  }

  [Fact]
  public void DerivedAttributeWithoutTemplates_IsTwm013_Unreadable()
  {
    HostCompilation host = HostCompiler.Compile("PatternUnreadable", Join(ContractsSource, UnreadableHost));

    Diagnostic twm013 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM013").ShouldHaveSingleItem();
    twm013.GetMessage().ShouldBe(
        "Request pattern 'App.BareAttribute' on 'App.RoleState' has no constant requestTypeName. Add a constructor parameter named requestTypeName, or a const string field named RequestTypeName on the attribute.");
    Span(twm013).ShouldBe("Bare");
  }

  [Fact]
  public void Pattern_UnrelatedEdit_StaysCached()
  {
    CSharpCompilation compilation = CSharpCompilation.Create(
        "IncrementalHost",
        new[] { CSharpSyntaxTree.ParseText(Join(ContractsSource, PatternHost), HostCompiler.ParseOptions, path: "Host.cs") },
        HostCompiler.GetBaseReferences(),
        HostCompiler.CompilationOptions);
    GeneratorDriver driver = HostCompiler.CreateDriver(trackSteps: true).RunGenerators(compilation);

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
        "namespace App { [System.Obsolete] public static class Unrelated { public static int Answer => 42; } }",
        HostCompiler.ParseOptions,
        path: "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    Reasons(result, TrackingNames.HostCandidates).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    Reasons(result, TrackingNames.Model).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    result.GeneratedSources.ShouldContain(source => source.SourceText.ToString().Contains("App.RoleState.CreateRoleActionSet.Handler"));
  }

  // Hoists using directives and assembly attributes so separate snippets compile as one file.
  private static string Join(params string[] parts)
  {
    List<string> usings = new();
    List<string> assemblyAttributes = new();
    List<string> body = new();
    foreach (string part in parts)
    {
      foreach (string line in part.Split('\n'))
      {
        if (line.StartsWith("using ", StringComparison.Ordinal))
        {
          if (!usings.Contains(line))
          {
            usings.Add(line);
          }
        }
        else if (line.StartsWith("[assembly:", StringComparison.Ordinal))
        {
          assemblyAttributes.Add(line);
        }
        else
        {
          body.Add(line);
        }
      }
    }

    return string.Join("\n", usings.Concat(assemblyAttributes).Concat(body));
  }

  private static IncrementalStepRunReason[] Reasons(GeneratorRunResult result, string stepName)
  {
    return result.TrackedSteps[stepName]
        .SelectMany(step => step.Outputs)
        .Select(output => output.Reason)
        .ToArray();
  }

  private static string Span(Diagnostic diagnostic)
  {
    return diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
  }

  private static string MediatorSource(HostCompilation host)
  {
    return string.Join(
        "\n",
        host.Compilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains("Mediator.g.cs", StringComparison.Ordinal))
            .Select(tree => tree.GetText().ToString()));
  }

  private static Diagnostic[] Problems(HostCompilation host)
  {
    return host.Diagnostics
        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
        .ToArray();
  }

  private static string[] Log(Assembly assembly)
  {
    object entries = assembly.GetType("App.Log", true)!.GetField("Entries")!.GetValue(null)!;
    return ((IEnumerable<string>)entries).ToArray();
  }

  private static async Task<string?> Send(Assembly assembly, string requestTypeName)
  {
    ServiceCollection services = new();
    assembly
        .GetType(HostCompiler.DefaultNamespace + ".TimeWarpMediatorGeneratedServiceCollectionExtensions", true)!
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == "AddGeneratedMediator" && !method.IsGenericMethod)
        .Invoke(null, new object[] { services });
    await using ServiceProvider provider = services.BuildServiceProvider();
    using IServiceScope scope = provider.CreateScope();
    ISender sender = (ISender)scope.ServiceProvider.GetRequiredService(
        assembly.GetType(HostCompiler.DefaultNamespace + ".Mediator", true)!);
    object request = Activator.CreateInstance(assembly.GetType(requestTypeName, true)!)!;
    object? response = await sender.Send(request);
    return response?.ToString();
  }

  private static Assembly Load(HostCompilation host, byte[]? companion = null)
  {
    using MemoryStream stream = new();
    EmitResult result = host.Compilation.Emit(stream);
    result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics));
    stream.Position = 0;
    AssemblyLoadContext context = new(host.Compilation.AssemblyName, isCollectible: true);
    if (companion is not null)
    {
      Assembly companionAssembly = context.LoadFromStream(new MemoryStream(companion));
      context.Resolving += (_, name) =>
          companionAssembly.GetName().Name == name.Name ? companionAssembly : null;
    }

    return context.LoadFromStream(stream);
  }

  // Stands in for a consumer generator such as an action-set emitter. Its output is not visible
  // to MediatorGenerator, the same as a real sibling generator in one compilation.
  private sealed class SiblingGenerator : IIncrementalGenerator
  {
    private readonly string Source;

    public SiblingGenerator(string source)
    {
      Source = source;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      string source = Source;
      context.RegisterSourceOutput(
          context.CompilationProvider,
          (production, _) => production.AddSource("ActionSets.g.cs", source));
    }
  }
}
