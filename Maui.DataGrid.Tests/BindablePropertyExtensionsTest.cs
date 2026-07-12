namespace Maui.DataGrid.Tests;

using Maui.DataGrid.Extensions;
using Xunit;

public class BindablePropertyExtensionsTest
{
    private static readonly BindableProperty TestProperty =
        BindablePropertyExtensions.Create<TestBindable, string>("default");

    private static readonly BindableProperty NumericProperty =
        BindablePropertyExtensions.Create<TestBindable, int>(42);

    [Fact]
    public void Create_SetsDefaultValue()
    {
        var bindable = new TestBindable();

        Assert.Equal("default", bindable.GetValue(TestProperty));
    }

    [Fact]
    public void Create_TrimsPropertySuffix()
    {
        Assert.Equal("Test", TestProperty.PropertyName);
    }

    [Fact]
    public void Create_NumericDefaultValue()
    {
        var bindable = new TestBindable();

        Assert.Equal(42, bindable.GetValue(NumericProperty));
    }

    [Fact]
    public void Create_SetAndGetValue()
    {
        var bindable = new TestBindable();

        bindable.SetValue(TestProperty, "updated");

        Assert.Equal("updated", bindable.GetValue(TestProperty));
    }

    private sealed class TestBindable : BindableObject;
}
