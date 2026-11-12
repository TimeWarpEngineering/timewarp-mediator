#region Purpose
// Benchmark entry point: BenchmarkDotNet switcher, or the AOT runner when BENCHMARK_AOT is set.
#endregion

#if BENCHMARK_AOT
Environment.ExitCode = MediatorBenchmarks.AotBenchmarkRunner.Run();
#else
BenchmarkSwitcher.FromAssembly(typeof(MediatorBenchmarks.DispatchBenchmarks).Assembly).Run(args);
#endif
