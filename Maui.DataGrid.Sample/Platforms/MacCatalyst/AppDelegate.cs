namespace Maui.DataGrid.Sample.Platforms.MacCatalyst;

using Foundation;

[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class AppDelegate : MauiUIApplicationDelegate
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <inheritdoc/>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
