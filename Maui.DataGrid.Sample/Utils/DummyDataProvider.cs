namespace Maui.DataGrid.Sample.Utils;

using System.Reflection;
using Maui.DataGrid.Sample.Models;
using System.Text.Json;

internal static class DummyDataProvider
{
    private static readonly Random RandomNumber = new();

    private static List<Team>? _realTeams;

    public static List<Team> GetTeams(int numberOfCopies = 1)
    {
        if (_realTeams == null)
        {
            var assembly = typeof(DummyDataProvider).GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream("Maui.DataGrid.Sample.teams.json")
                ?? throw new FileNotFoundException("Could not load teams.json");

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            _realTeams = JsonSerializer.Deserialize<List<Team>>(json)
                ?? throw new InvalidOperationException("Could not deserialize teams.json");
        }

        if (numberOfCopies == 1)
        {
            return _realTeams;
        }

        var teams = new List<Team>(_realTeams);

        for (var i = 0; i < numberOfCopies; i++)
        {
            foreach (var realTeam in _realTeams)
            {
                var randomTeam = new Team
                {
                    Name = $"{realTeam.Name} {i}",
                    Won = RandomNumber.Next(0, 50),
                    Lost = RandomNumber.Next(0, 50),
                    Percentage = Math.Round(RandomNumber.NextDouble() * 100) / 100,
                    Conf = $"{realTeam.Conf} {RandomNumber.Next(1, 10)}",
                    Div = $"{realTeam.Div} {RandomNumber.Next(1, 10)}",
                    Home = $"{RandomNumber.Next(1, 10)}",
                    Road = $"{RandomNumber.Next(1, 10)}",
                    Last10 = $"{RandomNumber.Next(1, 10)}",
                    Streak = new Streak
                    {
                        Result = (Result)RandomNumber.Next(0, 2),
                        NumStreak = RandomNumber.Next(0, 10)
                    },
                    Logo = realTeam.Logo,
                };

                teams.Add(randomTeam);
            }
        }

        return teams;
    }
}
