namespace Maui.DataGrid.Sample.ViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly Dictionary<string, object> _properties = [];

    public Dictionary<string, ICommand> Commands
    {
        get => GetValue<Dictionary<string, ICommand>>();
        private init => SetValue(value);
    }

    protected ViewModelBase()
    {
        Commands = [];
    }

    protected void SetValue(object value, [CallerMemberName] string propertyName = null)
    {
        if (_properties.TryGetValue(propertyName!, out var item) && item == value)
        {
            return;
        }

        _properties[propertyName!] = value;
        OnPropertyChanged(propertyName);
    }

    protected T GetValue<T>([CallerMemberName] string propertyName = null)
    {
        if (_properties.TryGetValue(propertyName!, out var value))
        {
            return (T)value;
        }

        return default;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
