namespace Maui.DataGrid.Sample.Utils;

using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Maui.DataGrid.Sample.Models;

internal static class DummyDataProvider
{
    private static readonly List<Team> RealTeams = LoadTeamsFromResource();

    public static List<Team> GetTeams(int numberOfCopies = 1)
    {
        if (numberOfCopies == 1)
        {
            return RealTeams;
        }

        return GenerateRandomTeams(numberOfCopies);
    }

    private static List<Team> LoadTeamsFromResource()
    {
        var assembly = typeof(DummyDataProvider).GetTypeInfo().Assembly;

        using var stream = assembly.GetManifestResourceStream("Maui.DataGrid.Sample.teams.json")
            ?? throw new FileNotFoundException("Could not load teams.json");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return JsonSerializer.Deserialize<List<Team>>(json)
            ?? throw new InvalidOperationException("Could not deserialize teams.json");
    }

    private static List<Team> GenerateRandomTeams(int numberOfCopies)
    {
        var teams = new List<Team>(RealTeams);

        for (var i = 0; i < numberOfCopies; i++)
        {
            foreach (var realTeam in RealTeams)
            {
                var totalGames = RandomNumberGenerator.GetInt32(72, 83); // Total games in a season
                var won = RandomNumberGenerator.GetInt32(20, Math.Min(60, totalGames)); // Random wins between 20 and the lesser of 60 or total games
                var lost = totalGames - won; // Losses are the remaining games
                var homeWins = RandomNumberGenerator.GetInt32(10, Math.Min(won, totalGames / 2)); // Home wins between 10 and half of total games or total wins
                var roadWins = won - homeWins; // Road wins are the remaining wins
                var last10Wins = RandomNumberGenerator.GetInt32(0, Math.Min(10, won)); // Wins in the last 10 games, but not more than total wins
                var streakResult = (GameResult)RandomNumberGenerator.GetInt32(0, 2); // Random streak result (Win or Loss)
                var streakLength = Math.Min(10, streakResult == GameResult.Won ? won : lost); // Streak length between 0 and 10, but not more than total wins or losses

                var randomTeam = new Team
                {
                    Name = $"{realTeam.Name} {i}",
                    Won = won,
                    Lost = lost,
                    Percentage = (double)won / totalGames,
                    Conf = $"{realTeam.Conf} {RandomNumberGenerator.GetInt32(1, 10)}",
                    Div = $"{realTeam.Div} {RandomNumberGenerator.GetInt32(1, 10)}",
                    Home = $"{homeWins}-{(totalGames / 2) - homeWins}", // Home record
                    Road = $"{roadWins}-{(totalGames / 2) - roadWins}", // Road record
                    Last10 = $"{last10Wins}-{10 - last10Wins}", // Last 10 games record
                    Streak = new Streak
                    {
                        Result = streakResult,
                        NumStreak = streakLength,
                    },
                    Logo = realTeam.Logo,
                };

                teams.Add(randomTeam);
            }
        }

        return teams;
    }
}
