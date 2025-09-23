using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class HasItemsConverterTests
{
    private readonly HasItemsConverter _sut = new();

    public static IEnumerable<object?[]> ConvertTestCases()
    {
        yield return [new List<object> { 1 }, true];
        yield return [new object?[] { null }, true]; 

        yield return [new List<object>(), false];
        yield return [Array.Empty<object>(), false];

        yield return [null, false];

        yield return [5, false];
        yield return [5.0, false];

        yield return [new List<int> { 1 }, false];
        yield return [new[] { 1, 2 }, false];
        yield return ["abc", false];
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
