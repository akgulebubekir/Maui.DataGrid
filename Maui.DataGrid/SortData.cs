using System.ComponentModel;

namespace Maui.DataGrid;

[TypeConverter(typeof(SortDataTypeConverter))]
public class SortData
{
    public static implicit operator SortData(int index)
    {
        return new SortData
        {
            Index = Math.Abs(index),
            Order = index < 0 ? SortingOrder.Descendant : SortingOrder.Ascendant
        };
    }

    public override bool Equals(object obj)
    {
        if (obj is SortData other)
        {
            return other.Index == Index && other.Order == Order;
        }

        return false;
    }

    #region ctor

    public SortData()
    { }

    public SortData(int index, SortingOrder order)
    {
        Index = index;
        Order = order;
    }

    #endregion

    #region Properties

    public SortingOrder Order { get; set; }

    public int Index { get; set; }

    #endregion
}