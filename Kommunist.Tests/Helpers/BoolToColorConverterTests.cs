using System.Globalization;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class BoolToColorConverterTests
{
    private static void EnsureApp()
    {
        if (Microsoft.Maui.Controls.Application.Current is null)
        {
            _ = new Microsoft.Maui.Controls.Application();
        }
    }

    private static void AssertColorEquals(Color actual, Color expected, float tolerance = 0.0001f)
    {
        Assert.InRange(actual.Red, expected.Red - tolerance, expected.Red + tolerance);
        Assert.InRange(actual.Green, expected.Green - tolerance, expected.Green + tolerance);
        Assert.InRange(actual.Blue, expected.Blue - tolerance, expected.Blue + tolerance);
        Assert.InRange(actual.Alpha, expected.Alpha - tolerance, expected.Alpha + tolerance);
    }

    [Fact]
    public void Convert_NonBoolValue_ReturnsWhite()
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
        var converter = new BoolToColorConverter();

        // Act
        var result = (Color)converter.Convert("not a bool", typeof(Color), null, CultureInfo.InvariantCulture);

        // Assert
        AssertColorEquals(result, Colors.White);
    }

    [Fact]
    public void Convert_BoolTrue_ParameterNotString_ReturnsLightBlue()
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
        var converter = new BoolToColorConverter();

        // Act
        var result = (Color)converter.Convert(true, typeof(Color), 123, CultureInfo.InvariantCulture);

        // Assert
        AssertColorEquals(result, Colors.LightBlue);
    }

    [Fact]
    public void Convert_BoolFalse_ParameterNotString_ReturnsWhite()
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
        var converter = new BoolToColorConverter();

        // Act
        var result = (Color)converter.Convert(false, typeof(Color), 123, CultureInfo.InvariantCulture);

        // Assert
        AssertColorEquals(result, Colors.White);
    }

    [Theory]
    [InlineData(true, "SingleColorNoPipe")]
    [InlineData(false, "A|B|C")]
    public void Convert_BoolWithInvalidParameterFormat_ReturnsDefaultFallback(bool value, string parameter)
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
        var converter = new BoolToColorConverter();

        // Act
        var result = (Color)converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        var expected = value ? Colors.LightBlue : Colors.White;
        AssertColorEquals(result, expected);
    }

    [Theory]
    [InlineData(true, "#FF0000|#00FF00", "#FF0000")]
    [InlineData(false, "#FF0000|#00FF00", "#00FF00")] 
    [InlineData(true, "#112233|#445566", "#112233")]
    [InlineData(false, "#112233|#445566", "#445566")]
    public void Convert_ValidParameters_LightTheme_UsesProvidedColors(bool value, string parameter, string expectedHex)
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
        var converter = new BoolToColorConverter();
        Assert.True(Color.TryParse(expectedHex, out var expectedColor));

        // Act
        var result = (Color)converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        AssertColorEquals(result, expectedColor);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Convert_InvalidColorStrings_FallsBackToDefault(bool value)
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
        var converter = new BoolToColorConverter();
        const string parameter = "#ZZZZZZ|#QQQQQQ";

        // Act
        var result = (Color)converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        var expected = value ? Colors.LightBlue : Colors.White;
        AssertColorEquals(result, expected);
    }

    [Theory]
    [InlineData(true, "#3A2D78")]
    [InlineData(false, "#1E1E1E")]
    public void Convert_DarkTheme_IgnoresParameters_UsesHardcodedDarkColors(bool value, string expectedHex)
    {
        // Arrange
        EnsureApp();
        if (Microsoft.Maui.Controls.Application.Current != null)
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Dark;
        var converter = new BoolToColorConverter();
        const string parameter = "#FF0000|#00FF00";
        Assert.True(Color.TryParse(expectedHex, out var expectedColor));

        // Act
        var result = (Color)converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        AssertColorEquals(result, expectedColor);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsNull()
    {
        // Arrange
        var converter = new BoolToColorConverter();

        // Act
        var result = converter.ConvertBack(Colors.Red, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }
}
