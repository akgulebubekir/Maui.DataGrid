namespace Maui.DataGrid.Sample.Tests.TestUtils;

using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

internal static class TestExtensions
{
    public static async Task<object> GetValueSafe(this BindableObject bindableObject, BindableProperty property)
    {
        if (Application.Current!.Dispatcher.IsDispatchRequired)
        {
            return await Application.Current.Dispatcher.DispatchAsync(() => bindableObject.GetValue(property));
        }

        return bindableObject.GetValue(property);
    }

    public static async void CheckPropertyBindingWorks<T>(this BindableObject bindableObject, BindableProperty property, T testValue, T? updatedValue)
        where T : notnull
    {
        Assert.Equal(property.DefaultValue, await bindableObject.GetValueSafe(property));

        var viewModel = new SingleVM<T> { Item = testValue };
        bindableObject.SetBinding(property, new Binding(nameof(SingleVM<T>.Item), source: viewModel));

        Assert.Equal(1, viewModel.NumberOfSubscribers);
        Assert.Equal(testValue, await bindableObject.GetValueSafe(property));

        var propertyChangedEventTriggered = false;
        bindableObject.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == property.PropertyName)
            {
                propertyChangedEventTriggered = true;
            }
        };

        viewModel.Item = updatedValue;
        Assert.Equal(updatedValue, await bindableObject.GetValueSafe(property));
        Assert.True(propertyChangedEventTriggered);
    }

    public static async void DispatchIfRequired(this BindableObject bindableObject, Action action)
    {
        if (bindableObject.Dispatcher.IsDispatchRequired)
        {
            await bindableObject.Dispatcher.DispatchAsync(action);
        }
        else
        {
            action();
        }
    }

    internal static void CheckStyleSettingWorks<T>(this NavigableElement element, BindableProperty property, T value)
        where T : notnull
    {
        var style = new Style(element.GetType())
        {
            Setters =
            {
                new Setter() { Property = property, Value = value },
            },
        };

        element.DispatchIfRequired(() =>
        {
            element.Style = style;
            Assert.Equal(value, element.GetValue(property));
        });
    }
}
