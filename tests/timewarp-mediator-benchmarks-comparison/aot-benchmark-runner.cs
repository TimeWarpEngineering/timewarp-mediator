#region Purpose
// Times the TimeWarp rows of DispatchBenchmarks in a PublishAot build.
#endregion

#region Design
// BenchmarkDotNet's NativeAotToolchain hosts BenchmarkDotNet, and that package is not
// in the PublishAot compilation. This runner calls the same benchmark methods through
// an Action, so the mean includes that delegate invoke. Each result is stored in a
// generic static field and is not boxed. The numbers are not the regression gate.
#endregion

#if BENCHMARK_AOT
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace MediatorBenchmarks;

internal static class AotBenchmarkRunner
{
  private const int Warmup = 20_000;
  private const int Iterations = 200_000;

  public static int Run()
  {
    DispatchBenchmarks benchmarks = new();
    benchmarks.Setup();
    StringBuilder table = new();
    table.AppendLine("## MediatorBenchmarks.DispatchBenchmarks");
    table.AppendLine();
    table.AppendLine("PublishAot runner. Not BenchmarkDotNet. Not gated.");
    table.AppendLine("Each iteration invokes the row through a delegate, so the mean includes that call.");
    table.AppendLine("The result is stored in a generic static field and is not boxed.");
    table.AppendLine($"Warmup {Warmup.ToString(CultureInfo.InvariantCulture)}, iterations {Iterations.ToString(CultureInfo.InvariantCulture)}.");
    table.AppendLine();
    table.AppendLine("| Method | Mean | Allocated |");
    table.AppendLine("| --- | ---: | ---: |");

    Measure(table, "TimeWarpGeneratedMonomorphicSend", () => Keep(benchmarks.TimeWarpGeneratedMonomorphicSend().GetAwaiter().GetResult()));
    Measure(table, "TimeWarpGeneratedInterfaceSend", () => Keep(benchmarks.TimeWarpGeneratedInterfaceSend().GetAwaiter().GetResult()));
    Measure(table, "TimeWarpGeneratedObjectSend", () => Keep(benchmarks.TimeWarpGeneratedObjectSend().GetAwaiter().GetResult()));
    Measure(table, "TimeWarpGeneratedInterfaceSendUnit", () => benchmarks.TimeWarpGeneratedInterfaceSendUnit().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedMonomorphicSendUnit", () => Keep(benchmarks.TimeWarpGeneratedMonomorphicSendUnit().GetAwaiter().GetResult()));
    Measure(table, "TimeWarpCallSiteInliningPrototype", () => Keep(benchmarks.TimeWarpCallSiteInliningPrototype().GetAwaiter().GetResult()));
    Measure(table, "TimeWarpGeneratedScopedSend", () => Keep(benchmarks.TimeWarpGeneratedScopedSend().GetAwaiter().GetResult()));
    Measure(table, "TimeWarpGeneratedInterfacePublish", () => benchmarks.TimeWarpGeneratedInterfacePublish().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedScopedPublish", () => benchmarks.TimeWarpGeneratedScopedPublish().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedStatePipeline", () => benchmarks.TimeWarpGeneratedStatePipeline().GetAwaiter().GetResult());
    Measure(table, "TimeWarpGeneratedArchitectureSend", () => Keep(benchmarks.TimeWarpGeneratedArchitectureSend().GetAwaiter().GetResult()));

    Console.Write(table);
    return 0;
  }

  // NoInlining keeps the store visible to the caller. A generic static field holds T
  // without boxing a struct into object.
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void Keep<T>(T value)
  {
    Consumed<T>.Value = value;
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

  private static class Consumed<T>
  {
    public static T? Value;
  }
}
#endif
