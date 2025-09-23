using System.Net;
using FluentAssertions;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Tests.Services;

public class SmtpClientWrapperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("smtp.example.com")]
    public void Ctor_Allows_Any_Host_Value(string? host)
    {
        // Act
        var act = () =>
        {
            using var _ = new SmtpClientWrapper(host);
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Instance_Implements_ISmtpClient()
    {
        // Arrange & Act
        using var sut = new SmtpClientWrapper("smtp.example.com");

        // Assert
        sut.Should().BeAssignableTo<ISmtpClient>();
    }

    [Fact]
    public void Defaults_Are_Expected()
    {
        // Arrange
        using var sut = new SmtpClientWrapper("smtp.example.com");

        // Assert
        sut.Port.Should().Be(25);
        sut.EnableSsl.Should().BeFalse();
        sut.Credentials.Should().BeNull();
    }

    [Theory]
    [InlineData(25)]
    [InlineData(465)]
    [InlineData(587)]
    [InlineData(2525)]
    [InlineData(65535)]
    [InlineData(70000)]
    public void Port_Set_Get_Roundtrips(int port)
    {
        // Arrange
        using var sut = new SmtpClientWrapper("smtp.example.com");

        // Act
        sut.Port = port;

        // Assert
        sut.Port.Should().Be(port);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Port_Set_Invalid_Throws(int port)
    {
        // Arrange & Act
        var act = () =>
        {
            using var local = new SmtpClientWrapper("smtp.example.com");
            local.Port = port;
        };

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableSsl_Set_Get_Roundtrips(bool value)
    {
        // Arrange
        using var sut = new SmtpClientWrapper("smtp.example.com");

        // Act
        sut.EnableSsl = value;

        // Assert
        sut.EnableSsl.Should().Be(value);
    }

    [Fact]
    public void Credentials_Set_Get_Roundtrips()
    {
        // Arrange
        using var sut = new SmtpClientWrapper("smtp.example.com");
        var creds = new NetworkCredential("user", "pass");

        // Act
        sut.Credentials = creds;

        // Assert
        sut.Credentials.Should().BeSameAs(creds);
    }

    [Fact]
    public void Credentials_Can_Be_Set_To_Null()
    {
        // Arrange
        using var sut = new SmtpClientWrapper("smtp.example.com");

        // Act
        sut.Credentials = null;

        // Assert
        sut.Credentials.Should().BeNull();
    }

    [Fact]
    public async Task SendMailAsync_With_Null_Throws()
    {
        // Act
        var act = () => new SmtpClientWrapper("smtp.example.com").SendMailAsync(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Dispose_Is_Idempotent()
    {
        // Act
        var act = () =>
        {
            var local = new SmtpClientWrapper("smtp.example.com");
            local.Dispose();
            local.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }
}
