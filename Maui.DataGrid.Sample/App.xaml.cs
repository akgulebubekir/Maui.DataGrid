namespace Maui.DataGrid.Sample;

/// <summary>
/// Codebehind for the App.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class App
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
