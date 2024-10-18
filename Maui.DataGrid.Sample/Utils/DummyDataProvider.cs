namespace Maui.DataGrid.Sample.Utils;

using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Maui.DataGrid.Sample.Models;

internal static class DummyDataProvider
{
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
                    Won = RandomNumberGenerator.GetInt32(0, 50),
                    Lost = RandomNumberGenerator.GetInt32(0, 50),
                    Percentage = Math.Round(RandomDouble() * 100) / 100,
                    Conf = $"{realTeam.Conf} {RandomNumberGenerator.GetInt32(1, 10)}",
                    Div = $"{realTeam.Div} {RandomNumberGenerator.GetInt32(1, 10)}",
                    Home = $"{RandomNumberGenerator.GetInt32(1, 10)}",
                    Road = $"{RandomNumberGenerator.GetInt32(1, 10)}",
                    Last10 = $"{RandomNumberGenerator.GetInt32(1, 10)}",
                    Streak = new Streak
                    {
                        Result = (GameResult)RandomNumberGenerator.GetInt32(0, 2),
                        NumStreak = RandomNumberGenerator.GetInt32(0, 10),
                    },
                    Logo = realTeam.Logo,
                };

                teams.Add(randomTeam);
            }
        }

        return teams;
    }

    private static double RandomDouble() => (double)RandomNumberGenerator.GetInt32(int.MaxValue) / int.MaxValue;
}
