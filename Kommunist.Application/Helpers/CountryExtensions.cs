using System.Collections.ObjectModel;

namespace Kommunist.Application.Helpers;

public static class CountryExtensions
{
    private static readonly Dictionary<string, string> LanguageToFlag = new()
    {
        { "En", "English" },
        { "Esp", "Spanish" },
        { "Ua", "Ukrainian" },
        { "Ru", "Russian" },
        { "By", "Belarusian" },
        { "Slk", "Slovak" }
    };
    
    public static List<string> WithoutFlags(this List<string> countries)
    {
        return
        [
            ..countries.Select(c =>
            {
                var parts = c.Split(' ', 2);
                return parts.Length == 2 ? parts[1] : c;
            })
        ];
    }
    
    public static string? FindWithFlag(this ObservableCollection<string> countries, string countryName)
    {
        return countries.FirstOrDefault(c =>
        {
            var parts = c.Split(' ', 2);
            var name = parts.Length == 2 ? parts[1] : c;
            return string.Equals(name, countryName, StringComparison.OrdinalIgnoreCase);
        });
    }
    
    public static List<string> ReplaceCodesWithFlags(this List<string> codes)
    {
        return codes
            .Select(code => LanguageToFlag.GetValueOrDefault(code, code))
            .ToList();
    }
}