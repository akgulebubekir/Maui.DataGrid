namespace Maui.DataGrid.Sample.Platforms.iOS;

using UIKit;

#pragma warning disable CA1515 // CA1515: entry point kept public to match the MAUI template and the WinUI head, which requires a public application type.
public static class Program
#pragma warning restore CA1515
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
