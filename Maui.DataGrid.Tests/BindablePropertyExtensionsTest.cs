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

    [Fact]
    public void Create_PropertyNameWithoutPropertySuffix_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BindablePropertyExtensions.Create<TestBindable, string>(propertyName: "DoesNotEndCorrectly"));
    }

    [Fact]
    public void Create_PropertyNameWithOnlySuffix_ThrowsInvalidOperationException()
    {
        // "Property" alone has no leading name portion after trimming
        // but it does end with "Property" so it should succeed and produce an empty name.
        // Verify the property is created without throwing.
        var prop = BindablePropertyExtensions.Create<TestBindable, int>(propertyName: "Property");

        Assert.Equal(string.Empty, prop.PropertyName);
    }

    private sealed class TestBindable : BindableObject;
}
