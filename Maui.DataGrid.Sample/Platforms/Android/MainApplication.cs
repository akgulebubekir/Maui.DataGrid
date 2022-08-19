using Android.App;
using Android.Runtime;

namespace Maui.DataGrid.Sample.Platforms.Android;

[Application]
public class MainApplication : MauiApplication
{
  public MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : base(handle, ownership)
  {
  }

  protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
