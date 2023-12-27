namespace Maui.DataGrid.Sample.Models;

public class Team
{
    public string Name { get; set; }
    public int Won { get; set; }
    public int Lost { get; set; }
    public double Percentage { get; set; }
    public string Conf { get; set; }
    public string Div { get; set; }
    public string Home { get; set; }
    public string Road { get; set; }
    public string Last10 { get; set; }
    public Streak Streak { get; set; }
    public string Logo { get; set; }
}

public class Streak : IComparable
{
    public Result Result { get; set; }
    public int NumStreak { get; set; }

    public int CompareTo(object obj)
    {
        var score = Result == Result.Won ? NumStreak : -NumStreak;
        if (obj is Streak s)
        {
            var otherScore = s.Result == Result.Won ? s.NumStreak : -s.NumStreak;
            return score - otherScore;
        }

        return score;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Enum.GetName(typeof(Result), Result)} {NumStreak}";
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
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
    Won = 1
}
