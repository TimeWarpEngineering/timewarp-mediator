// ServiceRegistrar copies MaxTypesClosing and related limits into process-wide statics.
// xUnit parallelizes test collections by default, so a concurrent AddMediator call
// overwrites MaxTypesClosing=0 and ShouldThrowExceptionWhenTimeoutOccurs throws
// ArgumentException (default 100) instead of TimeoutException.
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
