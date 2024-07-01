namespace Maui.DataGrid.Sample.Tests.TestUtils;

using System.ComponentModel;

internal sealed class SingleVM<T> : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private T? _item;

    public T? Item
    {
        get => _item;
        set
        {
            _item = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Item)));
        }
    }

    internal int NumberOfSubscribers => PropertyChanged?.GetInvocationList()?.Length ?? 0;
}
