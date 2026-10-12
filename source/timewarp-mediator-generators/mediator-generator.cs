#region Purpose
// Incremental generator: build MessageGraph per compilation, render Mediator, scoped Sender/Publisher, and manifest.
#endregion

#region Design
// Generated code names TimeWarp.Mediator.IMediator and, under the Host profile, the MS.DI
// abstractions. When one cannot be resolved (for example a project that references only the
// Generators package) the generator emits nothing and reports TWM008 once, instead of a wall of
// CS0234/CS0246 inside .g.cs files.
// Pipeline: Options (equatable build properties) + Compilation -> MediatorModel (rendered sources
// and diagnostics, no symbols) -> Sources / Diagnostics. Each step has a TrackingNames entry so
// tests assert that an unrelated edit leaves the model unchanged and the AddSource step cached.
// The diagnostics output re-joins the Compilation only to put each span back on its syntax tree.
// This generator is the only reporter of TWM001-TWM003 in its host: the bundled analyzer sees
// TimeWarpMediatorGenerator=true and skips them, so each is reported once.
#endregion

namespace TimeWarp.Mediator.Generators;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
  private const string ContractsHint = "Reference the TimeWarp.Mediator.Contracts package";
  private const string DependencyInjectionHint =
      "Reference Microsoft.Extensions.DependencyInjection.Abstractions, or set TimeWarpMediatorProfile to Aot";

  private static readonly (string MetadataName, string Hint)[] RequiredTypes =
  {
        ("TimeWarp.Mediator.IMediator", ContractsHint),
    };

  private static readonly (string MetadataName, string Hint)[] HostRequiredTypes =
  {
        ("Microsoft.Extensions.DependencyInjection.IServiceCollection", DependencyInjectionHint),
        ("Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions", DependencyInjectionHint),
        ("Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions", DependencyInjectionHint),
    };

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValueProvider<MembershipOptions> options = context.AnalyzerConfigOptionsProvider
        .Select(static (provider, _) => MembershipOptions.Read(provider.GlobalOptions))
        .WithTrackingName(TrackingNames.Options);

    IncrementalValueProvider<MediatorModel> model = context.CompilationProvider
        .Combine(options)
        .Select(static (input, cancellationToken) => CreateModel(input.Left, input.Right, cancellationToken))
        .WithTrackingName(TrackingNames.Model);

    IncrementalValueProvider<EquatableArray<GeneratedSource>> sources = model
        .Select(static (value, _) => value.Sources)
        .WithTrackingName(TrackingNames.Sources);

    IncrementalValueProvider<EquatableArray<DiagnosticInfo>> diagnostics = model
        .Select(static (value, _) => value.Diagnostics)
        .WithTrackingName(TrackingNames.Diagnostics);

    context.RegisterSourceOutput(sources, static (productionContext, generated) =>
    {
      foreach (GeneratedSource source in generated)
      {
        productionContext.AddSource(source.HintName, SourceText.From(source.Text, Encoding.UTF8));
      }
    });

    context.RegisterSourceOutput(diagnostics.Combine(context.CompilationProvider), static (productionContext, input) =>
    {
      foreach (DiagnosticInfo diagnostic in input.Left)
      {
        productionContext.ReportDiagnostic(diagnostic.ToDiagnostic(input.Right));
      }
    });
  }

  private static MediatorModel CreateModel(Compilation compilation, MembershipOptions options, CancellationToken cancellationToken)
  {
    Membership membership = Membership.ForGenerator(compilation, options, cancellationToken);
    Diagnostic? missing = FindMissingRuntime(compilation, membership);
    if (missing is not null)
    {
      return new MediatorModel(
          EquatableArray<GeneratedSource>.Empty,
          new EquatableArray<DiagnosticInfo>(ImmutableArray.Create(DiagnosticInfo.From(missing))));
    }

    MessageGraph graph = MessageGraphBuilder.Build(compilation, membership, cancellationToken);
    AotProfile aotProfile = AotProfile.Analyze(compilation, graph);
    ImmutableArray<DiagnosticInfo> diagnostics = graph.Diagnostics
        .Concat(graph.GeneratorDiagnostics)
        .Concat(aotProfile.Diagnostics)
        .Select(DiagnosticInfo.From)
        .ToImmutableArray();

    cancellationToken.ThrowIfCancellationRequested();
    List<GeneratedSource> sources = new();
    MediatorEmitter.Emit(sources, graph, aotProfile);
    ManifestEmitter.Emit(sources, graph);

    return new MediatorModel(
        new EquatableArray<GeneratedSource>(sources.ToImmutableArray()),
        new EquatableArray<DiagnosticInfo>(diagnostics));
  }

  private static Diagnostic? FindMissingRuntime(Compilation compilation, Membership membership)
  {
    bool aot = string.Equals(membership.Profile, "Aot", StringComparison.OrdinalIgnoreCase)
        || string.Equals(membership.Profile, "Link", StringComparison.OrdinalIgnoreCase);
    IEnumerable<(string MetadataName, string Hint)> required = aot
        ? RequiredTypes
        : RequiredTypes.Concat(HostRequiredTypes);

    foreach ((string metadataName, string hint) in required)
    {
      if (compilation.GetTypeByMetadataName(metadataName) is null)
      {
        return Diagnostic.Create(DiagnosticDescriptors.MediatorRuntimeMissing, Location.None, metadataName, hint);
      }
    }

    return null;
  }
}