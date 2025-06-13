using System.Collections;
using System.Globalization;

namespace Kommunist.Application.Helpers;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle null values
        if (value == null)
            return false;

        // Handle boolean values directly
        if (value is bool boolValue)
            return boolValue;

        // Handle collections
        if (value is IEnumerable enumerable)
        {
            // Handle strings separately since they're IEnumerable<char>
            if (value is string stringValue)
                return !string.IsNullOrEmpty(stringValue);
            
            // For other collections, check if they have any elements
            return enumerable.Cast<object>().Any();
        }

        // Handle numeric values
        if (value is IConvertible convertible)
        {
            try
            {
                return System.Convert.ToBoolean(convertible, culture);
            }
            catch (InvalidCastException)
            {
                // If conversion fails, treat as truthy if not null
                return true;
            }
        }

        // For all other non-null objects, return true
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // If the target type is bool, convert the visibility back to boolean
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            if (value is bool boolValue)
                return boolValue;
            
            // Handle string representations
            if (value is string stringValue)
                return bool.TryParse(stringValue, out bool result) && result;
            
            return false;
        }

        throw new NotSupportedException($"ConvertBack is not supported for target type {targetType}");
    }
}