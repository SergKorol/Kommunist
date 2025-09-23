using FluentAssertions;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public class DateTimeHelperTests
{
    [Theory]
    [InlineData(0L)]
    [InlineData(1_600_000_000L)]      // 2020-09-13T12:26:40Z
    [InlineData(-1_000_000_000L)]     // 1938-04-24T22:13:20Z
    [InlineData(2_147_483_647L)]      // Int32.MaxValue seconds
    public void ToLocalDateTime_ReturnsExpected_LocalClockTime(long seconds)
    {
        // Arrange
        var dto = DateTimeOffset.FromUnixTimeSeconds(seconds);
        var expected = TimeZoneInfo.ConvertTime(dto, TimeZoneInfo.Local).DateTime;

        // Act
        var result = seconds.ToLocalDateTime();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToLocalDateTime_ReturnsDateTimeWithUnspecifiedKind()
    {
        // Arrange
        const long seconds = 0L;

        // Act
        var result = seconds.ToLocalDateTime();

        // Assert
        result.Kind.Should().Be(DateTimeKind.Unspecified);
    }

    [Fact]
    public void ToLocalDateTime_SupportsMinimumUnixSeconds()
    {
        // Arrange
        var minSeconds = DateTimeOffset.MinValue.ToUnixTimeSeconds();
        var dto = DateTimeOffset.FromUnixTimeSeconds(minSeconds);
        var expected = TimeZoneInfo.ConvertTime(dto, TimeZoneInfo.Local).DateTime;

        // Act
        var result = minSeconds.ToLocalDateTime();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToLocalDateTime_SupportsMaximumUnixSeconds()
    {
        // Arrange
        var maxSeconds = DateTimeOffset.MaxValue.ToUnixTimeSeconds();
        var dto = DateTimeOffset.FromUnixTimeSeconds(maxSeconds);
        var expected = TimeZoneInfo.ConvertTime(dto, TimeZoneInfo.Local).DateTime;

        // Act
        var result = maxSeconds.ToLocalDateTime();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToLocalDateTime_BelowMinimumUnixSeconds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var belowMin = DateTimeOffset.MinValue.ToUnixTimeSeconds() - 1;

        // Act
        Action act = () => belowMin.ToLocalDateTime();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToLocalDateTime_AboveMaximumUnixSeconds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var aboveMax = DateTimeOffset.MaxValue.ToUnixTimeSeconds() + 1;

        // Act
        Action act = () => aboveMax.ToLocalDateTime();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
