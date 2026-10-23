// Validating benchmarks:
// ***** BenchmarkRunner: Start   *****
// ***** Found 8 benchmark(s) in total *****
// ***** Building 1 exe(s) in Parallel: Start   *****
// start dotnet  restore --nodeReuse:false /p:UseSharedCompilation=false /p:Deterministic=true /p:Optimize=true /p:ArtifactsPath="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/" /p:OutDir="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0/" /p:OutputPath="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0/" /p:PublishDir="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/publish/" in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1
// command took 1.78 sec and exited with 0
// start dotnet  build -c Release --no-restore --nodeReuse:false /p:UseSharedCompilation=false /p:Deterministic=true /p:Optimize=true /p:ArtifactsPath="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/" /p:OutDir="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0/" /p:OutputPath="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0/" /p:PublishDir="/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/publish/" --output "/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0/" in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1
// command took 10.89 sec and exited with 0
// ***** Done, took 00:00:12 (12.73 sec)   *****
// Found 8 benchmarks:
//   DispatchBenchmarks.TimeWarpLegacyMakeGenericType: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.TimeWarpGeneratedMonomorphicSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.TimeWarpGeneratedInterfaceSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.TimeWarpGeneratedObjectSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.TimeWarpGeneratedInterfaceSendUnit: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.TimeWarpGeneratedMonomorphicSendUnit: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.TimeWarpCallSiteInliningPrototype: Job-LGHQEI(IterationCount=8, WarmupCount=3)
//   DispatchBenchmarks.MartinothamarGenerated: Job-LGHQEI(IterationCount=8, WarmupCount=3)

// **************************
// Benchmark: DispatchBenchmarks.TimeWarpLegacyMakeGenericType: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpLegacyMakeGenericType --job "IterationCount=8, WarmupCount=3" --benchmarkId 0 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 182625.00 ns, 182.6250 us/op
WorkloadJitting  1: 1 op, 6764725.00 ns, 6.7647 ms/op

OverheadJitting  2: 16 op, 494801.00 ns, 30.9251 us/op
WorkloadJitting  2: 16 op, 1144772.00 ns, 71.5483 us/op

WorkloadPilot    1: 16 op, 167455.00 ns, 10.4659 us/op
WorkloadPilot    2: 32 op, 47642.00 ns, 1.4888 us/op
WorkloadPilot    3: 64 op, 100445.00 ns, 1.5695 us/op
WorkloadPilot    4: 128 op, 85075.00 ns, 664.6484 ns/op
WorkloadPilot    5: 256 op, 220348.00 ns, 860.7344 ns/op
WorkloadPilot    6: 512 op, 263582.00 ns, 514.8086 ns/op
WorkloadPilot    7: 1024 op, 477016.00 ns, 465.8359 ns/op
WorkloadPilot    8: 2048 op, 985273.00 ns, 481.0903 ns/op
WorkloadPilot    9: 4096 op, 3592014.00 ns, 876.9565 ns/op
WorkloadPilot   10: 8192 op, 5325420.00 ns, 650.0757 ns/op
WorkloadPilot   11: 16384 op, 8565010.00 ns, 522.7667 ns/op
WorkloadPilot   12: 32768 op, 16227638.00 ns, 495.2282 ns/op
WorkloadPilot   13: 65536 op, 38816809.00 ns, 592.2975 ns/op
WorkloadPilot   14: 131072 op, 68416275.00 ns, 521.9748 ns/op
WorkloadPilot   15: 262144 op, 130431739.00 ns, 497.5576 ns/op
WorkloadPilot   16: 524288 op, 45820701.00 ns, 87.3961 ns/op
WorkloadPilot   17: 1048576 op, 105926496.00 ns, 101.0194 ns/op
WorkloadPilot   18: 2097152 op, 303726102.00 ns, 144.8279 ns/op
WorkloadPilot   19: 4194304 op, 282073283.00 ns, 67.2515 ns/op
WorkloadPilot   20: 8388608 op, 606361780.00 ns, 72.2840 ns/op

OverheadWarmup   1: 8388608 op, 23436587.00 ns, 2.7939 ns/op
OverheadWarmup   2: 8388608 op, 24206259.00 ns, 2.8856 ns/op
OverheadWarmup   3: 8388608 op, 23852985.00 ns, 2.8435 ns/op
OverheadWarmup   4: 8388608 op, 24023146.00 ns, 2.8638 ns/op
OverheadWarmup   5: 8388608 op, 23733683.00 ns, 2.8293 ns/op

OverheadActual   1: 8388608 op, 23331682.00 ns, 2.7814 ns/op
OverheadActual   2: 8388608 op, 23131574.00 ns, 2.7575 ns/op
OverheadActual   3: 8388608 op, 23695159.00 ns, 2.8247 ns/op
OverheadActual   4: 8388608 op, 23997226.00 ns, 2.8607 ns/op
OverheadActual   5: 8388608 op, 23989710.00 ns, 2.8598 ns/op
OverheadActual   6: 8388608 op, 23354638.00 ns, 2.7841 ns/op
OverheadActual   7: 8388608 op, 23325371.00 ns, 2.7806 ns/op
OverheadActual   8: 8388608 op, 23205258.00 ns, 2.7663 ns/op
OverheadActual   9: 8388608 op, 23746107.00 ns, 2.8308 ns/op
OverheadActual  10: 8388608 op, 23438570.00 ns, 2.7941 ns/op
OverheadActual  11: 8388608 op, 23578973.00 ns, 2.8108 ns/op
OverheadActual  12: 8388608 op, 16893475.00 ns, 2.0139 ns/op
OverheadActual  13: 8388608 op, 17544374.00 ns, 2.0915 ns/op
OverheadActual  14: 8388608 op, 17122717.00 ns, 2.0412 ns/op
OverheadActual  15: 8388608 op, 17550557.00 ns, 2.0922 ns/op
OverheadActual  16: 8388608 op, 18972287.00 ns, 2.2617 ns/op
OverheadActual  17: 8388608 op, 19508390.00 ns, 2.3256 ns/op
OverheadActual  18: 8388608 op, 27192173.00 ns, 3.2416 ns/op
OverheadActual  19: 8388608 op, 21336984.00 ns, 2.5436 ns/op
OverheadActual  20: 8388608 op, 16861981.00 ns, 2.0101 ns/op

WorkloadWarmup   1: 8388608 op, 795224323.00 ns, 94.7981 ns/op
WorkloadWarmup   2: 8388608 op, 619758031.00 ns, 73.8809 ns/op
WorkloadWarmup   3: 8388608 op, 664730964.00 ns, 79.2421 ns/op

// BeforeActualRun
WorkloadActual   1: 8388608 op, 920341163.00 ns, 109.7132 ns/op
WorkloadActual   2: 8388608 op, 504771415.00 ns, 60.1734 ns/op
WorkloadActual   3: 8388608 op, 572894557.00 ns, 68.2944 ns/op
WorkloadActual   4: 8388608 op, 645883804.00 ns, 76.9953 ns/op
WorkloadActual   5: 8388608 op, 567090355.00 ns, 67.6024 ns/op
WorkloadActual   6: 8388608 op, 501393301.00 ns, 59.7707 ns/op
WorkloadActual   7: 8388608 op, 549174868.00 ns, 65.4667 ns/op
WorkloadActual   8: 8388608 op, 651630861.00 ns, 77.6805 ns/op

// AfterActualRun
WorkloadResult   1: 8388608 op, 481506100.50 ns, 57.4000 ns/op
WorkloadResult   2: 8388608 op, 549629242.50 ns, 65.5209 ns/op
WorkloadResult   3: 8388608 op, 622618489.50 ns, 74.2219 ns/op
WorkloadResult   4: 8388608 op, 543825040.50 ns, 64.8290 ns/op
WorkloadResult   5: 8388608 op, 478127986.50 ns, 56.9973 ns/op
WorkloadResult   6: 8388608 op, 525909553.50 ns, 62.6933 ns/op
WorkloadResult   7: 8388608 op, 628365546.50 ns, 74.9070 ns/op
// GC:  37 0 0 1879048192 8388608
// Threading:  0 0 8388608

// AfterAll
// Benchmark Process 2418593 has exited with code 0.

Mean = 65.224 ns, StdErr = 2.718 ns (4.17%), N = 7, StdDev = 7.191 ns
Min = 56.997 ns, Q1 = 60.047 ns, Median = 64.829 ns, Q3 = 69.871 ns, Max = 74.907 ns
IQR = 9.825 ns, LowerFence = 45.310 ns, UpperFence = 84.609 ns
ConfidenceInterval = [49.028 ns; 81.420 ns] (CI 99.9%), Margin = 16.196 ns (24.83% of Mean)
Skewness = 0.22, Kurtosis = 1.27, MValue = 2.8

// ** Remained 7 (87.5 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 1m from now) **
// **************************
// Benchmark: DispatchBenchmarks.TimeWarpGeneratedMonomorphicSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpGeneratedMonomorphicSend --job "IterationCount=8, WarmupCount=3" --benchmarkId 1 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 224726.00 ns, 224.7260 us/op
WorkloadJitting  1: 1 op, 2236314.00 ns, 2.2363 ms/op

OverheadJitting  2: 16 op, 303169.00 ns, 18.9481 us/op
WorkloadJitting  2: 16 op, 416509.00 ns, 26.0318 us/op

WorkloadPilot    1: 16 op, 639993.00 ns, 39.9996 us/op
WorkloadPilot    2: 32 op, 20801.00 ns, 650.0313 ns/op
WorkloadPilot    3: 64 op, 39116.00 ns, 611.1875 ns/op
WorkloadPilot    4: 128 op, 67261.00 ns, 525.4766 ns/op
WorkloadPilot    5: 256 op, 100315.00 ns, 391.8555 ns/op
WorkloadPilot    6: 512 op, 268552.00 ns, 524.5156 ns/op
WorkloadPilot    7: 1024 op, 348136.00 ns, 339.9766 ns/op
WorkloadPilot    8: 2048 op, 685021.00 ns, 334.4829 ns/op
WorkloadPilot    9: 4096 op, 1246622.00 ns, 304.3511 ns/op
WorkloadPilot   10: 8192 op, 2810420.00 ns, 343.0688 ns/op
WorkloadPilot   11: 16384 op, 5411991.00 ns, 330.3217 ns/op
WorkloadPilot   12: 32768 op, 10702862.00 ns, 326.6254 ns/op
WorkloadPilot   13: 65536 op, 23430068.00 ns, 357.5145 ns/op
WorkloadPilot   14: 131072 op, 56632713.00 ns, 432.0733 ns/op
WorkloadPilot   15: 262144 op, 82350166.00 ns, 314.1410 ns/op
WorkloadPilot   16: 524288 op, 44879037.00 ns, 85.6000 ns/op
WorkloadPilot   17: 1048576 op, 46736889.00 ns, 44.5718 ns/op
WorkloadPilot   18: 2097152 op, 103435677.00 ns, 49.3220 ns/op
WorkloadPilot   19: 4194304 op, 261023281.00 ns, 62.2328 ns/op
WorkloadPilot   20: 8388608 op, 401769444.00 ns, 47.8946 ns/op
WorkloadPilot   21: 16777216 op, 699542462.00 ns, 41.6960 ns/op

OverheadWarmup   1: 16777216 op, 56022808.00 ns, 3.3392 ns/op
OverheadWarmup   2: 16777216 op, 58339227.00 ns, 3.4773 ns/op
OverheadWarmup   3: 16777216 op, 52650075.00 ns, 3.1382 ns/op
OverheadWarmup   4: 16777216 op, 43821363.00 ns, 2.6120 ns/op
OverheadWarmup   5: 16777216 op, 34599136.00 ns, 2.0623 ns/op
OverheadWarmup   6: 16777216 op, 34729299.00 ns, 2.0700 ns/op
OverheadWarmup   7: 16777216 op, 34099557.00 ns, 2.0325 ns/op

OverheadActual   1: 16777216 op, 33116254.00 ns, 1.9739 ns/op
OverheadActual   2: 16777216 op, 44319270.00 ns, 2.6416 ns/op
OverheadActual   3: 16777216 op, 34700994.00 ns, 2.0683 ns/op
OverheadActual   4: 16777216 op, 33238473.00 ns, 1.9812 ns/op
OverheadActual   5: 16777216 op, 32849107.00 ns, 1.9580 ns/op
OverheadActual   6: 16777216 op, 32362642.00 ns, 1.9290 ns/op
OverheadActual   7: 16777216 op, 32718914.00 ns, 1.9502 ns/op
OverheadActual   8: 16777216 op, 33090817.00 ns, 1.9724 ns/op
OverheadActual   9: 16777216 op, 32952632.00 ns, 1.9641 ns/op
OverheadActual  10: 16777216 op, 32371292.00 ns, 1.9295 ns/op
OverheadActual  11: 16777216 op, 33191323.00 ns, 1.9784 ns/op
OverheadActual  12: 16777216 op, 33292573.00 ns, 1.9844 ns/op
OverheadActual  13: 16777216 op, 32747426.00 ns, 1.9519 ns/op
OverheadActual  14: 16777216 op, 32465305.00 ns, 1.9351 ns/op
OverheadActual  15: 16777216 op, 33085600.00 ns, 1.9721 ns/op

WorkloadWarmup   1: 16777216 op, 569377654.00 ns, 33.9376 ns/op
WorkloadWarmup   2: 16777216 op, 541415076.00 ns, 32.2709 ns/op
WorkloadWarmup   3: 16777216 op, 531907657.00 ns, 31.7042 ns/op

// BeforeActualRun
WorkloadActual   1: 16777216 op, 537564122.00 ns, 32.0413 ns/op
WorkloadActual   2: 16777216 op, 533661617.00 ns, 31.8087 ns/op
WorkloadActual   3: 16777216 op, 531588819.00 ns, 31.6852 ns/op
WorkloadActual   4: 16777216 op, 532601055.00 ns, 31.7455 ns/op
WorkloadActual   5: 16777216 op, 536710854.00 ns, 31.9905 ns/op
WorkloadActual   6: 16777216 op, 533951479.00 ns, 31.8260 ns/op
WorkloadActual   7: 16777216 op, 537057215.00 ns, 32.0111 ns/op
WorkloadActual   8: 16777216 op, 536011657.00 ns, 31.9488 ns/op

// AfterActualRun
WorkloadResult   1: 16777216 op, 504478522.00 ns, 30.0693 ns/op
WorkloadResult   2: 16777216 op, 500576017.00 ns, 29.8367 ns/op
WorkloadResult   3: 16777216 op, 498503219.00 ns, 29.7131 ns/op
WorkloadResult   4: 16777216 op, 499515455.00 ns, 29.7734 ns/op
WorkloadResult   5: 16777216 op, 503625254.00 ns, 30.0184 ns/op
WorkloadResult   6: 16777216 op, 500865879.00 ns, 29.8539 ns/op
WorkloadResult   7: 16777216 op, 503971615.00 ns, 30.0390 ns/op
WorkloadResult   8: 16777216 op, 502926057.00 ns, 29.9767 ns/op
// GC:  32 0 0 1610612736 16777216
// Threading:  0 0 16777216

// AfterAll
// Benchmark Process 2419438 has exited with code 0.

Mean = 29.910 ns, StdErr = 0.047 ns (0.16%), N = 8, StdDev = 0.133 ns
Min = 29.713 ns, Q1 = 29.821 ns, Median = 29.915 ns, Q3 = 30.024 ns, Max = 30.069 ns
IQR = 0.203 ns, LowerFence = 29.517 ns, UpperFence = 30.328 ns
ConfidenceInterval = [29.655 ns; 30.165 ns] (CI 99.9%), Margin = 0.255 ns (0.85% of Mean)
Skewness = -0.16, Kurtosis = 1.18, MValue = 2

// ** Remained 6 (75.0 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// **************************
// Benchmark: DispatchBenchmarks.TimeWarpGeneratedInterfaceSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpGeneratedInterfaceSend --job "IterationCount=8, WarmupCount=3" --benchmarkId 2 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 153968.00 ns, 153.9680 us/op
WorkloadJitting  1: 1 op, 3150723.00 ns, 3.1507 ms/op

OverheadJitting  2: 16 op, 675150.00 ns, 42.1969 us/op
WorkloadJitting  2: 16 op, 861441.00 ns, 53.8401 us/op

WorkloadPilot    1: 16 op, 1174038.00 ns, 73.3774 us/op
WorkloadPilot    2: 32 op, 64595.00 ns, 2.0186 us/op
WorkloadPilot    3: 64 op, 75276.00 ns, 1.1762 us/op
WorkloadPilot    4: 128 op, 137898.00 ns, 1.0773 us/op
WorkloadPilot    5: 256 op, 318869.00 ns, 1.2456 us/op
WorkloadPilot    6: 512 op, 466235.00 ns, 910.6152 ns/op
WorkloadPilot    7: 1024 op, 845170.00 ns, 825.3613 ns/op
WorkloadPilot    8: 2048 op, 1648378.00 ns, 804.8721 ns/op
WorkloadPilot    9: 4096 op, 3291226.00 ns, 803.5220 ns/op
WorkloadPilot   10: 8192 op, 7484831.00 ns, 913.6757 ns/op
WorkloadPilot   11: 16384 op, 13368238.00 ns, 815.9325 ns/op
WorkloadPilot   12: 32768 op, 18480874.00 ns, 563.9915 ns/op
WorkloadPilot   13: 65536 op, 33607142.00 ns, 512.8043 ns/op
WorkloadPilot   14: 131072 op, 68890299.00 ns, 525.5913 ns/op
WorkloadPilot   15: 262144 op, 108034673.00 ns, 412.1196 ns/op
WorkloadPilot   16: 524288 op, 38241756.00 ns, 72.9404 ns/op
WorkloadPilot   17: 1048576 op, 74999090.00 ns, 71.5247 ns/op
WorkloadPilot   18: 2097152 op, 195290340.00 ns, 93.1217 ns/op
WorkloadPilot   19: 4194304 op, 462829601.00 ns, 110.3472 ns/op
WorkloadPilot   20: 8388608 op, 987953452.00 ns, 117.7732 ns/op

OverheadWarmup   1: 8388608 op, 34027516.00 ns, 4.0564 ns/op
OverheadWarmup   2: 8388608 op, 36193209.00 ns, 4.3146 ns/op
OverheadWarmup   3: 8388608 op, 35796470.00 ns, 4.2673 ns/op
OverheadWarmup   4: 8388608 op, 36210331.00 ns, 4.3166 ns/op
OverheadWarmup   5: 8388608 op, 35701337.00 ns, 4.2559 ns/op

OverheadActual   1: 8388608 op, 30516237.00 ns, 3.6378 ns/op
OverheadActual   2: 8388608 op, 32442883.00 ns, 3.8675 ns/op
OverheadActual   3: 8388608 op, 28566923.00 ns, 3.4054 ns/op
OverheadActual   4: 8388608 op, 33606270.00 ns, 4.0062 ns/op
OverheadActual   5: 8388608 op, 33950888.00 ns, 4.0473 ns/op
OverheadActual   6: 8388608 op, 31539287.00 ns, 3.7598 ns/op
OverheadActual   7: 8388608 op, 31222365.00 ns, 3.7220 ns/op
OverheadActual   8: 8388608 op, 30393971.00 ns, 3.6232 ns/op
OverheadActual   9: 8388608 op, 30515956.00 ns, 3.6378 ns/op
OverheadActual  10: 8388608 op, 22478062.00 ns, 2.6796 ns/op
OverheadActual  11: 8388608 op, 23711453.00 ns, 2.8266 ns/op
OverheadActual  12: 8388608 op, 23015513.00 ns, 2.7437 ns/op
OverheadActual  13: 8388608 op, 19737405.00 ns, 2.3529 ns/op
OverheadActual  14: 8388608 op, 23046514.00 ns, 2.7474 ns/op
OverheadActual  15: 8388608 op, 20603906.00 ns, 2.4562 ns/op
OverheadActual  16: 8388608 op, 18911548.00 ns, 2.2544 ns/op
OverheadActual  17: 8388608 op, 21314681.00 ns, 2.5409 ns/op
OverheadActual  18: 8388608 op, 17407977.00 ns, 2.0752 ns/op
OverheadActual  19: 8388608 op, 18393976.00 ns, 2.1927 ns/op
OverheadActual  20: 8388608 op, 21749903.00 ns, 2.5928 ns/op

WorkloadWarmup   1: 8388608 op, 940600250.00 ns, 112.1283 ns/op
WorkloadWarmup   2: 8388608 op, 675306492.00 ns, 80.5028 ns/op
WorkloadWarmup   3: 8388608 op, 552031457.00 ns, 65.8073 ns/op

// BeforeActualRun
WorkloadActual   1: 8388608 op, 517410184.00 ns, 61.6801 ns/op
WorkloadActual   2: 8388608 op, 550586662.00 ns, 65.6350 ns/op
WorkloadActual   3: 8388608 op, 733459382.00 ns, 87.4352 ns/op
WorkloadActual   4: 8388608 op, 963260117.00 ns, 114.8296 ns/op
WorkloadActual   5: 8388608 op, 583972075.00 ns, 69.6149 ns/op
WorkloadActual   6: 8388608 op, 523671330.00 ns, 62.4265 ns/op
WorkloadActual   7: 8388608 op, 526233070.00 ns, 62.7319 ns/op
WorkloadActual   8: 8388608 op, 531664137.00 ns, 63.3793 ns/op

// AfterActualRun
WorkloadResult   1: 8388608 op, 494031200.50 ns, 58.8931 ns/op
WorkloadResult   2: 8388608 op, 527207678.50 ns, 62.8481 ns/op
WorkloadResult   3: 8388608 op, 710080398.50 ns, 84.6482 ns/op
WorkloadResult   4: 8388608 op, 560593091.50 ns, 66.8279 ns/op
WorkloadResult   5: 8388608 op, 500292346.50 ns, 59.6395 ns/op
WorkloadResult   6: 8388608 op, 502854086.50 ns, 59.9449 ns/op
WorkloadResult   7: 8388608 op, 508285153.50 ns, 60.5923 ns/op
// GC:  40 0 0 2013265920 8388608
// Threading:  0 0 8388608

// AfterAll
// Benchmark Process 2420301 has exited with code 0.

Mean = 64.771 ns, StdErr = 3.466 ns (5.35%), N = 7, StdDev = 9.171 ns
Min = 58.893 ns, Q1 = 59.792 ns, Median = 60.592 ns, Q3 = 64.838 ns, Max = 84.648 ns
IQR = 5.046 ns, LowerFence = 52.223 ns, UpperFence = 72.407 ns
ConfidenceInterval = [44.115 ns; 85.426 ns] (CI 99.9%), Margin = 20.656 ns (31.89% of Mean)
Skewness = 1.36, Kurtosis = 3.21, MValue = 2.4

// ** Remained 5 (62.5 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// **************************
// Benchmark: DispatchBenchmarks.TimeWarpGeneratedObjectSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpGeneratedObjectSend --job "IterationCount=8, WarmupCount=3" --benchmarkId 3 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 172284.00 ns, 172.2840 us/op
WorkloadJitting  1: 1 op, 2348876.00 ns, 2.3489 ms/op

OverheadJitting  2: 16 op, 246957.00 ns, 15.4348 us/op
WorkloadJitting  2: 16 op, 360257.00 ns, 22.5161 us/op

WorkloadPilot    1: 16 op, 794808.00 ns, 49.6755 us/op
WorkloadPilot    2: 32 op, 34637.00 ns, 1.0824 us/op
WorkloadPilot    3: 64 op, 47823.00 ns, 747.2344 ns/op
WorkloadPilot    4: 128 op, 64986.00 ns, 507.7031 ns/op
WorkloadPilot    5: 256 op, 119351.00 ns, 466.2148 ns/op
WorkloadPilot    6: 512 op, 203864.00 ns, 398.1719 ns/op
WorkloadPilot    7: 1024 op, 452416.00 ns, 441.8125 ns/op
WorkloadPilot    8: 2048 op, 719582.00 ns, 351.3584 ns/op
WorkloadPilot    9: 4096 op, 1441300.00 ns, 351.8799 ns/op
WorkloadPilot   10: 8192 op, 3267944.00 ns, 398.9189 ns/op
WorkloadPilot   11: 16384 op, 6366221.00 ns, 388.5633 ns/op
WorkloadPilot   12: 32768 op, 13696257.00 ns, 417.9766 ns/op
WorkloadPilot   13: 65536 op, 26971149.00 ns, 411.5471 ns/op
WorkloadPilot   14: 131072 op, 52013505.00 ns, 396.8316 ns/op
WorkloadPilot   15: 262144 op, 97275995.00 ns, 371.0785 ns/op
WorkloadPilot   16: 524288 op, 32509586.00 ns, 62.0071 ns/op
WorkloadPilot   17: 1048576 op, 45861600.00 ns, 43.7370 ns/op
WorkloadPilot   18: 2097152 op, 92585075.00 ns, 44.1480 ns/op
WorkloadPilot   19: 4194304 op, 255064584.00 ns, 60.8121 ns/op
WorkloadPilot   20: 8388608 op, 472186282.00 ns, 56.2890 ns/op
WorkloadPilot   21: 16777216 op, 726788210.00 ns, 43.3200 ns/op

OverheadWarmup   1: 16777216 op, 46458044.00 ns, 2.7691 ns/op
OverheadWarmup   2: 16777216 op, 48575391.00 ns, 2.8953 ns/op
OverheadWarmup   3: 16777216 op, 50613306.00 ns, 3.0168 ns/op
OverheadWarmup   4: 16777216 op, 46569990.00 ns, 2.7758 ns/op
OverheadWarmup   5: 16777216 op, 48358952.00 ns, 2.8824 ns/op
OverheadWarmup   6: 16777216 op, 63517442.00 ns, 3.7859 ns/op
OverheadWarmup   7: 16777216 op, 35058972.00 ns, 2.0897 ns/op

OverheadActual   1: 16777216 op, 33480258.00 ns, 1.9956 ns/op
OverheadActual   2: 16777216 op, 32602698.00 ns, 1.9433 ns/op
OverheadActual   3: 16777216 op, 32888791.00 ns, 1.9603 ns/op
OverheadActual   4: 16777216 op, 33605600.00 ns, 2.0030 ns/op
OverheadActual   5: 16777216 op, 36236702.00 ns, 2.1599 ns/op
OverheadActual   6: 16777216 op, 35820504.00 ns, 2.1351 ns/op
OverheadActual   7: 16777216 op, 35329415.00 ns, 2.1058 ns/op
OverheadActual   8: 16777216 op, 39095282.00 ns, 2.3303 ns/op
OverheadActual   9: 16777216 op, 34811593.00 ns, 2.0749 ns/op
OverheadActual  10: 16777216 op, 34493467.00 ns, 2.0560 ns/op
OverheadActual  11: 16777216 op, 34001836.00 ns, 2.0267 ns/op
OverheadActual  12: 16777216 op, 35515895.00 ns, 2.1169 ns/op
OverheadActual  13: 16777216 op, 39221607.00 ns, 2.3378 ns/op
OverheadActual  14: 16777216 op, 38450094.00 ns, 2.2918 ns/op
OverheadActual  15: 16777216 op, 42254775.00 ns, 2.5186 ns/op
OverheadActual  16: 16777216 op, 43729583.00 ns, 2.6065 ns/op
OverheadActual  17: 16777216 op, 44703878.00 ns, 2.6646 ns/op
OverheadActual  18: 16777216 op, 45658356.00 ns, 2.7215 ns/op
OverheadActual  19: 16777216 op, 48152414.00 ns, 2.8701 ns/op
OverheadActual  20: 16777216 op, 57048831.00 ns, 3.4004 ns/op

WorkloadWarmup   1: 16777216 op, 1359327044.00 ns, 81.0222 ns/op
WorkloadWarmup   2: 16777216 op, 1174149997.00 ns, 69.9848 ns/op
WorkloadWarmup   3: 16777216 op, 772824628.00 ns, 46.0639 ns/op

// BeforeActualRun
WorkloadActual   1: 16777216 op, 721627151.00 ns, 43.0123 ns/op
WorkloadActual   2: 16777216 op, 783402223.00 ns, 46.6944 ns/op
WorkloadActual   3: 16777216 op, 733302213.00 ns, 43.7082 ns/op
WorkloadActual   4: 16777216 op, 856029440.00 ns, 51.0233 ns/op
WorkloadActual   5: 16777216 op, 798685217.00 ns, 47.6053 ns/op
WorkloadActual   6: 16777216 op, 1015003799.00 ns, 60.4989 ns/op
WorkloadActual   7: 16777216 op, 1120232758.00 ns, 66.7711 ns/op
WorkloadActual   8: 16777216 op, 1042122656.00 ns, 62.1154 ns/op

// AfterActualRun
WorkloadResult   1: 16777216 op, 685598548.00 ns, 40.8649 ns/op
WorkloadResult   2: 16777216 op, 747373620.00 ns, 44.5469 ns/op
WorkloadResult   3: 16777216 op, 697273610.00 ns, 41.5607 ns/op
WorkloadResult   4: 16777216 op, 820000837.00 ns, 48.8759 ns/op
WorkloadResult   5: 16777216 op, 762656614.00 ns, 45.4579 ns/op
WorkloadResult   6: 16777216 op, 978975196.00 ns, 58.3515 ns/op
WorkloadResult   7: 16777216 op, 1084204155.00 ns, 64.6236 ns/op
WorkloadResult   8: 16777216 op, 1006094053.00 ns, 59.9679 ns/op
// GC:  56 0 0 2818572288 16777216
// Threading:  0 0 16777216

// AfterAll
// Benchmark Process 2420789 has exited with code 0.

Mean = 50.531 ns, StdErr = 3.238 ns (6.41%), N = 8, StdDev = 9.157 ns
Min = 40.865 ns, Q1 = 43.800 ns, Median = 47.167 ns, Q3 = 58.756 ns, Max = 64.624 ns
IQR = 14.955 ns, LowerFence = 21.368 ns, UpperFence = 81.188 ns
ConfidenceInterval = [33.023 ns; 68.040 ns] (CI 99.9%), Margin = 17.509 ns (34.65% of Mean)
Skewness = 0.35, Kurtosis = 1.21, MValue = 3.2

// ** Remained 4 (50.0 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// **************************
// Benchmark: DispatchBenchmarks.TimeWarpGeneratedInterfaceSendUnit: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpGeneratedInterfaceSendUnit --job "IterationCount=8, WarmupCount=3" --benchmarkId 4 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 112558.00 ns, 112.5580 us/op
WorkloadJitting  1: 1 op, 2932440.00 ns, 2.9324 ms/op

OverheadJitting  2: 16 op, 151393.00 ns, 9.4621 us/op
WorkloadJitting  2: 16 op, 222271.00 ns, 13.8919 us/op

WorkloadPilot    1: 16 op, 646454.00 ns, 40.4034 us/op
WorkloadPilot    2: 32 op, 21993.00 ns, 687.2813 ns/op
WorkloadPilot    3: 64 op, 46430.00 ns, 725.4688 ns/op
WorkloadPilot    4: 128 op, 62110.00 ns, 485.2344 ns/op
WorkloadPilot    5: 256 op, 97700.00 ns, 381.6406 ns/op
WorkloadPilot    6: 512 op, 186232.00 ns, 363.7344 ns/op
WorkloadPilot    7: 1024 op, 345761.00 ns, 337.6572 ns/op
WorkloadPilot    8: 2048 op, 714427.00 ns, 348.8413 ns/op
WorkloadPilot    9: 4096 op, 1267410.00 ns, 309.4263 ns/op
WorkloadPilot   10: 8192 op, 3209718.00 ns, 391.8113 ns/op
WorkloadPilot   11: 16384 op, 5669550.00 ns, 346.0419 ns/op
WorkloadPilot   12: 32768 op, 10942232.00 ns, 333.9304 ns/op
WorkloadPilot   13: 65536 op, 21457484.00 ns, 327.4152 ns/op
WorkloadPilot   14: 131072 op, 43438375.00 ns, 331.4085 ns/op
WorkloadPilot   15: 262144 op, 37628582.00 ns, 143.5416 ns/op
WorkloadPilot   16: 524288 op, 57904915.00 ns, 110.4449 ns/op
WorkloadPilot   17: 1048576 op, 59697594.00 ns, 56.9321 ns/op
WorkloadPilot   18: 2097152 op, 94313560.00 ns, 44.9722 ns/op
WorkloadPilot   19: 4194304 op, 184424035.00 ns, 43.9701 ns/op
WorkloadPilot   20: 8388608 op, 371685724.00 ns, 44.3084 ns/op
WorkloadPilot   21: 16777216 op, 1029166624.00 ns, 61.3431 ns/op

OverheadWarmup   1: 16777216 op, 28193878.00 ns, 1.6805 ns/op
OverheadWarmup   2: 16777216 op, 30193976.00 ns, 1.7997 ns/op
OverheadWarmup   3: 16777216 op, 24585205.00 ns, 1.4654 ns/op
OverheadWarmup   4: 16777216 op, 24339236.00 ns, 1.4507 ns/op
OverheadWarmup   5: 16777216 op, 24190016.00 ns, 1.4418 ns/op
OverheadWarmup   6: 16777216 op, 34231713.00 ns, 2.0404 ns/op
OverheadWarmup   7: 16777216 op, 24470417.00 ns, 1.4586 ns/op

OverheadActual   1: 16777216 op, 23827633.00 ns, 1.4202 ns/op
OverheadActual   2: 16777216 op, 24038285.00 ns, 1.4328 ns/op
OverheadActual   3: 16777216 op, 24264136.00 ns, 1.4463 ns/op
OverheadActual   4: 16777216 op, 32585935.00 ns, 1.9423 ns/op
OverheadActual   5: 16777216 op, 24084852.00 ns, 1.4356 ns/op
OverheadActual   6: 16777216 op, 24644980.00 ns, 1.4690 ns/op
OverheadActual   7: 16777216 op, 24431035.00 ns, 1.4562 ns/op
OverheadActual   8: 16777216 op, 24015778.00 ns, 1.4315 ns/op
OverheadActual   9: 16777216 op, 24042129.00 ns, 1.4330 ns/op
OverheadActual  10: 16777216 op, 23864113.00 ns, 1.4224 ns/op
OverheadActual  11: 16777216 op, 23891626.00 ns, 1.4241 ns/op
OverheadActual  12: 16777216 op, 18719754.00 ns, 1.1158 ns/op
OverheadActual  13: 16777216 op, 17393666.00 ns, 1.0367 ns/op
OverheadActual  14: 16777216 op, 17521827.00 ns, 1.0444 ns/op
OverheadActual  15: 16777216 op, 17368789.00 ns, 1.0353 ns/op
OverheadActual  16: 16777216 op, 17347998.00 ns, 1.0340 ns/op
OverheadActual  17: 16777216 op, 17422233.00 ns, 1.0384 ns/op
OverheadActual  18: 16777216 op, 17404939.00 ns, 1.0374 ns/op
OverheadActual  19: 16777216 op, 17472380.00 ns, 1.0414 ns/op
OverheadActual  20: 16777216 op, 17617282.00 ns, 1.0501 ns/op

WorkloadWarmup   1: 16777216 op, 767043051.00 ns, 45.7193 ns/op
WorkloadWarmup   2: 16777216 op, 747310958.00 ns, 44.5432 ns/op
WorkloadWarmup   3: 16777216 op, 731394623.00 ns, 43.5945 ns/op

// BeforeActualRun
WorkloadActual   1: 16777216 op, 823455155.00 ns, 49.0818 ns/op
WorkloadActual   2: 16777216 op, 762425227.00 ns, 45.4441 ns/op
WorkloadActual   3: 16777216 op, 804581957.00 ns, 47.9568 ns/op
WorkloadActual   4: 16777216 op, 724902759.00 ns, 43.2076 ns/op
WorkloadActual   5: 16777216 op, 731811549.00 ns, 43.6194 ns/op
WorkloadActual   6: 16777216 op, 753571950.00 ns, 44.9164 ns/op
WorkloadActual   7: 16777216 op, 747452268.00 ns, 44.5516 ns/op
WorkloadActual   8: 16777216 op, 728665272.00 ns, 43.4318 ns/op

// AfterActualRun
WorkloadResult   1: 16777216 op, 799609282.00 ns, 47.6604 ns/op
WorkloadResult   2: 16777216 op, 738579354.00 ns, 44.0228 ns/op
WorkloadResult   3: 16777216 op, 780736084.00 ns, 46.5355 ns/op
WorkloadResult   4: 16777216 op, 701056886.00 ns, 41.7862 ns/op
WorkloadResult   5: 16777216 op, 707965676.00 ns, 42.1980 ns/op
WorkloadResult   6: 16777216 op, 729726077.00 ns, 43.4951 ns/op
WorkloadResult   7: 16777216 op, 723606395.00 ns, 43.1303 ns/op
WorkloadResult   8: 16777216 op, 704819399.00 ns, 42.0105 ns/op
// GC:  32 0 0 1610612736 16777216
// Threading:  0 0 16777216

// AfterAll
// Benchmark Process 2421788 has exited with code 0.

Mean = 43.855 ns, StdErr = 0.765 ns (1.74%), N = 8, StdDev = 2.164 ns
Min = 41.786 ns, Q1 = 42.151 ns, Median = 43.313 ns, Q3 = 44.651 ns, Max = 47.660 ns
IQR = 2.500 ns, LowerFence = 38.401 ns, UpperFence = 48.401 ns
ConfidenceInterval = [39.718 ns; 47.992 ns] (CI 99.9%), Margin = 4.137 ns (9.43% of Mean)
Skewness = 0.67, Kurtosis = 1.71, MValue = 2.67

// ** Remained 3 (37.5 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// **************************
// Benchmark: DispatchBenchmarks.TimeWarpGeneratedMonomorphicSendUnit: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpGeneratedMonomorphicSendUnit --job "IterationCount=8, WarmupCount=3" --benchmarkId 5 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 165942.00 ns, 165.9420 us/op
WorkloadJitting  1: 1 op, 2869997.00 ns, 2.8700 ms/op

OverheadJitting  2: 16 op, 220187.00 ns, 13.7617 us/op
WorkloadJitting  2: 16 op, 324480.00 ns, 20.2800 us/op

WorkloadPilot    1: 16 op, 717212.00 ns, 44.8258 us/op
WorkloadPilot    2: 32 op, 11612.00 ns, 362.8750 ns/op
WorkloadPilot    3: 64 op, 17354.00 ns, 271.1563 ns/op
WorkloadPilot    4: 128 op, 35710.00 ns, 278.9844 ns/op
WorkloadPilot    5: 256 op, 113791.00 ns, 444.4961 ns/op
WorkloadPilot    6: 512 op, 110995.00 ns, 216.7871 ns/op
WorkloadPilot    7: 1024 op, 225186.00 ns, 219.9082 ns/op
WorkloadPilot    8: 2048 op, 435184.00 ns, 212.4922 ns/op
WorkloadPilot    9: 4096 op, 887321.00 ns, 216.6311 ns/op
WorkloadPilot   10: 8192 op, 1871300.00 ns, 228.4302 ns/op
WorkloadPilot   11: 16384 op, 3703845.00 ns, 226.0648 ns/op
WorkloadPilot   12: 32768 op, 7461074.00 ns, 227.6939 ns/op
WorkloadPilot   13: 65536 op, 14507203.00 ns, 221.3624 ns/op
WorkloadPilot   14: 131072 op, 28940277.00 ns, 220.7968 ns/op
WorkloadPilot   15: 262144 op, 57739421.00 ns, 220.2584 ns/op
WorkloadPilot   16: 524288 op, 83208266.00 ns, 158.7072 ns/op
WorkloadPilot   17: 1048576 op, 26133538.00 ns, 24.9229 ns/op
WorkloadPilot   18: 2097152 op, 52974528.00 ns, 25.2602 ns/op
WorkloadPilot   19: 4194304 op, 110076001.00 ns, 26.2442 ns/op
WorkloadPilot   20: 8388608 op, 214266476.00 ns, 25.5426 ns/op
WorkloadPilot   21: 16777216 op, 424061680.00 ns, 25.2760 ns/op
WorkloadPilot   22: 33554432 op, 845616811.00 ns, 25.2013 ns/op

OverheadWarmup   1: 33554432 op, 47773017.00 ns, 1.4237 ns/op
OverheadWarmup   2: 33554432 op, 47264379.00 ns, 1.4086 ns/op
OverheadWarmup   3: 33554432 op, 47815811.00 ns, 1.4250 ns/op
OverheadWarmup   4: 33554432 op, 49763518.00 ns, 1.4831 ns/op
OverheadWarmup   5: 33554432 op, 48283117.00 ns, 1.4389 ns/op
OverheadWarmup   6: 33554432 op, 46994837.00 ns, 1.4006 ns/op
OverheadWarmup   7: 33554432 op, 37672954.00 ns, 1.1227 ns/op
OverheadWarmup   8: 33554432 op, 33988486.00 ns, 1.0129 ns/op
OverheadWarmup   9: 33554432 op, 34327132.00 ns, 1.0230 ns/op
OverheadWarmup  10: 33554432 op, 34297214.00 ns, 1.0221 ns/op

OverheadActual   1: 33554432 op, 34936213.00 ns, 1.0412 ns/op
OverheadActual   2: 33554432 op, 34448598.00 ns, 1.0266 ns/op
OverheadActual   3: 33554432 op, 34674286.00 ns, 1.0334 ns/op
OverheadActual   4: 33554432 op, 34481362.00 ns, 1.0276 ns/op
OverheadActual   5: 33554432 op, 34581184.00 ns, 1.0306 ns/op
OverheadActual   6: 33554432 op, 34243802.00 ns, 1.0205 ns/op
OverheadActual   7: 33554432 op, 34339987.00 ns, 1.0234 ns/op
OverheadActual   8: 33554432 op, 34040888.00 ns, 1.0145 ns/op
OverheadActual   9: 33554432 op, 34120071.00 ns, 1.0169 ns/op
OverheadActual  10: 33554432 op, 33984408.00 ns, 1.0128 ns/op
OverheadActual  11: 33554432 op, 33981382.00 ns, 1.0127 ns/op
OverheadActual  12: 33554432 op, 34059994.00 ns, 1.0151 ns/op
OverheadActual  13: 33554432 op, 34193103.00 ns, 1.0190 ns/op
OverheadActual  14: 33554432 op, 34234563.00 ns, 1.0203 ns/op
OverheadActual  15: 33554432 op, 35128938.00 ns, 1.0469 ns/op

WorkloadWarmup   1: 33554432 op, 1094911994.00 ns, 32.6309 ns/op
WorkloadWarmup   2: 33554432 op, 901114078.00 ns, 26.8553 ns/op
WorkloadWarmup   3: 33554432 op, 872505093.00 ns, 26.0027 ns/op

// BeforeActualRun
WorkloadActual   1: 33554432 op, 836362167.00 ns, 24.9255 ns/op
WorkloadActual   2: 33554432 op, 852905449.00 ns, 25.4186 ns/op
WorkloadActual   3: 33554432 op, 843339350.00 ns, 25.1335 ns/op
WorkloadActual   4: 33554432 op, 836428515.00 ns, 24.9275 ns/op
WorkloadActual   5: 33554432 op, 848975215.00 ns, 25.3014 ns/op
WorkloadActual   6: 33554432 op, 846372139.00 ns, 25.2239 ns/op
WorkloadActual   7: 33554432 op, 842449055.00 ns, 25.1069 ns/op
WorkloadActual   8: 33554432 op, 845907303.00 ns, 25.2100 ns/op

// AfterActualRun
WorkloadResult   1: 33554432 op, 802118365.00 ns, 23.9050 ns/op
WorkloadResult   2: 33554432 op, 818661647.00 ns, 24.3980 ns/op
WorkloadResult   3: 33554432 op, 809095548.00 ns, 24.1129 ns/op
WorkloadResult   4: 33554432 op, 802184713.00 ns, 23.9070 ns/op
WorkloadResult   5: 33554432 op, 814731413.00 ns, 24.2809 ns/op
WorkloadResult   6: 33554432 op, 812128337.00 ns, 24.2033 ns/op
WorkloadResult   7: 33554432 op, 808205253.00 ns, 24.0864 ns/op
WorkloadResult   8: 33554432 op, 811663501.00 ns, 24.1895 ns/op
// GC:  0 0 0 0 33554432
// Threading:  0 0 33554432

// AfterAll
// Benchmark Process 2422224 has exited with code 0.

Mean = 24.135 ns, StdErr = 0.061 ns (0.25%), N = 8, StdDev = 0.171 ns
Min = 23.905 ns, Q1 = 24.042 ns, Median = 24.151 ns, Q3 = 24.223 ns, Max = 24.398 ns
IQR = 0.181 ns, LowerFence = 23.770 ns, UpperFence = 24.494 ns
ConfidenceInterval = [23.807 ns; 24.463 ns] (CI 99.9%), Margin = 0.328 ns (1.36% of Mean)
Skewness = -0.06, Kurtosis = 1.56, MValue = 2

// ** Remained 2 (25.0 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// **************************
// Benchmark: DispatchBenchmarks.TimeWarpCallSiteInliningPrototype: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.TimeWarpCallSiteInliningPrototype --job "IterationCount=8, WarmupCount=3" --benchmarkId 6 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 145923.00 ns, 145.9230 us/op
WorkloadJitting  1: 1 op, 1845161.00 ns, 1.8452 ms/op

OverheadJitting  2: 16 op, 307466.00 ns, 19.2166 us/op
WorkloadJitting  2: 16 op, 380769.00 ns, 23.7981 us/op

WorkloadPilot    1: 16 op, 616616.00 ns, 38.5385 us/op
WorkloadPilot    2: 32 op, 19318.00 ns, 603.6875 ns/op
WorkloadPilot    3: 64 op, 34668.00 ns, 541.6875 ns/op
WorkloadPilot    4: 128 op, 49846.00 ns, 389.4219 ns/op
WorkloadPilot    5: 256 op, 84043.00 ns, 328.2930 ns/op
WorkloadPilot    6: 512 op, 148729.00 ns, 290.4863 ns/op
WorkloadPilot    7: 1024 op, 303700.00 ns, 296.5820 ns/op
WorkloadPilot    8: 2048 op, 590245.00 ns, 288.2056 ns/op
WorkloadPilot    9: 4096 op, 1135213.00 ns, 277.1516 ns/op
WorkloadPilot   10: 8192 op, 2487426.00 ns, 303.6409 ns/op
WorkloadPilot   11: 16384 op, 5609785.00 ns, 342.3941 ns/op
WorkloadPilot   12: 32768 op, 9467769.00 ns, 288.9334 ns/op
WorkloadPilot   13: 65536 op, 18589768.00 ns, 283.6573 ns/op
WorkloadPilot   14: 131072 op, 36767789.00 ns, 280.5160 ns/op
WorkloadPilot   15: 262144 op, 45167768.00 ns, 172.3014 ns/op
WorkloadPilot   16: 524288 op, 27316852.00 ns, 52.1028 ns/op
WorkloadPilot   17: 1048576 op, 32232990.00 ns, 30.7398 ns/op
WorkloadPilot   18: 2097152 op, 66255683.00 ns, 31.5932 ns/op
WorkloadPilot   19: 4194304 op, 129653244.00 ns, 30.9117 ns/op
WorkloadPilot   20: 8388608 op, 256774559.00 ns, 30.6099 ns/op
WorkloadPilot   21: 16777216 op, 522060857.00 ns, 31.1173 ns/op

OverheadWarmup   1: 16777216 op, 49945232.00 ns, 2.9770 ns/op
OverheadWarmup   2: 16777216 op, 45860742.00 ns, 2.7335 ns/op
OverheadWarmup   3: 16777216 op, 45528957.00 ns, 2.7137 ns/op
OverheadWarmup   4: 16777216 op, 46859069.00 ns, 2.7930 ns/op
OverheadWarmup   5: 16777216 op, 46123431.00 ns, 2.7492 ns/op
OverheadWarmup   6: 16777216 op, 47304452.00 ns, 2.8196 ns/op
OverheadWarmup   7: 16777216 op, 43498328.00 ns, 2.5927 ns/op

OverheadActual   1: 16777216 op, 34922638.00 ns, 2.0816 ns/op
OverheadActual   2: 16777216 op, 32981162.00 ns, 1.9658 ns/op
OverheadActual   3: 16777216 op, 32357523.00 ns, 1.9287 ns/op
OverheadActual   4: 16777216 op, 32804478.00 ns, 1.9553 ns/op
OverheadActual   5: 16777216 op, 33327153.00 ns, 1.9865 ns/op
OverheadActual   6: 16777216 op, 32373553.00 ns, 1.9296 ns/op
OverheadActual   7: 16777216 op, 33380546.00 ns, 1.9896 ns/op
OverheadActual   8: 16777216 op, 33179176.00 ns, 1.9776 ns/op
OverheadActual   9: 16777216 op, 34210277.00 ns, 2.0391 ns/op
OverheadActual  10: 16777216 op, 34357192.00 ns, 2.0478 ns/op
OverheadActual  11: 16777216 op, 33769492.00 ns, 2.0128 ns/op
OverheadActual  12: 16777216 op, 34954018.00 ns, 2.0834 ns/op
OverheadActual  13: 16777216 op, 32736608.00 ns, 1.9513 ns/op
OverheadActual  14: 16777216 op, 33212151.00 ns, 1.9796 ns/op
OverheadActual  15: 16777216 op, 33700640.00 ns, 2.0087 ns/op

WorkloadWarmup   1: 16777216 op, 540670910.00 ns, 32.2265 ns/op
WorkloadWarmup   2: 16777216 op, 532212273.00 ns, 31.7223 ns/op
WorkloadWarmup   3: 16777216 op, 516099385.00 ns, 30.7619 ns/op

// BeforeActualRun
WorkloadActual   1: 16777216 op, 532613992.00 ns, 31.7463 ns/op
WorkloadActual   2: 16777216 op, 535085818.00 ns, 31.8936 ns/op
WorkloadActual   3: 16777216 op, 522471675.00 ns, 31.1417 ns/op
WorkloadActual   4: 16777216 op, 516458431.00 ns, 30.7833 ns/op
WorkloadActual   5: 16777216 op, 524073778.00 ns, 31.2372 ns/op
WorkloadActual   6: 16777216 op, 528659505.00 ns, 31.5106 ns/op
WorkloadActual   7: 16777216 op, 524571173.00 ns, 31.2669 ns/op
WorkloadActual   8: 16777216 op, 533436338.00 ns, 31.7953 ns/op

// AfterActualRun
WorkloadResult   1: 16777216 op, 499286839.00 ns, 29.7598 ns/op
WorkloadResult   2: 16777216 op, 501758665.00 ns, 29.9071 ns/op
WorkloadResult   3: 16777216 op, 489144522.00 ns, 29.1553 ns/op
WorkloadResult   4: 16777216 op, 483131278.00 ns, 28.7969 ns/op
WorkloadResult   5: 16777216 op, 490746625.00 ns, 29.2508 ns/op
WorkloadResult   6: 16777216 op, 495332352.00 ns, 29.5241 ns/op
WorkloadResult   7: 16777216 op, 491244020.00 ns, 29.2804 ns/op
WorkloadResult   8: 16777216 op, 500109185.00 ns, 29.8088 ns/op
// GC:  32 0 0 1610612736 16777216
// Threading:  0 0 16777216

// AfterAll
// Benchmark Process 2422552 has exited with code 0.

Mean = 29.435 ns, StdErr = 0.135 ns (0.46%), N = 8, StdDev = 0.382 ns
Min = 28.797 ns, Q1 = 29.227 ns, Median = 29.402 ns, Q3 = 29.772 ns, Max = 29.907 ns
IQR = 0.545 ns, LowerFence = 28.409 ns, UpperFence = 30.590 ns
ConfidenceInterval = [28.706 ns; 30.165 ns] (CI 99.9%), Margin = 0.730 ns (2.48% of Mean)
Skewness = -0.23, Kurtosis = 1.5, MValue = 2

// ** Remained 1 (12.5 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// **************************
// Benchmark: DispatchBenchmarks.MartinothamarGenerated: Job-LGHQEI(IterationCount=8, WarmupCount=3)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1.dll --anonymousPipes 140 141 --benchmarkName MediatorBenchmarks.DispatchBenchmarks.MartinothamarGenerated --job "IterationCount=8, WarmupCount=3" --benchmarkId 7 in /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-mediator/task-021-deep-code-review-150-breaking-redesign-for-archite/tests/timewarp-mediator-benchmarks-comparison/bin/Release/net10.0/timewarp-mediator-benchmarks-comparison-Job-LGHQEI-1/bin/Release/net10.0
// Failed to set up high priority (Permission denied). In order to run benchmarks with high priority, make sure you have the right permissions.
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.15.8
// Runtime=.NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
// GC=Concurrent Workstation
// HardwareIntrinsics=AVX512 BITALG+VBMI2+VNNI+VPOPCNTDQ,AVX512 IFMA+VBMI,AVX512 F+BW+CD+DQ+VL,AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256
// Job: Job-LGHQEI(IterationCount=8, WarmupCount=3)

OverheadJitting  1: 1 op, 145633.00 ns, 145.6330 us/op
WorkloadJitting  1: 1 op, 3532343.00 ns, 3.5323 ms/op

OverheadJitting  2: 16 op, 282298.00 ns, 17.6436 us/op
WorkloadJitting  2: 16 op, 284882.00 ns, 17.8051 us/op

WorkloadPilot    1: 16 op, 3477.00 ns, 217.3125 ns/op
WorkloadPilot    2: 32 op, 3436.00 ns, 107.3750 ns/op
WorkloadPilot    3: 64 op, 7534.00 ns, 117.7188 ns/op
WorkloadPilot    4: 128 op, 8326.00 ns, 65.0469 ns/op
WorkloadPilot    5: 256 op, 25119.00 ns, 98.1211 ns/op
WorkloadPilot    6: 512 op, 42803.00 ns, 83.5996 ns/op
WorkloadPilot    7: 1024 op, 65477.00 ns, 63.9424 ns/op
WorkloadPilot    8: 2048 op, 114081.00 ns, 55.7036 ns/op
WorkloadPilot    9: 4096 op, 198645.00 ns, 48.4973 ns/op
WorkloadPilot   10: 8192 op, 433561.00 ns, 52.9249 ns/op
WorkloadPilot   11: 16384 op, 798108.00 ns, 48.7126 ns/op
WorkloadPilot   12: 32768 op, 1636084.00 ns, 49.9293 ns/op
WorkloadPilot   13: 65536 op, 3280514.00 ns, 50.0567 ns/op
WorkloadPilot   14: 131072 op, 7189327.00 ns, 54.8502 ns/op
WorkloadPilot   15: 262144 op, 13212081.00 ns, 50.4001 ns/op
WorkloadPilot   16: 524288 op, 26249705.00 ns, 50.0673 ns/op
WorkloadPilot   17: 1048576 op, 46110596.00 ns, 43.9745 ns/op
WorkloadPilot   18: 2097152 op, 35289872.00 ns, 16.8275 ns/op
WorkloadPilot   19: 4194304 op, 45275895.00 ns, 10.7946 ns/op
WorkloadPilot   20: 8388608 op, 86620824.00 ns, 10.3260 ns/op
WorkloadPilot   21: 16777216 op, 175087238.00 ns, 10.4360 ns/op
WorkloadPilot   22: 33554432 op, 351170207.00 ns, 10.4657 ns/op
WorkloadPilot   23: 67108864 op, 698135117.00 ns, 10.4030 ns/op

OverheadWarmup   1: 67108864 op, 180584322.00 ns, 2.6909 ns/op
OverheadWarmup   2: 67108864 op, 134460944.00 ns, 2.0036 ns/op
OverheadWarmup   3: 67108864 op, 128148548.00 ns, 1.9096 ns/op
OverheadWarmup   4: 67108864 op, 129469429.00 ns, 1.9292 ns/op
OverheadWarmup   5: 67108864 op, 129374456.00 ns, 1.9278 ns/op
OverheadWarmup   6: 67108864 op, 128910636.00 ns, 1.9209 ns/op
OverheadWarmup   7: 67108864 op, 128154008.00 ns, 1.9096 ns/op
OverheadWarmup   8: 67108864 op, 129465813.00 ns, 1.9292 ns/op
OverheadWarmup   9: 67108864 op, 129972889.00 ns, 1.9367 ns/op
OverheadWarmup  10: 67108864 op, 135158356.00 ns, 2.0140 ns/op

OverheadActual   1: 67108864 op, 157655932.00 ns, 2.3493 ns/op
OverheadActual   2: 67108864 op, 131960793.00 ns, 1.9664 ns/op
OverheadActual   3: 67108864 op, 129635622.00 ns, 1.9317 ns/op
OverheadActual   4: 67108864 op, 130312846.00 ns, 1.9418 ns/op
OverheadActual   5: 67108864 op, 130058391.00 ns, 1.9380 ns/op
OverheadActual   6: 67108864 op, 129870358.00 ns, 1.9352 ns/op
OverheadActual   7: 67108864 op, 128134490.00 ns, 1.9094 ns/op
OverheadActual   8: 67108864 op, 129472315.00 ns, 1.9293 ns/op
OverheadActual   9: 67108864 op, 128818958.00 ns, 1.9196 ns/op
OverheadActual  10: 67108864 op, 129338415.00 ns, 1.9273 ns/op
OverheadActual  11: 67108864 op, 127784752.00 ns, 1.9041 ns/op
OverheadActual  12: 67108864 op, 128856364.00 ns, 1.9201 ns/op
OverheadActual  13: 67108864 op, 129489188.00 ns, 1.9295 ns/op
OverheadActual  14: 67108864 op, 133986864.00 ns, 1.9966 ns/op
OverheadActual  15: 67108864 op, 130410095.00 ns, 1.9433 ns/op

WorkloadWarmup   1: 67108864 op, 746247695.00 ns, 11.1200 ns/op
WorkloadWarmup   2: 67108864 op, 721332550.00 ns, 10.7487 ns/op
WorkloadWarmup   3: 67108864 op, 696712306.00 ns, 10.3818 ns/op

// BeforeActualRun
WorkloadActual   1: 67108864 op, 717254412.00 ns, 10.6879 ns/op
WorkloadActual   2: 67108864 op, 710717254.00 ns, 10.5905 ns/op
WorkloadActual   3: 67108864 op, 700107376.00 ns, 10.4324 ns/op
WorkloadActual   4: 67108864 op, 705414907.00 ns, 10.5115 ns/op
WorkloadActual   5: 67108864 op, 699635600.00 ns, 10.4254 ns/op
WorkloadActual   6: 67108864 op, 702404353.00 ns, 10.4666 ns/op
WorkloadActual   7: 67108864 op, 703742391.00 ns, 10.4866 ns/op
WorkloadActual   8: 67108864 op, 715817012.00 ns, 10.6665 ns/op

// AfterActualRun
WorkloadResult   1: 67108864 op, 587618790.00 ns, 8.7562 ns/op
WorkloadResult   2: 67108864 op, 581081632.00 ns, 8.6588 ns/op
WorkloadResult   3: 67108864 op, 570471754.00 ns, 8.5007 ns/op
WorkloadResult   4: 67108864 op, 575779285.00 ns, 8.5798 ns/op
WorkloadResult   5: 67108864 op, 569999978.00 ns, 8.4937 ns/op
WorkloadResult   6: 67108864 op, 572768731.00 ns, 8.5349 ns/op
WorkloadResult   7: 67108864 op, 574106769.00 ns, 8.5549 ns/op
WorkloadResult   8: 67108864 op, 586181390.00 ns, 8.7348 ns/op
// GC:  32 0 0 1610612736 67108864
// Threading:  0 0 67108864

// AfterAll
// Benchmark Process 2422887 has exited with code 0.

Mean = 8.602 ns, StdErr = 0.036 ns (0.42%), N = 8, StdDev = 0.103 ns
Min = 8.494 ns, Q1 = 8.526 ns, Median = 8.567 ns, Q3 = 8.678 ns, Max = 8.756 ns
IQR = 0.151 ns, LowerFence = 8.299 ns, UpperFence = 8.905 ns
ConfidenceInterval = [8.405 ns; 8.798 ns] (CI 99.9%), Margin = 0.196 ns (2.28% of Mean)
Skewness = 0.41, Kurtosis = 1.3, MValue = 2

// ** Remained 0 (0.0 %) benchmark(s) to run. Estimated finish 2026-10-13 22:15 (0h 0m from now) **
// ***** BenchmarkRunner: Finish  *****

// * Export *
  BenchmarkDotNet.Artifacts/results/MediatorBenchmarks.DispatchBenchmarks-report.csv
  BenchmarkDotNet.Artifacts/results/MediatorBenchmarks.DispatchBenchmarks-report-github.md
  BenchmarkDotNet.Artifacts/results/MediatorBenchmarks.DispatchBenchmarks-report.html

// * Detailed results *
DispatchBenchmarks.TimeWarpLegacyMakeGenericType: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 65.224 ns, StdErr = 2.718 ns (4.17%), N = 7, StdDev = 7.191 ns
Min = 56.997 ns, Q1 = 60.047 ns, Median = 64.829 ns, Q3 = 69.871 ns, Max = 74.907 ns
IQR = 9.825 ns, LowerFence = 45.310 ns, UpperFence = 84.609 ns
ConfidenceInterval = [49.028 ns; 81.420 ns] (CI 99.9%), Margin = 16.196 ns (24.83% of Mean)
Skewness = 0.22, Kurtosis = 1.27, MValue = 2.8
-------------------- Histogram --------------------
[56.325 ns ; 66.193 ns) | @@@@@
[66.193 ns ; 69.630 ns) | 
[69.630 ns ; 79.841 ns) | @@
---------------------------------------------------

DispatchBenchmarks.TimeWarpGeneratedMonomorphicSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 29.910 ns, StdErr = 0.047 ns (0.16%), N = 8, StdDev = 0.133 ns
Min = 29.713 ns, Q1 = 29.821 ns, Median = 29.915 ns, Q3 = 30.024 ns, Max = 30.069 ns
IQR = 0.203 ns, LowerFence = 29.517 ns, UpperFence = 30.328 ns
ConfidenceInterval = [29.655 ns; 30.165 ns] (CI 99.9%), Margin = 0.255 ns (0.85% of Mean)
Skewness = -0.16, Kurtosis = 1.18, MValue = 2
-------------------- Histogram --------------------
[29.626 ns ; 30.157 ns) | @@@@@@@@
---------------------------------------------------

DispatchBenchmarks.TimeWarpGeneratedInterfaceSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 64.771 ns, StdErr = 3.466 ns (5.35%), N = 7, StdDev = 9.171 ns
Min = 58.893 ns, Q1 = 59.792 ns, Median = 60.592 ns, Q3 = 64.838 ns, Max = 84.648 ns
IQR = 5.046 ns, LowerFence = 52.223 ns, UpperFence = 72.407 ns
ConfidenceInterval = [44.115 ns; 85.426 ns] (CI 99.9%), Margin = 20.656 ns (31.89% of Mean)
Skewness = 1.36, Kurtosis = 3.21, MValue = 2.4
-------------------- Histogram --------------------
[56.568 ns ; 69.153 ns) | @@@@@@
[69.153 ns ; 78.356 ns) | 
[78.356 ns ; 90.941 ns) | @
---------------------------------------------------

DispatchBenchmarks.TimeWarpGeneratedObjectSend: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 50.531 ns, StdErr = 3.238 ns (6.41%), N = 8, StdDev = 9.157 ns
Min = 40.865 ns, Q1 = 43.800 ns, Median = 47.167 ns, Q3 = 58.756 ns, Max = 64.624 ns
IQR = 14.955 ns, LowerFence = 21.368 ns, UpperFence = 81.188 ns
ConfidenceInterval = [33.023 ns; 68.040 ns] (CI 99.9%), Margin = 17.509 ns (34.65% of Mean)
Skewness = 0.35, Kurtosis = 1.21, MValue = 3.2
-------------------- Histogram --------------------
[38.861 ns ; 50.880 ns) | @@@@@
[50.880 ns ; 55.478 ns) | 
[55.478 ns ; 67.497 ns) | @@@
---------------------------------------------------

DispatchBenchmarks.TimeWarpGeneratedInterfaceSendUnit: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 43.855 ns, StdErr = 0.765 ns (1.74%), N = 8, StdDev = 2.164 ns
Min = 41.786 ns, Q1 = 42.151 ns, Median = 43.313 ns, Q3 = 44.651 ns, Max = 47.660 ns
IQR = 2.500 ns, LowerFence = 38.401 ns, UpperFence = 48.401 ns
ConfidenceInterval = [39.718 ns; 47.992 ns] (CI 99.9%), Margin = 4.137 ns (9.43% of Mean)
Skewness = 0.67, Kurtosis = 1.71, MValue = 2.67
-------------------- Histogram --------------------
[41.484 ns ; 44.325 ns) | @@@@@@
[44.325 ns ; 45.678 ns) | 
[45.678 ns ; 49.081 ns) | @@
---------------------------------------------------

DispatchBenchmarks.TimeWarpGeneratedMonomorphicSendUnit: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 24.135 ns, StdErr = 0.061 ns (0.25%), N = 8, StdDev = 0.171 ns
Min = 23.905 ns, Q1 = 24.042 ns, Median = 24.151 ns, Q3 = 24.223 ns, Max = 24.398 ns
IQR = 0.181 ns, LowerFence = 23.770 ns, UpperFence = 24.494 ns
ConfidenceInterval = [23.807 ns; 24.463 ns] (CI 99.9%), Margin = 0.328 ns (1.36% of Mean)
Skewness = -0.06, Kurtosis = 1.56, MValue = 2
-------------------- Histogram --------------------
[23.792 ns ; 24.425 ns) | @@@@@@@@
---------------------------------------------------

DispatchBenchmarks.TimeWarpCallSiteInliningPrototype: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 29.435 ns, StdErr = 0.135 ns (0.46%), N = 8, StdDev = 0.382 ns
Min = 28.797 ns, Q1 = 29.227 ns, Median = 29.402 ns, Q3 = 29.772 ns, Max = 29.907 ns
IQR = 0.545 ns, LowerFence = 28.409 ns, UpperFence = 30.590 ns
ConfidenceInterval = [28.706 ns; 30.165 ns] (CI 99.9%), Margin = 0.730 ns (2.48% of Mean)
Skewness = -0.23, Kurtosis = 1.5, MValue = 2
-------------------- Histogram --------------------
[28.546 ns ; 30.158 ns) | @@@@@@@@
---------------------------------------------------

DispatchBenchmarks.MartinothamarGenerated: Job-LGHQEI(IterationCount=8, WarmupCount=3)
Runtime = .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4; GC = Concurrent Workstation
Mean = 8.602 ns, StdErr = 0.036 ns (0.42%), N = 8, StdDev = 0.103 ns
Min = 8.494 ns, Q1 = 8.526 ns, Median = 8.567 ns, Q3 = 8.678 ns, Max = 8.756 ns
IQR = 0.151 ns, LowerFence = 8.299 ns, UpperFence = 8.905 ns
ConfidenceInterval = [8.405 ns; 8.798 ns] (CI 99.9%), Margin = 0.196 ns (2.28% of Mean)
Skewness = 0.41, Kurtosis = 1.3, MValue = 2
-------------------- Histogram --------------------
[8.451 ns ; 8.795 ns) | @@@@@@@@
---------------------------------------------------

// * Summary *

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04 LTS (Resolute Raccoon)
AMD Ryzen 9 7950X3D 4.19GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4
  Job-LGHQEI : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4

IterationCount=8  WarmupCount=3  

| Method                               | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| TimeWarpLegacyMakeGenericType        | 65.224 ns | 16.1958 ns | 7.1910 ns |  1.01 |    0.15 | 0.0044 |     224 B |        1.00 |
| TimeWarpGeneratedMonomorphicSend     | 29.910 ns |  0.2546 ns | 0.1332 ns |  0.46 |    0.05 | 0.0019 |      96 B |        0.43 |
| TimeWarpGeneratedInterfaceSend       | 64.771 ns | 20.6556 ns | 9.1712 ns |  1.00 |    0.17 | 0.0048 |     240 B |        1.07 |
| TimeWarpGeneratedObjectSend          | 50.531 ns | 17.5086 ns | 9.1574 ns |  0.78 |    0.16 | 0.0033 |     168 B |        0.75 |
| TimeWarpGeneratedInterfaceSendUnit   | 43.855 ns |  4.1373 ns | 2.1639 ns |  0.68 |    0.08 | 0.0019 |      96 B |        0.43 |
| TimeWarpGeneratedMonomorphicSendUnit | 24.135 ns |  0.3279 ns | 0.1715 ns |  0.37 |    0.04 |      - |         - |        0.00 |
| TimeWarpCallSiteInliningPrototype    | 29.435 ns |  0.7296 ns | 0.3816 ns |  0.46 |    0.05 | 0.0019 |      96 B |        0.43 |
| MartinothamarGenerated               |  8.602 ns |  0.1964 ns | 0.1027 ns |  0.13 |    0.01 | 0.0005 |      24 B |        0.11 |

// * Hints *
Outliers
  DispatchBenchmarks.TimeWarpLegacyMakeGenericType: IterationCount=8, WarmupCount=3  -> 1 outlier  was  removed (109.71 ns)
  DispatchBenchmarks.TimeWarpGeneratedInterfaceSend: IterationCount=8, WarmupCount=3 -> 1 outlier  was  removed (114.83 ns)

// * Legends *
  Mean        : Arithmetic mean of all measurements
  Error       : Half of 99.9% confidence interval
  StdDev      : Standard deviation of all measurements
  Ratio       : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD     : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Gen0        : GC Generation 0 collects per 1000 operations
  Allocated   : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  Alloc Ratio : Allocated memory ratio distribution ([Current]/[Baseline])
  1 ns        : 1 Nanosecond (0.000000001 sec)

// * Diagnostic Output - MemoryDiagnoser *


// ***** BenchmarkRunner: End *****
Run time: 00:01:32 (92.07 sec), executed benchmarks: 8

Global total time: 00:01:44 (104.9 sec), executed benchmarks: 8
// * Artifacts cleanup *
Artifacts cleanup is finished
