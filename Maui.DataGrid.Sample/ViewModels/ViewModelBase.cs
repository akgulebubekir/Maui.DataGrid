namespace Maui.DataGrid.Sample.ViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

internal abstract class ViewModelBase : INotifyPropertyChanged
{
    private readonly Dictionary<string, object?> _properties = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    public Dictionary<string, ICommand> Commands { get; } = [];

    protected bool SetValue(object? value, [CallerMemberName] string propertyName = null!)
    {
        if (_properties.TryGetValue(propertyName!, out var item) && item == value)
        {
            return false;
        }

        _properties[propertyName!] = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    protected T? GetValue<T>([CallerMemberName] string propertyName = null!)
    {
        if (_properties.TryGetValue(propertyName!, out var value))
        {
            return (T?)value;
        }

        return default;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
