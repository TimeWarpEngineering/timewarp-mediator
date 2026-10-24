#region Purpose
// Equatable generator output: rendered sources plus diagnostics, with no Compilation or ISymbol inside.
#endregion

#region Design
// The model step is a pure function of equatable host facts, referenced-assembly facts, and
// membership options. It does not take a Compilation: that input would re-run the step on every
// edit (Unchanged) instead of skipping it (Cached). The step returns strings and value records,
// so an edit that does not change the generated text or the diagnostics compares equal and the
// AddSource step is cached: no re-emission and no re-parse of the generated trees.
// Diagnostics keep their final message text and a file path + span instead of a Location (which
// holds a SyntaxTree). The diagnostics output re-attaches that span to the matching tree.
#endregion

namespace TimeWarp.Mediator.Generators;

internal sealed record MediatorModel(EquatableArray<GeneratedSource> Sources, EquatableArray<DiagnosticInfo> Diagnostics);

internal sealed record GeneratedSource(string HintName, string Text);

internal sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
  public static LocationInfo? From(Location location)
  {
    return location.IsInSource && location.SourceTree is not null
        ? new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span)
        : null;
  }

  public Location ToLocation(Compilation compilation)
  {
    SyntaxTree? match = null;
    foreach (SyntaxTree tree in compilation.SyntaxTrees)
    {
      if (tree.FilePath != FilePath)
      {
        continue;
      }

      if (match is not null)
      {
        // Two trees share the path (in-memory compilations): fall back to a path location.
        return Location.Create(FilePath, Span, LineSpan);
      }

      match = tree;
    }

    return match is not null && Span.End <= match.Length
        ? Location.Create(match, Span)
        : Location.Create(FilePath, Span, LineSpan);
  }
}

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    string Message,
    DiagnosticSeverity Severity,
    int WarningLevel,
    LocationInfo? Location)
{
  public static DiagnosticInfo From(Diagnostic diagnostic)
  {
    return new DiagnosticInfo(
        diagnostic.Descriptor,
        diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture),
        diagnostic.Severity,
        diagnostic.WarningLevel,
        LocationInfo.From(diagnostic.Location));
  }

  public Diagnostic ToDiagnostic(Compilation compilation)
  {
    return Diagnostic.Create(
        Descriptor.Id,
        Descriptor.Category,
        Message,
        Severity,
        Descriptor.DefaultSeverity,
        Descriptor.IsEnabledByDefault,
        WarningLevel,
        Descriptor.Title,
        Descriptor.Description,
        Descriptor.HelpLinkUri,
        Location?.ToLocation(compilation) ?? Microsoft.CodeAnalysis.Location.None,
        additionalLocations: null,
        customTags: Descriptor.CustomTags);
  }
}