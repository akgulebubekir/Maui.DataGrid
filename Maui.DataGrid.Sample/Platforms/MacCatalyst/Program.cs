namespace Maui.DataGrid.Sample.Platforms.MacCatalyst;

using UIKit;

internal static class Program
{
    /// <summary>
    /// This is the main entry point of the application.
    /// </summary>
    /// <param name="args">The arguments for the program.</param>
    private static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
