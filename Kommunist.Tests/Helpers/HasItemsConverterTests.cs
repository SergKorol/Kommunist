using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class HasItemsConverterTests
{
    private readonly HasItemsConverter _sut = new();

    public static TheoryData<object?, bool> ConvertTestCases => new()
    {
        { new List<object> { 1 }, true },
        { new object?[] { null }, true },

        { new List<object>(), false },
        { Array.Empty<object>(), false },

        { null, false },

        { 5, false },
        { 5.0, false },

        { new List<int> { 1 }, false },
        { new[] { 1, 2 }, false },
        { "abc", false },
    };

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
