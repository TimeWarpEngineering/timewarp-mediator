#region Purpose
// Parser and tolerance checks for the comparison benchmark regression gate.
#endregion

#region Design
// The gate reads GitHub-flavored BenchmarkDotNet tables. These fixtures cover unit
// conversion, the 15.0 target table (not a measurement), and the TimeWarp* rule.
// The stored comparison file is checked for structure: it parses, the scoped, publish,
// State, and Architecture rows are present, and the tolerance sentence matches
// MeanTolerancePercent. Measured byte counts and the row count stay out of that test
// so --record can change them without failing the suite.
#endregion

using DevCli;
using System.IO;

namespace TimeWarp.Mediator.Tests;

public class BenchmarkGateTests
{
  private const string Sample =
      """
      ## MediatorBenchmarks.DispatchBenchmarks

      | Method                            | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
      |---------------------------------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
      | TimeWarpLegacyMakeGenericType     | 69.010 ns | 0.3917 ns | 0.2048 ns |  1.00 | 0.0044 |     224 B |        1.00 |
      | TimeWarpGeneratedInterfaceSend    | 1.5 µs    | 0.1 µs    | 0.1 µs    |  1.00 | 0.0100 |     240 B |        1.00 |
      | TimeWarpGeneratedObjectSend       | 50.5 ns   | 1 ns      | 1 ns      |  0.73 | 0.0040 |    1.5 KB |        0.75 |
      | MartinothamarGenerated            | 8.770 ns  | 0.0885 ns | 0.0463 ns |  0.13 | 0.0005 |      24 B |        0.11 |

      """;

  [Fact]
  public void Parse_should_read_mean_nanoseconds_and_allocated_bytes()
  {
    BenchmarkTable table = BenchmarkGate.Parse(Sample);

    table.Errors.ShouldBeEmpty();
    table.Rows["TimeWarpLegacyMakeGenericType"].MeanNanoseconds.ShouldBe(69.010, 0.0001);
    table.Rows["TimeWarpLegacyMakeGenericType"].AllocatedBytes.ShouldBe(224);
    table.Rows["TimeWarpGeneratedInterfaceSend"].MeanNanoseconds.ShouldBe(1500, 0.001);
    table.Rows["TimeWarpGeneratedObjectSend"].AllocatedBytes.ShouldBe(1536);
  }

  [Fact]
  public void Parse_should_ignore_the_unenforced_target_table()
  {
    BenchmarkTable table = BenchmarkGate.Parse(Sample + "\n" + BenchmarkGate.BaselineSuffix());

    table.Errors.ShouldBeEmpty();
    table.Rows.Count.ShouldBe(4);
    table.Rows.ContainsKey("TimeWarpGeneratedInterfaceSendUnit").ShouldBeFalse();
  }

  [Fact]
  public void Compare_should_pass_when_rows_stay_inside_tolerance()
  {
    BenchmarkTable baseline = BenchmarkGate.Parse(Sample);
    List<string> failures = BenchmarkGate.Compare(baseline, baseline);

    failures.ShouldBeEmpty();
  }

  [Fact]
  public void Compare_should_fail_when_mean_or_bytes_exceed_the_baseline()
  {
    BenchmarkTable baseline = BenchmarkGate.Parse(Sample);
    const string slower =
        """
        | Method                         | Mean    | Allocated |
        |------------------------------- |--------:|----------:|
        | TimeWarpLegacyMakeGenericType  | 120 ns  |     224 B |
        | TimeWarpGeneratedInterfaceSend | 1.5 µs  |     241 B |
        | TimeWarpGeneratedObjectSend    | 50.5 ns |    1.5 KB |
        | MartinothamarGenerated         | 800 ns  |    9.9 KB |
        """;

    List<string> failures = BenchmarkGate.Compare(baseline, BenchmarkGate.Parse(slower));

    failures.Count.ShouldBe(2);
    string.Join('\n', failures).ShouldContain("TimeWarpLegacyMakeGenericType");
    string.Join('\n', failures).ShouldContain("TimeWarpGeneratedInterfaceSend");
    string.Join('\n', failures).ShouldContain("241 B");
    string.Join('\n', failures).ShouldNotContain("MartinothamarGenerated");
  }

  [Fact]
  public void Compare_should_fail_when_a_gated_row_is_missing_or_new()
  {
    BenchmarkTable baseline = BenchmarkGate.Parse(Sample);
    const string missing =
        """
        | Method                         | Mean    | Allocated |
        |------------------------------- |--------:|----------:|
        | TimeWarpLegacyMakeGenericType  | 69 ns   |     224 B |
        | TimeWarpGeneratedObjectSend    | 50.5 ns |    1.5 KB |
        | TimeWarpGeneratedStatePipeline | 80 ns   |      96 B |
        | MartinothamarGenerated         | 8.770 ns|      24 B |
        """;

    List<string> failures = BenchmarkGate.Compare(baseline, BenchmarkGate.Parse(missing));

    string text = string.Join('\n', failures);
    text.ShouldContain("TimeWarpGeneratedInterfaceSend");
    text.ShouldContain("missing");
    text.ShouldContain("TimeWarpGeneratedStatePipeline");
    text.ShouldContain("no baseline row");
  }

  [Fact]
  public void Parse_should_treat_a_dash_allocated_cell_as_zero_bytes()
  {
    const string dash =
        """
        | Method | Mean | Allocated |
        | --- | --- | --- |
        | TimeWarpGeneratedInterfacePublish | 31.20 ns | - |
        """;

    BenchmarkTable table = BenchmarkGate.Parse(dash);

    table.Errors.ShouldBeEmpty();
    table.Rows["TimeWarpGeneratedInterfacePublish"].AllocatedBytes.ShouldBe(0);
  }

  [Fact]
  public void Parse_should_read_the_recorded_comparison_baseline()
  {
    string? repoRoot = Path.GetDirectoryName(typeof(BenchmarkGateTests).Assembly.Location);
    while (repoRoot is not null && !File.Exists(Path.Combine(repoRoot, "timewarp-mediator.slnx")))
    {
      repoRoot = Path.GetDirectoryName(repoRoot);
    }

    repoRoot.ShouldNotBeNull();
    string text = File.ReadAllText(Path.Combine(repoRoot, "documentation", "benchmarks", "comparison-baseline.md"));
    BenchmarkTable table = BenchmarkGate.Parse(text);

    table.Errors.ShouldBeEmpty();
    string[] addedRows =
    [
      "TimeWarpGeneratedScopedSend",
      "TimeWarpGeneratedInterfacePublish",
      "TimeWarpGeneratedScopedPublish",
      "TimeWarpGeneratedStatePipeline",
      "TimeWarpGeneratedArchitectureSend"
    ];
    foreach (string method in addedRows)
    {
      table.Rows.ContainsKey(method).ShouldBeTrue($"{method} is missing from the recorded baseline");
      BenchmarkGate.IsGated(method).ShouldBeTrue();
    }

    text.ShouldContain($"at most {BenchmarkGate.MeanTolerancePercent}%");
  }

  [Fact]
  public void Parse_should_reject_a_duplicate_method()
  {
    const string duplicate =
        """
        | Method | Mean | Allocated |
        |------- |-----:|----------:|
        | TimeWarpGeneratedInterfaceSend | 10 ns | 24 B |
        | TimeWarpGeneratedInterfaceSend | 11 ns | 24 B |
        """;

    BenchmarkTable table = BenchmarkGate.Parse(duplicate);

    table.Errors.ShouldContain(error => error.Contains("more than once", StringComparison.Ordinal));
  }
}
