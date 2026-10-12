#region Purpose
// The TimeWarpMediator* build properties, read once from GlobalOptions as an equatable value.
#endregion

#region Design
// Equatable so the generator's options step caches: an edit that leaves the build properties alone
// does not invalidate anything downstream of it. IsGeneratorHost comes from the generator package's
// props (TimeWarpMediatorGenerator=true): the bundled analyzer then knows the generator reports
// TWM001–TWM003 in this compilation and does not report them again.
#endregion

namespace TimeWarp.Mediator.Analyzers;

public sealed class MembershipOptions : IEquatable<MembershipOptions>
{
  public MembershipOptions(string profile, string generatedNamespace, bool isMsBuildMember, bool isGeneratorHost)
  {
    Profile = profile;
    GeneratedNamespace = generatedNamespace;
    IsMsBuildMember = isMsBuildMember;
    IsGeneratorHost = isGeneratorHost;
  }

  public string Profile { get; }

  public string GeneratedNamespace { get; }

  /// <summary>
  /// MSBuild TimeWarpMediatorAssembly=true.
  /// </summary>
  public bool IsMsBuildMember { get; }

  /// <summary>
  /// MSBuild TimeWarpMediatorGenerator=true: the generator runs in this compilation.
  /// </summary>
  public bool IsGeneratorHost { get; }

  public static MembershipOptions Read(AnalyzerConfigOptions globalOptions)
  {
    return new MembershipOptions(
        ReadProperty(globalOptions, Membership.ProfilePropertyName) ?? "Host",
        ReadProperty(globalOptions, Membership.NamespacePropertyName) ?? "TimeWarp.Mediator.Generated",
        IsTrue(ReadProperty(globalOptions, Membership.MsBuildPropertyName)),
        IsTrue(ReadProperty(globalOptions, Membership.GeneratorPropertyName)));
  }

  public bool Equals(MembershipOptions? other)
  {
    return other is not null
        && Profile == other.Profile
        && GeneratedNamespace == other.GeneratedNamespace
        && IsMsBuildMember == other.IsMsBuildMember
        && IsGeneratorHost == other.IsGeneratorHost;
  }

  public override bool Equals(object? obj) => Equals(obj as MembershipOptions);

  public override int GetHashCode()
  {
    unchecked
    {
      int hash = StringComparer.Ordinal.GetHashCode(Profile);
      hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(GeneratedNamespace);
      hash = (hash * 397) ^ (IsMsBuildMember ? 1 : 0);
      return (hash * 397) ^ (IsGeneratorHost ? 2 : 0);
    }
  }

  private static string? ReadProperty(AnalyzerConfigOptions globalOptions, string key)
  {
    return globalOptions.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
        ? value.Trim()
        : null;
  }

  private static bool IsTrue(string? value)
  {
    return value is not null
        && (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || value == "1");
  }
}