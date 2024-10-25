using System;
using System.ComponentModel;
using System.Globalization;

namespace AW.Identifiers;

public class UrnTypeConverter : TypeConverter
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        var str = value?.ToString();

        return string.IsNullOrWhiteSpace(str)
            ? Urn.Empty
            : Urn.Parse(str);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        => destinationType == typeof(string)
            ? value?.ToString()
            : base.ConvertTo(context, culture, value, destinationType);

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string)
            ? true
            : base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string)
            ? true
            : base.CanConvertTo(context, destinationType);
}