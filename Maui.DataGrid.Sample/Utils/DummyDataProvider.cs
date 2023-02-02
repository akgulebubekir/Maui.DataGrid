namespace Maui.DataGrid.Sample.Utils;

using System.Reflection;
using Maui.DataGrid.Sample.Models;
using System.Text.Json;

internal static class DummyDataProvider
{
    public static List<Team> GetTeams()
    {
        var assembly = typeof(DummyDataProvider).GetTypeInfo().Assembly;

        using var stream = assembly.GetManifestResourceStream("Maui.DataGrid.Sample.teams.json");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return JsonSerializer.Deserialize<List<Team>>(json);
    }
}
