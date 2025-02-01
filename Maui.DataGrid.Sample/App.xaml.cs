namespace Maui.DataGrid.Sample;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

#pragma warning disable CA1724, CA1515

/// <summary>
/// Codebehind for the App.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
internal partial class App
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
