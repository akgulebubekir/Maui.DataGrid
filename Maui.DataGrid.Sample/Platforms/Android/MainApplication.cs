namespace Maui.DataGrid.Sample.Platforms.Android;

using global::Android.App;
using global::Android.Runtime;

[Application]
#pragma warning disable CA1515 // Android instantiates the application via the [Application] attribute; entry point kept public to match the MAUI template.
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
#pragma warning restore CA1515
{
    /// <inheritdoc/>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
