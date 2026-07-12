namespace Maui.DataGrid.Tests.TestUtils;

using Microsoft.Maui.Dispatching;

/// <summary>
/// Supplies the <see cref="MockDispatcher"/> for the current thread.
/// </summary>
internal sealed class MockDispatcherProvider : IDispatcherProvider
{
    private readonly MockDispatcher _dispatcher = new();

    public IDispatcher GetForCurrentThread() => _dispatcher;
}
