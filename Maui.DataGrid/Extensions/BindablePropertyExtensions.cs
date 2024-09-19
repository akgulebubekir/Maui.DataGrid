namespace Maui.DataGrid.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using static Microsoft.Maui.Controls.BindableProperty;

internal static class BindablePropertyExtensions
{
    public static BindableProperty Create<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)] TDeclaringType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TReturnType>(
        TReturnType? defaultValue = default,
        BindingMode defaultBindingMode = BindingMode.OneWay,
        ValidateValueDelegate<TReturnType?>? validateValue = null,
        BindingPropertyChangedDelegate<TReturnType?>? propertyChanged = null,
        BindingPropertyChangingDelegate<TReturnType?>? propertyChanging = null,
        CoerceValueDelegate<TReturnType?>? coerceValue = null,
        CreateDefaultValueDelegate<TDeclaringType?, TReturnType?>? defaultValueCreator = null,
        [CallerMemberName] string propertyName = "")
    {
        if (!propertyName.EndsWith("Property", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("This extension must be used on a BindableProperty whose name is suffixed with the word 'Property'");
        }

        var trimmedPropertyName = propertyName[..^8];

        ValidateValueDelegate? untypedValidateValue;
        if (validateValue != null)
        {
            untypedValidateValue = (bindable, value) => validateValue(bindable, value is TReturnType typedValue ? typedValue : default);
        }
        else
        {
            untypedValidateValue = null;
        }

        BindingPropertyChangedDelegate? untypedPropertyChanged;
        if (propertyChanged != null)
        {
            untypedPropertyChanged = (bindable, o, n) => propertyChanged(bindable, o is TReturnType typedOldValue ? typedOldValue : default, n is TReturnType typedNewValue ? typedNewValue : default);
        }
        else
        {
            untypedPropertyChanged = null;
        }

        BindingPropertyChangingDelegate? untypedPropertyChanging;
        if (propertyChanging != null)
        {
            untypedPropertyChanging = (bindable, o, n) => propertyChanging(bindable, o is TReturnType typedOldValue ? typedOldValue : default, n is TReturnType typedNewValue ? typedNewValue : default);
        }
        else
        {
            untypedPropertyChanging = null;
        }

        CoerceValueDelegate? untypedCoerceValue;
        if (coerceValue != null)
        {
            untypedCoerceValue = (bindable, value) => coerceValue(bindable, value is TReturnType typedValue ? typedValue : default);
        }
        else
        {
            untypedCoerceValue = null;
        }

        CreateDefaultValueDelegate? untypedDefaultValueCreator;
        if (defaultValueCreator != null)
        {
            untypedDefaultValueCreator = (bindable) => defaultValueCreator(bindable is TDeclaringType typedBindable ? typedBindable : default);
        }
        else
        {
            untypedDefaultValueCreator = null;
        }

        return BindableProperty.Create(
            trimmedPropertyName,
            typeof(TReturnType),
            typeof(TDeclaringType),
            defaultValue,
            defaultBindingMode,
            untypedValidateValue,
            untypedPropertyChanged,
            untypedPropertyChanging,
            untypedCoerceValue,
            untypedDefaultValueCreator);
    }
}
