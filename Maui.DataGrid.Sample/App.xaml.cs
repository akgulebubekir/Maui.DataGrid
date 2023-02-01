namespace Maui.DataGrid.Sample;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class App
{
    public App()
    {
        this.InitializeComponent();

        this.MainPage = new AppShell();
    }
}
