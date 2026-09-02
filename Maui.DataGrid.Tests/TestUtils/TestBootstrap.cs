namespace Maui.DataGrid.Tests.TestUtils;

using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;

/// <summary>
/// One-time setup that makes MAUI controls usable in a headless <c>dotnet test</c> run
/// by registering a synchronous dispatcher and a current <see cref="Application"/>.
/// </summary>
internal static class TestBootstrap
{
    /// <summary>
    /// Gets the dispatcher which every control in the test run shares, so that a test can decide how
    /// dispatched work is treated.
    /// </summary>
    internal static MockDispatcher Dispatcher { get; } = new();

    [ModuleInitializer]
    internal static void Initialize()
    {
        DispatcherProvider.SetCurrent(new MockDispatcherProvider(Dispatcher));

        // Establishes Application.Current, which the binding tests rely on.
        _ = new Application();
    }
}
