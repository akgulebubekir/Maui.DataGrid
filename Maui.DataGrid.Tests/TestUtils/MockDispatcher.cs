namespace Maui.DataGrid.Tests.TestUtils;

using System.Collections.Concurrent;
using Microsoft.Maui.Dispatching;

/// <summary>
/// A synchronous <see cref="IDispatcher"/> that lets MAUI controls be exercised
/// on the test thread without a platform, so tests can run under <c>dotnet test</c>.
/// A test which needs to observe marshalling can opt in with <see cref="SimulateMainThread"/>.
/// </summary>
internal sealed class MockDispatcher : IDispatcher
{
    /// <summary>
    /// The value of <see cref="_mainThreadId"/> when no thread is standing in for the main thread.
    /// Managed thread ids start at 1, so zero can never collide with a real one.
    /// </summary>
    private const int NoMainThread = 0;

    private readonly ConcurrentQueue<Action> _dispatchedFromOtherThreads = new();

    private volatile int _mainThreadId = NoMainThread;

    /// <summary>
    /// Gets a value indicating whether the caller is off the simulated main thread. Unless a test has
    /// called <see cref="SimulateMainThread"/> there is no main thread to be off of, so every caller
    /// keeps running inline.
    /// </summary>
    public bool IsDispatchRequired
    {
        get
        {
            var mainThreadId = _mainThreadId;
            return mainThreadId != NoMainThread && mainThreadId != Environment.CurrentManagedThreadId;
        }
    }

    public bool Dispatch(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsDispatchRequired)
        {
            _dispatchedFromOtherThreads.Enqueue(action);
        }
        else
        {
            action();
        }

        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);

    public IDispatcherTimer CreateTimer() => new MockDispatcherTimer(this);

    /// <summary>
    /// Treats the calling thread as the platform's main thread until the returned scope is disposed.
    /// Work dispatched from any other thread is then queued rather than run inline, exactly as it would
    /// be on a device, and <see cref="RunQueuedDispatches"/> stands in for the main loop draining it.
    /// </summary>
    /// <returns>A scope which restores the default run-everything-inline behaviour.</returns>
    internal IDisposable SimulateMainThread()
    {
        _mainThreadId = Environment.CurrentManagedThreadId;
        return new MainThreadScope(this);
    }

    /// <summary>
    /// Runs the work which other threads have dispatched to the main thread.
    /// </summary>
    internal void RunQueuedDispatches()
    {
        while (_dispatchedFromOtherThreads.TryDequeue(out var action))
        {
            action();
        }
    }

    private sealed class MainThreadScope(MockDispatcher dispatcher) : IDisposable
    {
        public void Dispose()
        {
            dispatcher._mainThreadId = NoMainThread;
            dispatcher._dispatchedFromOtherThreads.Clear();
        }
    }

    private sealed class MockDispatcherTimer(IDispatcher dispatcher) : IDispatcherTimer, IDisposable
    {
        private readonly IDispatcher _dispatcher = dispatcher;
        private Timer? _timer;

        public event EventHandler? Tick;

        public TimeSpan Interval { get; set; }

        public bool IsRepeating { get; set; }

        public bool IsRunning => _timer is not null;

        public void Start()
        {
            _timer = new Timer(
                _ => _dispatcher.Dispatch(() => Tick?.Invoke(this, EventArgs.Empty)),
                null,
                Interval,
                IsRepeating ? Interval : Timeout.InfiniteTimeSpan);
        }

        public void Stop() => Dispose();

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
