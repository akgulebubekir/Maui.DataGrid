namespace Maui.DataGrid.Sample.Platforms.Android;

using global::Android.App;
using global::Android.Content.PM;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
#pragma warning disable CA1515 // Android instantiates the activity via the [Activity] attribute; entry point kept public to match the MAUI template.
public class MainActivity : MauiAppCompatActivity;
#pragma warning restore CA1515
