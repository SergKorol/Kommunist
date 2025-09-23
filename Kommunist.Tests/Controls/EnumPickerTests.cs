using FluentAssertions;
using Kommunist.Application.Controls;

namespace Kommunist.Tests.Controls;

public class EnumPickerTests
{
    private enum SampleEnumA;

    private enum SampleEnumB;

    [Fact]
    public void EnumType_SetToValidEnum_PopulatesItemsSourceWithEnumValues()
    {
        // Arrange
        var picker = new EnumPicker
        {
            // Act
            EnumType = typeof(SampleEnumA)
        };

        // Assert
        var items = picker.ItemsSource.Cast<object>().ToArray();
        items.Should().BeEquivalentTo(Enum.GetValues<SampleEnumA>().Cast<object>(),
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void EnumType_SetToInvalidType_ThrowsArgumentException_AndLeavesItemsSourceClearedWhenPreviouslySet()
    {
        // Arrange
        var picker = new EnumPicker
        {
            EnumType = typeof(SampleEnumA)
        };
        picker.ItemsSource.Should().NotBeNull();

        // Act
        Action act = () => picker.EnumType = typeof(string);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("EnumPicker: EnumType property must be enumeration type");
        picker.ItemsSource.Should().BeNull("when switching from a valid enum, ItemsSource is cleared before validation failure");
    }

    [Fact]
    public void EnumType_SetToNullInitially_KeepsItemsSourceNull()
    {
        // Arrange
        var picker = new EnumPicker
        {
            // Act
            EnumType = null
        };

        // Assert
        picker.ItemsSource.Should().BeNull();
    }

    [Fact]
    public void EnumType_ChangeFromEnumToNull_ClearsItemsSource()
    {
        // Arrange
        var picker = new EnumPicker
        {
            EnumType = typeof(SampleEnumA)
        };
        picker.ItemsSource.Should().NotBeNull();

        // Act
        picker.EnumType = null;

        // Assert
        picker.ItemsSource.Should().BeNull();
    }

    [Fact]
    public void EnumType_ChangeFromOneEnumToAnother_UpdatesItemsSource()
    {
        // Arrange
        var picker = new EnumPicker
        {
            EnumType = typeof(SampleEnumA)
        };

        // Act
        picker.EnumType = typeof(SampleEnumB);

        // Assert
        var items = picker.ItemsSource.Cast<object>().ToArray();
        items.Should().BeEquivalentTo(Enum.GetValues<SampleEnumB>().Cast<object>(),
            options => options.WithStrictOrdering());
    }
}