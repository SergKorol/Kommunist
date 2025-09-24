using System.Globalization;
using System.Reflection;
using System.Windows.Input;

namespace Kommunist.Application.Helpers;

public class ViewModelCommandConverter : IValueConverter
{
    public string CommandPath { get; set; } = string.Empty;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Element element)
        {
            return null;
        }

        var vm = element.BindingContext;
        if (vm is null || string.IsNullOrWhiteSpace(CommandPath))
        {
            return null;
        }

        var prop = vm.GetType().GetRuntimeProperty(CommandPath);
        var cmd = prop?.GetValue(vm) as ICommand;
        return cmd;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
