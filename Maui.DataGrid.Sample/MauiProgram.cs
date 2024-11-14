namespace Maui.DataGrid.Sample;

#if TEST
using Xunit.Runners.Maui;
#else
using CommunityToolkit.Maui;
#endif

internal static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
#if TEST
        builder.ConfigureTests(new TestOptions
        {
            Assemblies =
            {
                typeof(MauiProgram).Assembly,
            },
        })
.UseVisualRunner();
#else

        _ = builder
            .UseMauiApp<App>()
#if DEBUG
            .UseMauiCommunityToolkit()
#else
            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldSuppressExceptionsInAnimations(true);
                options.SetShouldSuppressExceptionsInBehaviors(true);
                options.SetShouldSuppressExceptionsInConverters(true);
            })
#endif
.ConfigureFonts(fonts =>
            {
                _ = fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                _ = fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
#endif

        return builder.Build();
    }
}
