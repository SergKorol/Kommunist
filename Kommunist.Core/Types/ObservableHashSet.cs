using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Kommunist.Core.Types;

public class ObservableHashSet<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly HashSet<T> _set = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Count => _set.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        if (!_set.Add(item)) return;
        RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item);
        RaisePropertyChanged(nameof(Count));
    }

    void ICollection<T>.Add(T item) => Add(item);

    public bool Remove(T item)
    {
        if (!_set.Remove(item)) return false;
        RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        RaisePropertyChanged(nameof(Count));
        return true;
    }

    public void Clear()
    {
        if (_set.Count == 0) return;
        _set.Clear();
        RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        RaisePropertyChanged(nameof(Count));
    }

    public bool Contains(T item) => _set.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void RaiseCollectionChanged(NotifyCollectionChangedAction action, object? item = null)
    {
        var args = item is null
            ? new NotifyCollectionChangedEventArgs(action)
            : new NotifyCollectionChangedEventArgs(action, item);

        CollectionChanged?.Invoke(this, args);
    }

    private void RaisePropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}