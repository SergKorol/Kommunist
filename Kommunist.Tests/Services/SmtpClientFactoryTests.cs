using System;
using FluentAssertions;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Xunit;

namespace Kommunist.Tests.Services;

public class SmtpClientFactoryTests
{
    [Fact]
    public void Create_Returns_ISmtpClient_Instance()
    {
        // Arrange
        var factory = new SmtpClientFactory();

        // Act
        using var client = factory.Create("smtp.example.com");

        // Assert
        client.Should().NotBeNull().And.BeAssignableTo<ISmtpClient>();
    }

    [Fact]
    public void Create_Returns_New_Instance_Each_Time()
    {
        // Arrange
        var factory = new SmtpClientFactory();

        // Act
        using var client1 = factory.Create("smtp.example.com");
        using var client2 = factory.Create("smtp.example.com");

        // Assert
        client1.Should().NotBeSameAs(client2);
    }

    [Fact]
    public void Created_Client_Is_Disposable_And_Can_Be_Disposed_Multiple_Times()
    {
        // Arrange
        var factory = new SmtpClientFactory();
        var client = factory.Create("smtp.example.com");

        // Act
        Action act = () =>
        {
            client.Dispose();
            client.Dispose(); // disposing twice should be safe
        };

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_Allows_Null_Or_Empty_Host(string? host)
    {
        // Arrange
        var factory = new SmtpClientFactory();

        // Act
        using var client = factory.Create(host!);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Create_Returns_SmtpClientWrapper_Type()
    {
        // Arrange
        var factory = new SmtpClientFactory();

        // Act
        using var client = factory.Create("smtp.example.com");

        // Assert
        client.Should().BeOfType<SmtpClientWrapper>();
    }
}
