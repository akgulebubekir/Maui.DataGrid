namespace Maui.DataGrid.Tests;

using Maui.DataGrid.Extensions;
using Xunit;

public class ReflectionExtensionsTest
{
    private sealed class Deepest
    {
        public int Score { get; set; }
    }

    private sealed class Inner
    {
        public int Value { get; set; }
        public string? Text { get; set; }
        public Deepest? Nested { get; set; }
    }

    private sealed class Outer
    {
        public Inner? Child { get; set; }
        public string? Name { get; set; }
    }

    private interface IShape
    {
    }

    private sealed class Circle : IShape
    {
        public int Radius { get; set; }
    }

    private sealed class Square : IShape
    {
        public int Side { get; set; }
    }

    private sealed class ShapeHolder
    {
        public object? Boxed { get; set; }

        public IShape? Shape { get; set; }
    }

    [Fact]
    public void GetValueByPath_SimpleProperty()
    {
        var obj = new Outer { Name = "test" };

        var result = obj.GetValueByPath("Name");

        Assert.Equal("test", result);
    }

    [Fact]
    public void GetValueByPath_NestedProperty()
    {
        var obj = new Outer { Child = new Inner { Value = 42 } };

        var result = obj.GetValueByPath("Child.Value");

        Assert.Equal(42, result);
    }

    [Fact]
    public void GetValueByPath_DeepNestedStringProperty()
    {
        var obj = new Outer { Child = new Inner { Text = "hello" } };

        var result = obj.GetValueByPath("Child.Text");

        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetValueByPath_NullIntermediateReturnsNull()
    {
        var obj = new Outer { Child = null };

        var result = obj.GetValueByPath("Child.Value");

        Assert.Null(result);
    }

    [Fact]
    public void GetValueByPath_NullObjectReturnsNull()
    {
        object? obj = null;

        var result = obj!.GetValueByPath("Name");

        Assert.Null(result);
    }

    [Fact]
    public void GetValueByPath_EmptyPathReturnsNull()
    {
        var obj = new Outer { Name = "test" };

        var result = obj.GetValueByPath(string.Empty);

        Assert.Null(result);
    }

    [Fact]
    public void GetValueByPath_NonExistentPropertyReturnsNull()
    {
        var obj = new Outer { Name = "test" };

        var result = obj.GetValueByPath("NonExistent");

        Assert.Null(result);
    }

    [Fact]
    public void GetPropertyTypeByPath_SimpleProperty()
    {
        var result = typeof(Outer).GetPropertyTypeByPath("Name");

        Assert.Equal(typeof(string), result);
    }

    [Fact]
    public void GetPropertyTypeByPath_NestedProperty()
    {
        var result = typeof(Outer).GetPropertyTypeByPath("Child.Value");

        Assert.Equal(typeof(int), result);
    }

    [Fact]
    public void GetPropertyTypeByPath_DotReturnsTypeSelf()
    {
        var result = typeof(Outer).GetPropertyTypeByPath(".");

        Assert.Equal(typeof(Outer), result);
    }

    [Fact]
    public void GetPropertyTypeByPath_EmptyPathReturnsTypeSelf()
    {
        var result = typeof(Outer).GetPropertyTypeByPath(string.Empty);

        Assert.Equal(typeof(Outer), result);
    }

    [Fact]
    public void GetPropertyTypeByPath_NonExistentPropertyReturnsNull()
    {
        var result = typeof(Outer).GetPropertyTypeByPath("DoesNotExist");

        Assert.Null(result);
    }

    [Fact]
    public void GetPropertyTypeByPath_NonExistentNestedPropertyReturnsNull()
    {
        var result = typeof(Outer).GetPropertyTypeByPath("Child.DoesNotExist");

        Assert.Null(result);
    }

    [Fact]
    public void GetValueByPath_ThreeLevelDeepProperty()
    {
        var obj = new Outer { Child = new Inner { Nested = new Deepest { Score = 99 } } };

        var result = obj.GetValueByPath("Child.Nested.Score");

        Assert.Equal(99, result);
    }

    [Fact]
    public void GetValueByPath_ThreeLevelDeepNullIntermediateReturnsNull()
    {
        var obj = new Outer { Child = new Inner { Nested = null } };

        var result = obj.GetValueByPath("Child.Nested.Score");

        Assert.Null(result);
    }

    [Fact]
    public void GetPropertyTypeByPath_ThreeLevelDeepProperty()
    {
        var result = typeof(Outer).GetPropertyTypeByPath("Child.Nested.Score");

        Assert.Equal(typeof(int), result);
    }

    [Fact]
    public void GetValueByPath_CachesPropertyPath()
    {
        var obj1 = new Outer { Child = new Inner { Value = 10 } };
        var obj2 = new Outer { Child = new Inner { Value = 20 } };

        var result1 = obj1.GetValueByPath("Child.Value");
        var result2 = obj2.GetValueByPath("Child.Value");

        Assert.Equal(10, result1);
        Assert.Equal(20, result2);
    }

    [Fact]
    public void GetValueByPath_ResolvesDerivedPropertyWhenIntermediateDeclaredAsObject()
    {
        var obj = new ShapeHolder { Boxed = new Circle { Radius = 5 } };

        var result = obj.GetValueByPath("Boxed.Radius");

        Assert.Equal(5, result);
    }

    [Fact]
    public void GetValueByPath_ResolvesDerivedPropertyWhenIntermediateDeclaredAsInterface()
    {
        var obj = new ShapeHolder { Shape = new Circle { Radius = 9 } };

        var result = obj.GetValueByPath("Shape.Radius");

        Assert.Equal(9, result);
    }

    [Fact]
    public void GetValueByPath_ResolvesDistinctRuntimeTypesForSameDeclaredSlot()
    {
        var square = new ShapeHolder { Boxed = new Square { Side = 3 } };
        var circle = new ShapeHolder { Boxed = new Circle { Radius = 4 } };

        Assert.Equal(3, square.GetValueByPath("Boxed.Side"));
        Assert.Equal(4, circle.GetValueByPath("Boxed.Radius"));
    }
}
