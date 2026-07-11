using Xunit;

// MAUI keeps process-wide static state (Application.Current and the dispatcher provider),
// so tests run sequentially to stay deterministic.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
