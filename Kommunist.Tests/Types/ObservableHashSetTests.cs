using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using Kommunist.Core.Types;
using Xunit;

namespace Kommunist.Tests.Types;

public class ObservableHashSetTests
{
    private sealed class EventSink<T> : IDisposable
    {
        private readonly ObservableHashSet<T> _set;

        public List<NotifyCollectionChangedEventArgs> CollectionChangedEvents { get; } = new();
        public List<PropertyChangedEventArgs> PropertyChangedEvents { get; } = new();
        public List<string> CallOrder { get; } = new();

        public EventSink(ObservableHashSet<T> set)
        {
            _set = set;
            _set.CollectionChanged += OnCollectionChanged;
            _set.PropertyChanged += OnPropertyChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChangedEvents.Add(e);
            CallOrder.Add(nameof(_set.CollectionChanged));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
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
        cc.NewItems!.Count.Should().Be(1);
        cc.NewItems![0].Should().Be(42);

        sink.PropertyChangedEvents.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(set.Count));

        sink.CallOrder.Should().ContainInOrder(nameof(set.CollectionChanged), nameof(set.PropertyChanged));
    }

    [Fact]
    public void Add_WhenDuplicateItem_ShouldNotChangeOrRaiseEvents()
    {
        var set = new ObservableHashSet<int>();
        set.Add(7);
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
        var set = new ObservableHashSet<int>();
        set.Add(11);
        using var sink = new EventSink<int>(set);

        var result = set.Remove(11);

        result.Should().BeTrue();
        set.Count.Should().Be(0);
        set.Contains(11).Should().BeFalse();

        sink.CollectionChangedEvents.Should().HaveCount(1);
        var cc = sink.CollectionChangedEvents[0];
        cc.Action.Should().Be(NotifyCollectionChangedAction.Remove);
        cc.OldItems.Should().NotBeNull();
        cc.OldItems![0].Should().Be(11);
        cc.NewItems.Should().BeNull();

        sink.PropertyChangedEvents.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(set.Count));

        sink.CallOrder.Should().ContainInOrder(nameof(set.CollectionChanged), nameof(set.PropertyChanged));
    }

    [Fact]
    public void Remove_WhenMissingItem_ShouldReturnFalseAndNotRaiseEvents()
    {
        var set = new ObservableHashSet<int>();
        set.Add(1);
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
        var set = new ObservableHashSet<int>();
        set.Add(1);
        set.Add(2);
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
        var set = new ObservableHashSet<int>();
        set.Add(11);
        set.Add(22);

        var arr = new[] { -1, -1, -1, -1, -1 };

        set.CopyTo(arr, 1);

        // The order from a HashSet is not guaranteed, so just verify membership
        var copied = new[] { arr[1], arr[2] };
        copied.Should().BeEquivalentTo(new[] { 11, 22 });
        // Unaffected slots remain the same
        arr[0].Should().Be(-1);
        arr[3].Should().Be(-1);
        arr[4].Should().Be(-1);
    }

    [Fact]
    public void Enumerator_ShouldEnumerateAllElements()
    {
        var set = new ObservableHashSet<int>();
        set.Add(3);
        set.Add(5);
        set.Add(7);

        var enumerated = new List<int>();
        foreach (var x in set)
            enumerated.Add(x);

        enumerated.Should().BeEquivalentTo(new[] { 3, 5, 7 });
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
