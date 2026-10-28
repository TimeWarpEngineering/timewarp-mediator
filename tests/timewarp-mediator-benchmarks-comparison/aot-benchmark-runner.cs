#region Purpose
// Times the TimeWarp rows of DispatchBenchmarks in a PublishAot build.
#endregion

#region Design
// BenchmarkDotNet's NativeAotToolchain hosts BenchmarkDotNet, and that package is not
// in the PublishAot compilation. This runner calls the same benchmark methods.
// The numbers are not the regression gate.
#endregion

#if BENCHMARK_AOT
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace MediatorBenchmarks;

internal static class AotBenchmarkRunner
{
  private const int Warmup = 20_000;
  private const int Iterations = 200_000;
  private static object? Consumed;

  public static int Run()
  {
    DispatchBenchmarks benchmarks = new();
    benchmarks.Setup();
    StringBuilder table = new();
    table.AppendLine("## MediatorBenchmarks.DispatchBenchmarks");
    table.AppendLine();
    table.AppendLine("PublishAot runner. Not BenchmarkDotNet. Not gated.");
    table.AppendLine($"Warmup {Warmup.ToString(CultureInfo.InvariantCulture)}, iterations {Iterations.ToString(CultureInfo.InvariantCulture)}.");
    table.AppendLine();
    table.AppendLine("| Method | Mean | Allocated |");
    table.AppendLine("| --- | ---: | ---: |");

    Measure(table, "TimeWarpGeneratedMonomorphicSend", () => Keep(benchmarks.TimeWarpGeneratedMonomorphicSend()));
    Measure(table, "TimeWarpGeneratedInterfaceSend", () => Keep(benchmarks.TimeWarpGeneratedInterfaceSend()));
    Measure(table, "TimeWarpGeneratedObjectSend", () => Keep(benchmarks.TimeWarpGeneratedObjectSend()));
    Measure(table, "TimeWarpGeneratedInterfaceSendUnit", () => benchmarks.TimeWarpGeneratedInterfaceSendUnit().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedMonomorphicSendUnit", () => Keep(benchmarks.TimeWarpGeneratedMonomorphicSendUnit()));
    Measure(table, "TimeWarpCallSiteInliningPrototype", () => Keep(benchmarks.TimeWarpCallSiteInliningPrototype()));
    Measure(table, "TimeWarpGeneratedScopedSend", () => Keep(benchmarks.TimeWarpGeneratedScopedSend()));
    Measure(table, "TimeWarpGeneratedInterfacePublish", () => benchmarks.TimeWarpGeneratedInterfacePublish().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedScopedPublish", () => benchmarks.TimeWarpGeneratedScopedPublish().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedStatePipeline", () => benchmarks.TimeWarpGeneratedStatePipeline().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedArchitectureSend", () => Keep(benchmarks.TimeWarpGeneratedArchitectureSend()));

    Console.Write(table);
    GC.KeepAlive(Consumed);
    return 0;
  }

  private static void Keep<T>(ValueTask<T> pending)
  {
    Consumed = pending.GetAwaiter().GetResult();
  }

  private static void Keep(Task<object?> pending)
  {
    Consumed = pending.GetAwaiter().GetResult();
  }

  private static void Measure(StringBuilder table, string name, Action invoke)
  {
    for (int index = 0; index < Warmup; index++)
    {
      invoke();
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    long started = Stopwatch.GetTimestamp();
    for (int index = 0; index < Iterations; index++)
    {
      invoke();
    }

    long elapsed = Stopwatch.GetTimestamp() - started;
    long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
    double mean = elapsed * (1_000_000_000d / Stopwatch.Frequency) / Iterations;
    long bytes = allocated / Iterations;
    table.Append("| ");
    table.Append(name);
    table.Append(" | ");
    table.Append(mean.ToString("0.000", CultureInfo.InvariantCulture));
    table.Append(" ns | ");
    table.Append(bytes.ToString(CultureInfo.InvariantCulture));
    table.AppendLine(" B |");
  }
}
#endif
