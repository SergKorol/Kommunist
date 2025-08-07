using System.Collections;
using System.Globalization;

namespace Kommunist.Application.Helpers;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case null:
                return false;
            case bool boolValue:
                return boolValue;
            case string stringValue:
                return !string.IsNullOrEmpty(stringValue);
            case IEnumerable enumerable:
                return enumerable.Cast<object>().Any();
        }

        if (value is not IConvertible convertible) return true;
        try
        {
            return System.Convert.ToBoolean(convertible, culture);
        }
        catch (InvalidCastException)
        {
            return true;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            return value switch
            {
                bool boolValue => boolValue,
                string stringValue => bool.TryParse(stringValue, out var result) && result,
                _ => false
            };
        }

        throw new NotSupportedException($"ConvertBack is not supported for target type {targetType}");
    }
}