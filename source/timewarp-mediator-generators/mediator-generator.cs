#region Purpose
// Incremental generator: equatable host and reference facts, then Mediator, scoped Sender/Publisher, and manifest.
#endregion

#region Design
// Generated code names TimeWarp.Mediator.IMediator and the MS.DI abstractions. When one cannot
// be resolved (for example a project that references only the Generators package) the generator
// emits nothing and reports TWM008 once, instead of a wall of CS0234/CS0246 inside .g.cs files.
// Pipeline: Options, host syntax candidates, and referenced assemblies (MetadataReferencesProvider)
// are equatable records. The model step is a pure function of those three, so an unrelated edit
// leaves it Cached. ForAttributeWithMetadataName is not used: that API combines CompilationProvider,
// which re-runs the transform on every edit (Unchanged, not Cached). Syntax providers with a narrow
// predicate are the host candidate source. The diagnostics output re-joins the Compilation only to
// put each span back on its syntax tree.
// This generator is the only reporter of TWM001-TWM003 in its host: the analyzer sees
// TimeWarpMediatorGenerator=true and skips them, so each is reported once.
#endregion

namespace TimeWarp.Mediator.Generators;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValueProvider<MembershipOptions> options = context.AnalyzerConfigOptionsProvider
        .Select(static (provider, _) => MembershipOptions.Read(provider.GlobalOptions))
        .WithTrackingName(TrackingNames.Options);

    IncrementalValueProvider<HostInput> host = context.SyntaxProvider
        .CreateSyntaxProvider(CandidateExtractor.IsTypeCandidate, CandidateExtractor.ExtractType)
        .Where(static fact => fact is not null)
        .Select(static (fact, _) => fact!)
        .Collect()
        .Combine(context.SyntaxProvider
            .CreateSyntaxProvider(CandidateExtractor.IsBehaviorAttribute, CandidateExtractor.ExtractBehavior)
            .Where(static behavior => behavior is not null)
            .Select(static (behavior, _) => behavior!)
            .Collect())
        .Select(static (pair, _) => new HostInput(
            new EquatableArray<TypeFact>(pair.Left),
            new EquatableArray<BehaviorFact>(pair.Right)))
        .WithTrackingName(TrackingNames.HostCandidates);

    IncrementalValueProvider<ReferencedInput> referenced = context.MetadataReferencesProvider
        .Collect()
        .Combine(options)
        .Select(static (pair, cancellationToken) => ReferencedAssemblyCandidates.Read(pair.Left, pair.Right, cancellationToken))
        .WithTrackingName(TrackingNames.ReferencedAssemblies);

    IncrementalValueProvider<MediatorModel> model = host
        .Combine(referenced)
        .Combine(options)
        .Select(static (pair, cancellationToken) => CandidateGraph.Create(pair.Left.Left, pair.Left.Right, pair.Right, cancellationToken))
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

}