namespace Maui.DataGrid.Tests.TestUtils;

using Microsoft.Maui.Dispatching;

/// <summary>
/// Supplies the <see cref="MockDispatcher"/> for the current thread.
/// </summary>
internal sealed class MockDispatcherProvider(MockDispatcher dispatcher) : IDispatcherProvider
{
    public IDispatcher GetForCurrentThread() => dispatcher;
}
