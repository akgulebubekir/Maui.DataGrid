namespace Maui.DataGrid.Sample;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class App
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
