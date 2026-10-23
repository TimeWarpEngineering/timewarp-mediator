#region Purpose
// Parse BenchmarkDotNet GitHub tables and compare them to a recorded baseline.
#endregion

#region Design
// The baseline file is the source of truth, including its generated tolerance section.
// Only tables with Method, Mean, and Allocated columns are measurements. The 15.0 target
// table uses different headers so a re-record cannot turn those goals into a gate.
// Rows whose names start with TimeWarp are gated. MartinothamarGenerated is the external
// reference and is not. Mean may rise by MeanToleranceRatio (half, because an 8-iteration
// job records a 99.9% error bar of about 20 to 30 percent of the mean on the interface
// and State rows). Allocated bytes may not rise. A MemoryDiagnoser dash is 0 B.
#endregion

using System.Globalization;

namespace DevCli;

internal readonly record struct BenchmarkRow(double MeanNanoseconds, long AllocatedBytes);

internal sealed class BenchmarkTable
{
  public Dictionary<string, BenchmarkRow> Rows { get; } = new(StringComparer.Ordinal);

  public List<string> Errors { get; } = [];
}

internal static class BenchmarkGate
{
  internal const double MeanToleranceRatio = 0.50;

  internal static int MeanTolerancePercent => (int)Math.Round(MeanToleranceRatio * 100);

  internal static bool IsGated(string method) =>
      method.StartsWith("TimeWarp", StringComparison.Ordinal);

  internal static BenchmarkTable Parse(string markdown)
  {
    BenchmarkTable table = new();
    ParseInto(table, markdown);
    return table;
  }

  internal static BenchmarkTable ParseMany(IEnumerable<string> markdownDocuments)
  {
    BenchmarkTable table = new();
    foreach (string markdown in markdownDocuments)
    {
      ParseInto(table, markdown);
    }

    return table;
  }

  internal static List<string> Compare(BenchmarkTable baseline, BenchmarkTable measured)
  {
    List<string> failures = [];
    if (baseline.Errors.Count > 0 || measured.Errors.Count > 0)
    {
      foreach (string error in baseline.Errors)
      {
        failures.Add("Baseline: " + error);
      }

      foreach (string error in measured.Errors)
      {
        failures.Add("Run: " + error);
      }

      return failures;
    }

    bool anyGated = false;
    foreach (KeyValuePair<string, BenchmarkRow> pair in baseline.Rows)
    {
      if (!IsGated(pair.Key))
      {
        continue;
      }

      anyGated = true;
      if (!measured.Rows.TryGetValue(pair.Key, out BenchmarkRow actual))
      {
        failures.Add($"{pair.Key} is in the baseline and missing from this run.");
        continue;
      }

      double limit = pair.Value.MeanNanoseconds * (1d + MeanToleranceRatio);
      if (actual.MeanNanoseconds > limit)
      {
        failures.Add(
            $"{pair.Key} mean {FormatNanoseconds(actual.MeanNanoseconds)} exceeds baseline {FormatNanoseconds(pair.Value.MeanNanoseconds)} by more than {MeanTolerancePercent}% (limit {FormatNanoseconds(limit)}).");
      }

      if (actual.AllocatedBytes > pair.Value.AllocatedBytes)
      {
        failures.Add(
            $"{pair.Key} allocated {actual.AllocatedBytes} B, above the baseline {pair.Value.AllocatedBytes} B.");
      }
    }

    foreach (string method in measured.Rows.Keys)
    {
      if (IsGated(method) && !baseline.Rows.ContainsKey(method))
      {
        failures.Add($"{method} is gated and has no baseline row. Re-record with --record.");
      }
    }

    if (!anyGated)
    {
      failures.Add("The baseline has no TimeWarp* rows to gate.");
    }

    return failures;
  }

  internal static string BaselineSuffix()
  {
    int percent = MeanTolerancePercent;
    return
        "## Regression gate\n"
        + "\n"
        + "`./bin/dev benchmark --project comparison --gate` re-runs the comparison project and fails when a row whose name starts with `TimeWarp` exceeds this file.\n"
        + "\n"
        + $"- Mean time may be at most {percent}% above the recorded mean.\n"
        + "- Allocated bytes may not exceed the recorded bytes.\n"
        + "- `MartinothamarGenerated` is the external reference and is not gated.\n"
        + "- `--filter` cannot be combined with `--gate`. `--record` and `--gate` cannot be combined.\n"
        + "- Re-record after an intentional change: `./bin/dev benchmark --project comparison --record`.\n"
        + "- The gate is not part of `./bin/dev workflow`. A shared runner is too noisy for this number.\n"
        + "\n"
        + "## 15.0 targets (not enforced)\n"
        + "\n"
        + "The gate above does not apply this table. It is the dispatcher goal for 15.0.\n"
        + "\n"
        + "| Row | Time target | Allocation target |\n"
        + "| --- | --- | --- |\n"
        + "| TimeWarpGeneratedInterfaceSend | ≤ 1.5× the MartinothamarGenerated mean | ≤ 24 B |\n"
        + "| TimeWarpGeneratedInterfaceSendUnit | — | 0 B |\n"
        + "| TimeWarpGeneratedStatePipeline | — | 0 B apart from the handler (the handler returns a completed ValueTask, so the row target is 0 B) |\n";
  }

  private static void ParseInto(BenchmarkTable table, string markdown)
  {
    string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
    bool inFence = false;
    int methodIndex = -1;
    int meanIndex = -1;
    int allocatedIndex = -1;
    bool inTable = false;

    foreach (string rawLine in lines)
    {
      string line = rawLine.Trim();
      if (line.StartsWith("```", StringComparison.Ordinal))
      {
        inFence = !inFence;
        inTable = false;
        continue;
      }

      if (inFence)
      {
        continue;
      }

      if (line.Length == 0 || !line.Contains('|', StringComparison.Ordinal))
      {
        inTable = false;
        continue;
      }

      string[] cells = line.Split('|');
      if (IsSeparator(cells))
      {
        continue;
      }

      if (!inTable && TryReadHeader(cells, out methodIndex, out meanIndex, out allocatedIndex))
      {
        inTable = true;
        continue;
      }

      if (!inTable)
      {
        continue;
      }

      if (cells.Length <= Math.Max(methodIndex, Math.Max(meanIndex, allocatedIndex)))
      {
        inTable = false;
        continue;
      }

      string method = cells[methodIndex].Trim();
      if (method.Length == 0)
      {
        continue;
      }

      if (!TryParseTime(cells[meanIndex], out double meanNanoseconds))
      {
        table.Errors.Add($"Method {method} has an unreadable Mean cell '{cells[meanIndex].Trim()}'.");
        continue;
      }

      if (!TryParseBytes(cells[allocatedIndex], out long allocatedBytes))
      {
        table.Errors.Add($"Method {method} has an unreadable Allocated cell '{cells[allocatedIndex].Trim()}'.");
        continue;
      }

      if (!table.Rows.TryAdd(method, new BenchmarkRow(meanNanoseconds, allocatedBytes)))
      {
        table.Errors.Add($"Method {method} appears more than once.");
      }
    }
  }

  private static bool TryReadHeader(string[] cells, out int methodIndex, out int meanIndex, out int allocatedIndex)
  {
    methodIndex = -1;
    meanIndex = -1;
    allocatedIndex = -1;
    for (int index = 0; index < cells.Length; index++)
    {
      string header = cells[index].Trim();
      if (header.Equals("Method", StringComparison.Ordinal))
      {
        methodIndex = index;
      }
      else if (header.Equals("Mean", StringComparison.Ordinal))
      {
        meanIndex = index;
      }
      else if (header.Equals("Allocated", StringComparison.Ordinal))
      {
        allocatedIndex = index;
      }
    }

    return methodIndex >= 0 && meanIndex >= 0 && allocatedIndex >= 0;
  }

  private static bool IsSeparator(string[] cells)
  {
    bool any = false;
    foreach (string cell in cells)
    {
      string trimmed = cell.Trim();
      if (trimmed.Length == 0)
      {
        continue;
      }

      any = true;
      foreach (char character in trimmed)
      {
        if (character != '-' && character != ':')
        {
          return false;
        }
      }
    }

    return any;
  }

  private static bool TryParseTime(string cell, out double nanoseconds)
  {
    nanoseconds = 0;
    if (!TrySplitMeasurement(cell, out double value, out string unit))
    {
      return false;
    }

    switch (unit)
    {
      case "ns":
        nanoseconds = value;
        return true;
      case "us":
      case "µs":
      case "μs":
        nanoseconds = value * 1_000d;
        return true;
      case "ms":
        nanoseconds = value * 1_000_000d;
        return true;
      case "s":
        nanoseconds = value * 1_000_000_000d;
        return true;
      default:
        return false;
    }
  }

  private static bool TryParseBytes(string cell, out long bytes)
  {
    bytes = 0;
    if (cell.Trim() == "-")
    {
      return true;
    }

    if (!TrySplitMeasurement(cell, out double value, out string unit))
    {
      return false;
    }

    double scale = unit switch
    {
      "B" => 1d,
      "KB" => 1024d,
      "MB" => 1024d * 1024d,
      "GB" => 1024d * 1024d * 1024d,
      _ => double.NaN
    };
    if (double.IsNaN(scale))
    {
      return false;
    }

    bytes = (long)Math.Round(value * scale, MidpointRounding.AwayFromZero);
    return true;
  }

  private static bool TrySplitMeasurement(string cell, out double value, out string unit)
  {
    value = 0;
    unit = string.Empty;
    string trimmed = cell.Trim().Replace(",", string.Empty, StringComparison.Ordinal);
    if (trimmed.Length == 0 || trimmed == "-")
    {
      return false;
    }

    int split = trimmed.LastIndexOf(' ');
    if (split <= 0 || split == trimmed.Length - 1)
    {
      return false;
    }

    unit = trimmed[(split + 1)..];
    return double.TryParse(
        trimmed[..split],
        NumberStyles.Float,
        CultureInfo.InvariantCulture,
        out value);
  }

  private static string FormatNanoseconds(double nanoseconds) =>
      nanoseconds.ToString("0.###", CultureInfo.InvariantCulture) + " ns";
}
