// ShouldThrowExceptionWhenTimeoutOccurs relies on a 1000 ms registration timeout; running
// test collections in parallel skews that timing, so keep the suite serial.
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]