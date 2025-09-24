using System.Globalization;

namespace Kommunist.Application.Helpers;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool booleanValue;
        if (value is bool b)
        {
            booleanValue = b;
        }
        else
        {
            try
            {
                booleanValue = System.Convert.ToBoolean(value, culture);
            }
            catch
            {
                booleanValue = false;
            }
        }

        var result = !booleanValue;
        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool booleanValue;
        if (value is bool b)
        {
            booleanValue = b;
        }
        else
        {
            try
            {
                booleanValue = System.Convert.ToBoolean(value, culture);
            }
            catch
            {
                booleanValue = false;
            }
        }

        return !booleanValue;
    }
}
