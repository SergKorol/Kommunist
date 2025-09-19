using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class IsGreaterThanZeroConverterTests
{
    private readonly IsGreaterThanZeroConverter _sut = new();
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    [Fact]
    public void Convert_Null_ReturnsFalse()
    {
        var result = _sut.Convert(null, typeof(object), null, Culture);
        result.Should().Be(false);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(42, true)]
    [InlineData(-1, false)]
    [InlineData(int.MinValue, false)]
    public void Convert_IntValues_ReturnsExpected(int value, bool expected)
    {
        var result = _sut.Convert(value, typeof(object), null, Culture);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(1.0)]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("")]
    [InlineData(true)]
    [InlineData(false)]
    public void Convert_NonIntInputs_ReturnsFalse(object input)
    {
        var result = _sut.Convert(input, typeof(object), null, Culture);
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_Always_ThrowsNotImplementedException()
    {
        var act = () => _sut.ConvertBack(true, typeof(int), null, Culture);
        act.Should().Throw<NotImplementedException>();
    }
}