using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Kommunist.Core.Types;

public class ObservableHashSet<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly HashSet<T> _set = new();

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public int Count => _set.Count;
    public bool IsReadOnly => false;

    public bool Add(T item)
    {
        if (_set.Add(item))
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(nameof(Count));
            return true;
        }
        return false;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public bool Remove(T item)
    {
        if (_set.Remove(item))
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, item));
            OnPropertyChanged(nameof(Count));
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (_set.Count > 0)
        {
            _set.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
        }
    }

    public bool Contains(T item) => _set.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}