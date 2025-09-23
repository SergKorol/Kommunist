using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class MauiPreferences : IAppPreferences
{
    private readonly Func<string, string, string> _get;
    private readonly Action<string, string> _set;
    private readonly Action<string> _remove;

    public MauiPreferences()
    {
        _get = Preferences.Get;
        _set = Preferences.Set;
        _remove = Preferences.Remove;
    }

    public MauiPreferences(
        Func<string, string, string> get,
        Action<string, string> set,
        Action<string> remove)
    {
        _get = get ?? throw new ArgumentNullException(nameof(get));
        _set = set ?? throw new ArgumentNullException(nameof(set));
        _remove = remove ?? throw new ArgumentNullException(nameof(remove));
    }

    public string? Get(string key, string? defaultValue) => _get(key, defaultValue ?? string.Empty);

    public void Set(string key, string value) => _set(key, value);

    public void Remove(string key) => _remove(key);
}
