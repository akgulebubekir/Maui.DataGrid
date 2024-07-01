namespace Maui.DataGrid.Sample.Models;

public class Team
{
    public required string Name { get; set; }

    public int Won { get; set; }

    public int Lost { get; set; }

    public double Percentage { get; set; }

    public required string Conf { get; set; }

    public required string Div { get; set; }

    public required string Home { get; set; }

    public required string Road { get; set; }

    public required string Last10 { get; set; }

    public required Streak Streak { get; set; }

    public required string Logo { get; set; }
}

public class Streak : IComparable
{
    public Result Result { get; set; }

    public int NumStreak { get; set; }

    public int CompareTo(object? obj)
    {
        if (obj is Streak s)
        {
            // First compare the Result
            var resultComparison = Result.CompareTo(s.Result);
            if (resultComparison != 0)
            {
                return resultComparison;
            }

            var winLossIndicator = Result == Result.Won ? 1 : -1;

            // If Result is the same, then compare the NumStreak
            return winLossIndicator * NumStreak.CompareTo(s.NumStreak);
        }

        throw new ArgumentException("Object is not a Streak");
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Enum.GetName(typeof(Result), Result)} {NumStreak}";
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(Streak left, Streak right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Streak left, Streak right)
    {
        return !(left == right);
    }

    public static bool operator <(Streak left, Streak right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Streak left, Streak right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Streak left, Streak right)
    {
        return left?.CompareTo(right) > 0;
    }

    public static bool operator >=(Streak left, Streak right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }
}

public enum Result
{
    Lost = 0,
    Won = 1,
}
