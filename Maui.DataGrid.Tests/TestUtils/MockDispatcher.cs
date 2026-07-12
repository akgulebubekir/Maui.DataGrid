namespace Maui.DataGrid.Tests.TestUtils;

using Microsoft.Maui.Dispatching;

/// <summary>
/// A synchronous <see cref="IDispatcher"/> that lets MAUI controls be exercised
/// on the test thread without a platform, so tests can run under <c>dotnet test</c>.
/// </summary>
internal sealed class MockDispatcher : IDispatcher
{
    public bool IsDispatchRequired => false;

    public bool Dispatch(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
        return true;
    }

    public IDispatcherTimer CreateTimer() => new MockDispatcherTimer(this);

    private sealed class MockDispatcherTimer : IDispatcherTimer, IDisposable
    {
        private readonly IDispatcher _dispatcher;
        private Timer? _timer;

        public MockDispatcherTimer(IDispatcher dispatcher) => _dispatcher = dispatcher;

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
