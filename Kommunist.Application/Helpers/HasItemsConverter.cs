using System.Globalization;

namespace Kommunist.Application.Helpers;

public class HasItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<object> collection)
        {
            return collection.Any();
        }
            
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}