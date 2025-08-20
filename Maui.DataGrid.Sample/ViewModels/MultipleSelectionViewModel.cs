namespace Maui.DataGrid.Sample.ViewModels;

using Maui.DataGrid.Sample.Models;
using Maui.DataGrid.Sample.Utils;

using System.Diagnostics;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Core.Extensions;

#nullable disable
internal sealed class MultipleSelectionViewModel : MainViewModel
{
    public MultipleSelectionViewModel()
    {
        var oc = new ObservableCollection<object>();
        oc.CollectionChanged += SelectedTeams_CollectionChanged;
        SelectedTeams = oc;
    }

    void SelectedTeams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var action = e.Action;
    }

    public IList<object> SelectedTeams
    {
        get => GetValue<IList<object>>();
        set => SetValue(value);
    }
}
