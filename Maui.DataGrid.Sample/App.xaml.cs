namespace Maui.DataGrid.Sample;

#pragma warning disable CA1724

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
