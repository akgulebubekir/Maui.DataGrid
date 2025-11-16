namespace Maui.DataGrid.Sample.Tests.TestUtils;

using System.ComponentModel;

internal sealed class SingleVM<T> : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public T? Item
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Item)));
        }
    }

    internal int NumberOfSubscribers => PropertyChanged?.GetInvocationList()?.Length ?? 0;
}
