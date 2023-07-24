namespace Maui.DataGrid.Extensions;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using static Microsoft.Maui.Controls.BindableProperty;

internal static class BindablePropertyExtensions
{
    public static BindableProperty Create<TType>(
        TType? defaultValue = default,
        BindingMode defaultBindingMode = BindingMode.OneWay,
        ValidateValueDelegate<TType?>? validateValue = null,
        BindingPropertyChangedDelegate<TType?>? propertyChanged = null,
        BindingPropertyChangingDelegate<TType?>? propertyChanging = null,
        CoerceValueDelegate<TType?>? coerceValue = null,
        CreateDefaultValueDelegate? defaultValueCreator = null,
        [CallerMemberName] string propertyName = "")
    {
        if (!propertyName.EndsWith("Property", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("This extension must be used on a BindableProperty whose name is suffixed with the word 'Property'");
        }

        var trimmedPropertyName = propertyName[..^8];

        if (new StackTrace().GetFrame(1) is not StackFrame callerFrame)
        {
            throw new ArgumentException("StackFrame not found");
        }

        if (callerFrame.GetMethod() is not MethodBase callerMethod)
        {
            throw new ArgumentException("Caller method not found");
        }

        var callerType = callerMethod.DeclaringType;

        ValidateValueDelegate? untypedValidateValue = null;
        if (validateValue != null)
        {
            untypedValidateValue = (b, v) => validateValue(b, v is TType typedValue ? typedValue : default);
        }

        BindingPropertyChangedDelegate? untypedPropertyChanged = null;
        if (propertyChanged != null)
        {
            untypedPropertyChanged = (b, o, n) => propertyChanged(b, o is TType typedOldValue ? typedOldValue : default, n is TType typedNewValue ? typedNewValue : default);
        }

        BindingPropertyChangingDelegate? untypedPropertyChanging = null;
        if (propertyChanging != null)
        {
            untypedPropertyChanging = (b, o, n) => propertyChanging(b, o is TType typedOldValue ? typedOldValue : default, n is TType typedNewValue ? typedNewValue : default);
        }

        CoerceValueDelegate? untypedCoerceValue = null;
        if (coerceValue != null)
        {
            untypedCoerceValue = (b, v) => coerceValue(b, v is TType typedValue ? typedValue : default);
        }

        return BindableProperty.Create(
            trimmedPropertyName,
            typeof(TType),
            callerType,
            defaultValue,
            defaultBindingMode,
            untypedValidateValue,
            untypedPropertyChanged,
            untypedPropertyChanging,
            untypedCoerceValue,
            defaultValueCreator);
    }
}
