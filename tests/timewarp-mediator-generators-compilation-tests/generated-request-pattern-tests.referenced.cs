#region Purpose
// Request patterns whose contracts, derived attribute, interface, and behavior live in referenced assemblies.
#endregion

#region Design
// The consumer shape from timewarp-architecture: the host applies [ContractState(typeof(...))],
// the contracts and the attribute are references, and the response is a two-arity generic with a
// nested contract type as its first argument. Library images are compiled once per test; the
// derived attribute is a metadata-only reference, the others are full images that are also loaded
// at runtime beside the host.
#endregion

namespace TimeWarp.Mediator.Generators.Compilation.Tests;

public sealed partial class GeneratedRequestPatternTests
{
  [Fact]
  public async Task ReferencedContractsAndDerivedAttribute_OneOfResponse_Dispatch()
  {
    ReferencedLibraries libraries = ReferencedLibraries.Build();
    HostCompilation host = HostCompiler.Compile(
        "PatternReferenced",
        ReferencedOneOfHost,
        new SiblingGenerator(ThingActionSets),
        libraries.OneOf,
        libraries.Contracts,
        libraries.ContractState);

    Problems(host).ShouldBeEmpty();
    host.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "TWM013");
    string mediator = MediatorSource(host);
    mediator.ShouldContain("case global::App.ThingState.CreateThingActionSet.Action");
    mediator.ShouldContain("case global::App.ThingState.UpdateThingActionSet.Action");
    mediator.ShouldContain(
        "global::App.Tag<global::App.ThingState.CreateThingActionSet.Action, global::OneOf.OneOf<global::Ns.CreateThing.Response, global::Ns.SharedProblemDetails>>");
    Assembly assembly = Load(host, libraries.OneOfImage, libraries.ContractsImage);
    (await Send(assembly, "App.ThingState+CreateThingActionSet+Action")).ShouldBe("0:Response { Name = widget }");
    (await Send(assembly, "App.ThingState+UpdateThingActionSet+Action")).ShouldBe("1:conflict");
    Log(assembly).ShouldBe(new[]
    {
      "CreateThingActionSet:OneOf`2<Ns.CreateThing+Response,Ns.SharedProblemDetails>",
      "UpdateThingActionSet:OneOf`2<Ns.UpdateThing+Response,Ns.SharedProblemDetails>",
    });
  }

  [Fact]
  public async Task ReferencedNames_WithInaccessibleDuplicatesInAnotherReference_StillResolve()
  {
    ReferencedLibraries libraries = ReferencedLibraries.Build();
    HostCompilation host = HostCompiler.Compile(
        "PatternShadowed",
        ReferencedOneOfHost,
        new SiblingGenerator(ThingActionSets),
        libraries.OneOf,
        libraries.Contracts,
        libraries.ContractState,
        HostCompiler.CompileLibrary("Shadow", ShadowLibrary));

    Problems(host).ShouldBeEmpty();
    host.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "TWM013");
    MediatorSource(host).ShouldContain("case global::App.ThingState.CreateThingActionSet.Action");
    Assembly assembly = Load(host, libraries.OneOfImage, libraries.ContractsImage);
    (await Send(assembly, "App.ThingState+CreateThingActionSet+Action")).ShouldBe("0:Response { Name = widget }");
  }

  [Fact]
  public async Task VoidTemplate_NamingAReferencedInterface_ClosesTheReferencedBehavior()
  {
    ReferencedLibraries libraries = ReferencedLibraries.Build();
    HostCompilation host = HostCompiler.Compile(
        "PatternReferencedAction",
        ReferencedActionHost,
        new SiblingGenerator(StateActionSets),
        libraries.Contracts,
        libraries.StateRuntime);

    Problems(host).ShouldBeEmpty();
    host.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "TWM013");
    string mediator = MediatorSource(host);
    mediator.ShouldContain("global::TimeWarp.State.StateTransactionBehavior<global::App.CounterState.CreateThingActionSet.Action,");
    mediator.ShouldNotContain("global::App.MediatorActionOnly<global::App.CounterState.CreateThingActionSet.Action,");
    Assembly assembly = Load(host, libraries.ContractsImage, libraries.StateRuntimeImage);

    await Send(assembly, "App.CounterState+CreateThingActionSet+Action");

    object entries = AssemblyLoadContext.GetLoadContext(assembly)!
        .Assemblies
        .Single(loaded => loaded.GetName().Name == "StateRuntime")
        .GetType("TimeWarp.State.StateLog", true)!
        .GetField("Entries")!
        .GetValue(null)!;
    ((IEnumerable<string>)entries).ShouldBe(new[] { "state:Action", "handled:CreateThing" });
  }

  [Fact]
  public void ReferencedContractWithoutResponse_IsTwm013_FromTheGenerator()
  {
    ReferencedLibraries libraries = ReferencedLibraries.Build();
    HostCompilation host = HostCompiler.Compile(
        "PatternReferencedNoResponse",
        ReferencedNoResponseHost,
        libraries.OneOf,
        libraries.Contracts,
        libraries.ContractState);

    Diagnostic twm013 = host.Diagnostics.Where(diagnostic => diagnostic.Id == "TWM013").ShouldHaveSingleItem();
    twm013.GetMessage().ShouldBe(
        "Request pattern 'State.ContractStateAttribute' on 'App.ThingState' cannot be expanded: request interface 'TimeWarp.Mediator.IRequest<OneOf.OneOf<Ns.NoResponse.Response, Ns.SharedProblemDetails>>' does not resolve to an interface");
    Span(twm013).ShouldBe("ContractState");
  }

  [Fact]
  public void ReferencedContract_MissingRequest_IsTwm013_NamingTheContract()
  {
    ReferencedLibraries libraries = ReferencedLibraries.Build();
    HostCompilation host = HostCompiler.Compile(
        "PatternReferencedMissing",
        ReferencedOneOfHost,
        libraries.OneOf,
        libraries.Contracts,
        libraries.ContractState);

    host.Diagnostics
        .Where(diagnostic => diagnostic.Id == "TWM013")
        .Select(diagnostic => diagnostic.GetMessage())
        .ShouldContain("'App.ThingState' declares generated request 'App.ThingState.CreateThingActionSet.Action' for contract 'Ns.CreateThing', which does not exist after generation");
  }

  private sealed class ReferencedLibraries
  {
    private ReferencedLibraries()
    {
      OneOfImage = HostCompiler.CompileLibraryImage("OneOf", OneOfLibrary);
      OneOf = MetadataReference.CreateFromImage(OneOfImage);
      ContractsImage = HostCompiler.CompileLibraryImage("Web.Contracts", WebContractsLibrary);
      Contracts = MetadataReference.CreateFromImage(ContractsImage);
      ContractState = MetadataReference.CreateFromImage(
          HostCompiler.CompileLibraryImage("State.Attributes", ContractStateLibrary, metadataOnly: true));
      StateRuntimeImage = HostCompiler.CompileLibraryImage("StateRuntime", StateRuntimeLibrary);
      StateRuntime = MetadataReference.CreateFromImage(StateRuntimeImage);
    }

    internal byte[] OneOfImage { get; }

    internal MetadataReference OneOf { get; }

    internal byte[] ContractsImage { get; }

    internal MetadataReference Contracts { get; }

    internal MetadataReference ContractState { get; }

    internal byte[] StateRuntimeImage { get; }

    internal MetadataReference StateRuntime { get; }

    internal static ReferencedLibraries Build() => new();
  }
}
