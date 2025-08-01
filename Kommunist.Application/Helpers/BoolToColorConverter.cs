using System.Globalization;

namespace Kommunist.Application.Helpers;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (parameter is string colorParams)
            {
                var colors = colorParams.Split('|');
                if (colors.Length == 2)
                {
                    string colorString = string.Empty;
                    if (Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark)
                    {
                        Task.Delay(100);
                        colorString = boolValue ? "#3A2D78" : "#1E1E1E";   
                    }
                    else
                    {
                        colorString = boolValue ? colors[0] : colors[1];
                    }
                    
                    if (Color.TryParse(colorString, out var color))
                    {
                        return color;
                    }
                }
            }
                
            return boolValue ? Colors.LightBlue : Colors.White;
        }
            
        return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}