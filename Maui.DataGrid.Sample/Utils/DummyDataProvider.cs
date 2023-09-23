namespace Maui.DataGrid.Sample.Utils;

using System.Reflection;
using Maui.DataGrid.Sample.Models;
using System.Text.Json;
using System.Data;

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
    public static DataView GetDataTableTeams()
    {
        var assembly = typeof(DummyDataProvider).GetTypeInfo().Assembly;

        using var stream = assembly.GetManifestResourceStream("Maui.DataGrid.Sample.teams.json");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var dataTable = new DataTable();
        var properties = typeof(Team).GetProperties();
        foreach (var property in properties)
        {
            dataTable.Columns.Add(new DataColumn(property.Name, typeof(object)));
        }

        var teams = JsonSerializer.Deserialize<List<Team>>(json);
        foreach (var team in teams)
        {
            List<object> strings = new List<object>();
            foreach (var property in properties)
            {
                strings.Add(property.GetValue(team));
            }
            dataTable.Rows.Add(strings.ToArray());
        }
        return dataTable.DefaultView;
    }
}
