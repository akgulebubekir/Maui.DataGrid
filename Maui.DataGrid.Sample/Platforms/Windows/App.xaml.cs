// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Maui.DataGrid.Sample.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
#pragma warning disable CA1515 // WinUI requires the Application class to be public; the XAML compiler rejects an internal application type.
public partial class App : MauiWinUIApplication
#pragma warning restore CA1515
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// This is supposed to be a singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
