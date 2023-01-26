namespace Maui.DataGrid.Utils;

internal static class DataTemplateExtensions
{
    internal static void SetValueIfNeeded(this DataTemplate dataTemplate, BindableProperty property, object newValue)
    {
        dataTemplate.Values.TryGetValue(property, out object oldValue);
        if (newValue != oldValue)
        {
            dataTemplate.SetValue(property, newValue);
        }
    }
}
