#pragma warning disable IDE1006 // Naming Styles
namespace Maui.DataGrid.Sample.Platforms.iOS;
#pragma warning restore IDE1006 // Naming Styles

using Foundation;

[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class AppDelegate : MauiUIApplicationDelegate
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <inheritdoc/>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
