#if BENCHMARK_AOT
Environment.ExitCode = MediatorBenchmarks.AotBenchmarkRunner.Run();
#else
BenchmarkSwitcher.FromAssembly(typeof(MediatorBenchmarks.DispatchBenchmarks).Assembly).Run(args);
#endif
