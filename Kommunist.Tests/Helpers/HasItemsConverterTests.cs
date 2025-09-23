using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class HasItemsConverterTests
{
    private readonly HasItemsConverter _sut = new();

    public static TheoryData<string, bool> ConvertTestCases => new()
    {
        { "ListObj_1", true },
        { "ObjectArray_Null", true },

        { "ListObj_Empty", false },
        { "ArrayObj_Empty", false },

        { "Null", false },

        { "Int_5", false },
        { "Double_5", false },

        { "ListInt_1", false },
        { "ArrayInt_12", false },
        { "String_abc", false }
    };

    private static object? CreateValue(string key) => key switch
    {
        "ListObj_1" => new List<object> { 1 },
        "ObjectArray_Null" => new object?[] { null },

        "ListObj_Empty" => new List<object>(),
        "ArrayObj_Empty" => Array.Empty<object>(),

        "Null" => null,

        "Int_5" => 5,
        "Double_5" => 5.0,

        "ListInt_1" => new List<int> { 1 },
        "ArrayInt_12" => new[] { 1, 2 },
        "String_abc" => "abc",
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown test case key")
    };

    [Theory]
    [MemberData(nameof(ConvertTestCases))]
    public void Convert_ReturnsExpected(string key, bool expected)
    {
        var value = CreateValue(key);
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
