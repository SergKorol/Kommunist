using System.Globalization;

namespace Kommunist.Application.Helpers;

public class AllTrueMultiValueConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
        {
            return false;
        }

        foreach (var value in values)
        {
            if (value is bool b)
            {
                if (!b) return false;
            }
            else
            {
                try
                {
                    if (!System.Convert.ToBoolean(value, culture))
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException("AllTrueMultiValueConverter does not support ConvertBack.");
}
