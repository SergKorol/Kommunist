using Kommunist.Core.Types;

namespace Kommunist.Core.Helpers;

public static class ObservableHashSetExtensions
{
    public static ObservableHashSet<T> ToObservableHashSet<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var set = new ObservableHashSet<T>();
        foreach (var item in source)
        {
            set.Add(item);
        }
        return set;
    }
}

