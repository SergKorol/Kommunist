using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class HasItemsConverterTests
{
    private readonly HasItemsConverter _sut = new();

    public static IEnumerable<object[]> ConvertTestCases()
    {
        // True when IEnumerable<object> has at least one element
        yield return new object[] { new List<object> { 1 }, true };
        yield return new object[] { new object?[] { null }, true }; // null item still counts as an item

        // False when IEnumerable<object> is empty
        yield return new object[] { new List<object>(), false };
        yield return new object[] { Array.Empty<object>(), false };

        // False for null value
        yield return new object[] { null!, false };

        // False for non-enumerable inputs
        yield return new object[] { 5, false };
        yield return new object[] { 5.0, false };

        // False for enumerables that are NOT IEnumerable<object>
        // (e.g., IEnumerable<int> or string which is IEnumerable<char>)
        yield return new object[] { new List<int> { 1 }, false };
        yield return new object[] { new int[] { 1, 2 }, false };
        yield return new object[] { "abc", false };
    }

    [Theory]
    [MemberData(nameof(ConvertTestCases))]
    public void Convert_ReturnsExpected(object? value, bool expected)
    {
        var result = _sut.Convert(value, typeof(bool), parameter: null, culture: CultureInfo.InvariantCulture);

        result.Should().BeOfType<bool>();
        ((bool)result).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData("anything")]
    [InlineData(42)]
    [InlineData((object)new object[] { 1, 2, 3 })]
    public void ConvertBack_AlwaysReturnsNull(object? value)
    {
        var back = _sut.ConvertBack(value, typeof(object), parameter: null, culture: CultureInfo.InvariantCulture);

        back.Should().BeNull();
    }
}
