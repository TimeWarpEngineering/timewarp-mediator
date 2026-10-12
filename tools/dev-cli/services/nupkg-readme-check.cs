#region Purpose
// Verify each nupkg ships its own package readme with nuget.org-safe links
#endregion
#region Design
// nuget.org renders the packed readme.md but cannot resolve relative links, so
// every link must be absolute (http/https/mailto) or an in-page #anchor. The
// first heading must name the package, which catches a project that packs the
// repository readme (or a sibling's) instead of its own source/<project>/readme.md.
#endregion

namespace DevCli;

internal static class NupkgReadmeCheck
{
  internal const string ReadmeEntry = "readme.md";

  // Inline link or image target: ](target) — capture the target up to whitespace or ')'.
  private static readonly Regex LinkTargetPattern = new(
    @"\]\((?<target>[^)\s]+)",
    RegexOptions.CultureInvariant);

  private static readonly Regex AllowedTargetPattern = new(
    @"^(https?://|mailto:|#)",
    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

  internal static IReadOnlyList<string> FindProblems(string nupkgPath, string packageId)
  {
    List<string> problems = [];
    using ZipArchive archive = ZipFile.OpenRead(nupkgPath);
    ZipArchiveEntry? entry = archive.GetEntry(ReadmeEntry);
    if (entry is null)
    {
      problems.Add($"no {ReadmeEntry}");
      return problems;
    }

    string text;
    using (StreamReader reader = new(entry.Open()))
    {
      text = reader.ReadToEnd();
    }

    string? firstHeading = text
      .Split('\n')
      .Select(line => line.TrimEnd('\r'))
      .FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal));
    if (firstHeading != $"# {packageId}")
    {
      problems.Add($"{ReadmeEntry} first heading is '{firstHeading}', expected '# {packageId}'");
    }

    foreach (Match match in LinkTargetPattern.Matches(text))
    {
      string target = match.Groups["target"].Value;
      if (!AllowedTargetPattern.IsMatch(target))
      {
        problems.Add($"{ReadmeEntry} has a relative link '{target}' (nuget.org cannot resolve it)");
      }
    }

    return problems;
  }
}
