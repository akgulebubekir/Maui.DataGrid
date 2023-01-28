namespace Maui.DataGrid.Utils;

internal static class MathUtil
{
    internal static int RoundUp(double input)
    {
        if (input < 1)
            return 1;
        else
            return (int)Math.Round(input);
    }
}
