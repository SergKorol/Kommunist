using System.Collections.Specialized;
using System.ComponentModel;
using FluentAssertions;
using Kommunist.Core.Types;

namespace Kommunist.Tests.Types;

public class ObservableHashSetTests
{
    private sealed class EventSink<T> : IDisposable
    {
        private readonly ObservableHashSet<T> _set;

        public List<NotifyCollectionChangedEventArgs> CollectionChangedEvents { get; } = [];
        public List<PropertyChangedEventArgs> PropertyChangedEvents { get; } = [];
        public List<string> CallOrder { get; } = [];

        public EventSink(ObservableHashSet<T> set)
        {
            _set = set;
            _set.CollectionChanged += OnCollectionChanged;
            _set.PropertyChanged += OnPropertyChanged;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedEvents.Add(e);
            CallOrder.Add(nameof(_set.CollectionChanged));
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEvents.Add(e);
            CallOrder.Add(nameof(_set.PropertyChanged));
        }

        public void Dispose()
        {
            _set.CollectionChanged -= OnCollectionChanged;
            _set.PropertyChanged -= OnPropertyChanged;
        }
    }

    [Fact]
    public void Constructor_ShouldStartEmptyAndWritable()
    {
        var set = new ObservableHashSet<int>();

        set.Count.Should().Be(0);
        set.IsReadOnly.Should().BeFalse();

        set.Add(1);
        set.Remove(1);
    }

    [Fact]
    public void Add_WhenNewItem_ShouldAddAndRaiseEvents()
    {
        var set = new ObservableHashSet<int>();
        using var sink = new EventSink<int>(set);

        set.Add(42);

        set.Count.Should().Be(1);
        set.Contains(42).Should().BeTrue();

        sink.CollectionChangedEvents.Should().HaveCount(1);
        var cc = sink.CollectionChangedEvents[0];
        cc.Action.Should().Be(NotifyCollectionChangedAction.Add);
        cc.NewItems.Should().NotBeNull();
        cc.NewItems?.Count.Should().Be(1);
        cc.NewItems?[0].Should().Be(42);

        sink.PropertyChangedEvents.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(set.Count));

        sink.CallOrder.Should().ContainInOrder(nameof(set.CollectionChanged), nameof(set.PropertyChanged));
    }

    [Fact]
    public void Add_WhenDuplicateItem_ShouldNotChangeOrRaiseEvents()
    {
        var set = new ObservableHashSet<int> { 7 };
        using var sink = new EventSink<int>(set);

        set.Add(7);

        set.Count.Should().Be(1);
        sink.CollectionChangedEvents.Should().BeEmpty();
        sink.PropertyChangedEvents.Should().BeEmpty();
        sink.CallOrder.Should().BeEmpty();
    }

    [Fact]
    public void Remove_WhenExistingItem_ShouldRemoveAndRaiseEvents()
    {
        var set = new ObservableHashSet<int> { 11 };
        using var sink = new EventSink<int>(set);

        var result = set.Remove(11);

        result.Should().BeTrue();
        set.Count.Should().Be(0);
        set.Contains(11).Should().BeFalse();

        sink.CollectionChangedEvents.Should().HaveCount(1);
        var cc = sink.CollectionChangedEvents[0];
        cc.Action.Should().Be(NotifyCollectionChangedAction.Remove);
        cc.OldItems.Should().NotBeNull();
        cc.OldItems?[0].Should().Be(11);
        cc.NewItems.Should().BeNull();

        sink.PropertyChangedEvents.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(set.Count));

        sink.CallOrder.Should().ContainInOrder(nameof(set.CollectionChanged), nameof(set.PropertyChanged));
    }

    [Fact]
    public void Remove_WhenMissingItem_ShouldReturnFalseAndNotRaiseEvents()
    {
        var set = new ObservableHashSet<int> { 1 };
        using var sink = new EventSink<int>(set);

        var result = set.Remove(999);

        result.Should().BeFalse();
        set.Count.Should().Be(1);
        sink.CollectionChangedEvents.Should().BeEmpty();
        sink.PropertyChangedEvents.Should().BeEmpty();
        sink.CallOrder.Should().BeEmpty();
    }

    [Fact]
    public void Clear_WhenNotEmpty_ShouldClearAndRaiseResetAndPropertyChanged()
    {
        var set = new ObservableHashSet<int>
        {
            1,
            2
        };
        using var sink = new EventSink<int>(set);

        set.Clear();

        set.Count.Should().Be(0);

        sink.CollectionChangedEvents.Should().HaveCount(1);
        var cc = sink.CollectionChangedEvents[0];
        cc.Action.Should().Be(NotifyCollectionChangedAction.Reset);

        sink.PropertyChangedEvents.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(set.Count));

        sink.CallOrder.Should().ContainInOrder(nameof(set.CollectionChanged), nameof(set.PropertyChanged));
    }

    [Fact]
    public void Clear_WhenAlreadyEmpty_ShouldNotRaiseEvents()
    {
        var set = new ObservableHashSet<int>();
        using var sink = new EventSink<int>(set);

        set.Clear();

        sink.CollectionChangedEvents.Should().BeEmpty();
        sink.PropertyChangedEvents.Should().BeEmpty();
        sink.CallOrder.Should().BeEmpty();
    }

    [Fact]
    public void Contains_ShouldReflectMembership()
    {
        var set = new ObservableHashSet<int>();

        set.Contains(5).Should().BeFalse();
        set.Add(5);
        set.Contains(5).Should().BeTrue();
        set.Remove(5);
        set.Contains(5).Should().BeFalse();
    }

    [Fact]
    public void CopyTo_ShouldCopyElementsAtProvidedIndex()
    {
        var set = new ObservableHashSet<int>
        {
            11,
            22
        };

        var arr = new[] { -1, -1, -1, -1, -1 };

        set.CopyTo(arr, 1);

        var copied = new[] { arr[1], arr[2] };
        copied.Should().BeEquivalentTo([11, 22]);
        arr[0].Should().Be(-1);
        arr[3].Should().Be(-1);
        arr[4].Should().Be(-1);
    }

    [Fact]
    public void Enumerator_ShouldEnumerateAllElements()
    {
        var set = new ObservableHashSet<int>
        {
            3,
            5,
            7
        };

        var enumerated = set.ToList();

        enumerated.Should().BeEquivalentTo([3, 5, 7]);
    }

    [Fact]
    public void ExplicitInterfaceAdd_ShouldBehaveLikeAdd()
    {
        var set = new ObservableHashSet<int>();
        using var sink = new EventSink<int>(set);
        ICollection<int> collection = set;

        collection.Add(123);

        set.Contains(123).Should().BeTrue();
        set.Count.Should().Be(1);

        sink.CollectionChangedEvents.Should().HaveCount(1);
        sink.CollectionChangedEvents[0].Action.Should().Be(NotifyCollectionChangedAction.Add);

        sink.PropertyChangedEvents.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(set.Count));

        sink.CallOrder.Should().ContainInOrder(nameof(set.CollectionChanged), nameof(set.PropertyChanged));
    }
}
