#pragma warning disable IDE1006 // Naming Styles
namespace Maui.DataGrid.Sample.Platforms.iOS;
#pragma warning restore IDE1006 // Naming Styles

using Foundation;

[Register("AppDelegate")]
#pragma warning disable CA1711, CA1515 // CA1711: name is required by the Objective-C runtime. CA1515: entry point kept public to match the MAUI template and the WinUI head, which requires a public application type.
public class AppDelegate : MauiUIApplicationDelegate
#pragma warning restore CA1711, CA1515
{
    /// <inheritdoc/>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
