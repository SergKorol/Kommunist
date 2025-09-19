using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Helpers;
using Xunit;

namespace Kommunist.Tests.Helpers;

public class BooleanToVisibilityConverterTests
{
    private readonly BooleanToVisibilityConverter _sut = new();
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    // Convert tests

    [Fact]
    public void Convert_Null_ReturnsFalse()
    {
        var result = _sut.Convert(null, typeof(object), null, Culture);
        result.Should().Be(false);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Convert_Bool_PassesThrough(bool input, bool expected)
    {
        var result = _sut.Convert(input, typeof(object), null, Culture);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", true)]
    [InlineData("abc", true)]
    public void Convert_String_EmptyIsFalse_OtherwiseTrue(string input, bool expected)
    {
        var result = _sut.Convert(input, typeof(object), null, Culture);
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_EmptyList_ReturnsFalse()
    {
        var result = _sut.Convert(new List<int>(), typeof(object), null, Culture);
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_NonEmptyList_ReturnsTrue()
    {
        var result = _sut.Convert(new List<int> { 1 }, typeof(object), null, Culture);
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_EmptyArray_ReturnsFalse()
    {
        var result = _sut.Convert(Array.Empty<int>(), typeof(object), null, Culture);
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_NonEmptyArray_ReturnsTrue()
    {
        var result = _sut.Convert(new[] { 1 }, typeof(object), null, Culture);
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_NonGenericEnumerable_ReturnsExpected()
    {
        var empty = new ArrayList();
        var nonEmpty = new ArrayList { 1 };

        _sut.Convert(empty, typeof(object), null, Culture).Should().Be(false);
        _sut.Convert(nonEmpty, typeof(object), null, Culture).Should().Be(true);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(-5, true)]
    [InlineData(0.0, false)]
    [InlineData(0.1, true)]
    public void Convert_IConvertibleNumeric_UsesConvertToBoolean(object input, bool expected)
    {
        var result = _sut.Convert(input, typeof(object), null, Culture);
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_IConvertibleThatThrows_ReturnsTrue()
    {
        _sut.Convert('A', typeof(object), null, Culture).Should().Be(true);
        _sut.Convert(DateTime.Now, typeof(object), null, Culture).Should().Be(true);
    }

    [Fact]
    public void Convert_NonConvertibleObject_ReturnsTrue()
    {
        var result = _sut.Convert(new object(), typeof(object), null, Culture);
        result.Should().Be(true);
    }

    // ConvertBack tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConvertBack_BoolTarget_BoolInput_PassesThrough(bool value)
    {
        var result = _sut.ConvertBack(value, typeof(bool), null, Culture);
        result.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConvertBack_NullableBoolTarget_BoolInput_PassesThrough(bool value)
    {
        var result = _sut.ConvertBack(value, typeof(bool?), null, Culture);
        result.Should().Be(value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("FaLsE", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("yes", false)]
    [InlineData("1", false)]
    public void ConvertBack_BoolTarget_StringInput_ParsesOrFalse(string input, bool expected)
    {
        var result = _sut.ConvertBack(input, typeof(bool), null, Culture);
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertBack_BoolTarget_NullInput_ReturnsFalse()
    {
        var result = _sut.ConvertBack(null, typeof(bool), null, Culture);
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_BoolTarget_OtherType_ReturnsFalse()
    {
        var result = _sut.ConvertBack(123, typeof(bool), null, Culture);
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_NotBoolTarget_ThrowsNotSupportedException()
    {
        Action act = () => _sut.ConvertBack("true", typeof(string), null, Culture);
        act.Should().Throw<NotSupportedException>()
           .WithMessage("*ConvertBack is not supported*");
    }
}
