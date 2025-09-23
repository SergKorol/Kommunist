using System.Net;
using System.Net.Mail;
using FluentAssertions;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Kommunist.Tests.Services;

public class EmailServiceTests
{
    private static IConfiguration BuildConfig(string host = "smtp.example.com", string user = "user", string pass = "pass", string sender = "sender@example.com")
    {
        var dict = new Dictionary<string, string?>
        {
            ["SmtpProvider:Host"] = host,
            ["SmtpProvider:UserName"] = user,
            ["SmtpProvider:Password"] = pass,
            ["SmtpProvider:Sender"] = sender
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private sealed class FakeSmtpClient : ISmtpClient
    {
        public int Port { get; set; }
        public ICredentialsByHost? Credentials { get; set; }
        public bool EnableSsl { get; set; }

        public MailMessage? SentMessage { get; private set; }
        public bool ThrowOnSend { get; set; }

        public Task SendMailAsync(MailMessage message)
        {
            SentMessage = message;
            return ThrowOnSend ? throw new InvalidOperationException("SMTP send failed") : Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeSmtpClientFactory : ISmtpClientFactory
    {
        public string? CreatedHost { get; private set; }
        public FakeSmtpClient Client { get; } = new();

        public ISmtpClient Create(string host)
        {
            CreatedHost = host;
            return Client;
        }
    }

    [Fact]
    public async Task SendEmailAsync_ConfiguresClientAndSends_WithBasicMessage()
    {
        // Arrange
        var config = BuildConfig();
        var factory = new FakeSmtpClientFactory();
        var sut = new EmailService(config, factory);

        const string to = "recipient@example.com";
        const string subject = "Hello";
        const string body = "<b>World</b>";
        string? attachmentPath = null;
        const string email = "to-in-ctor@example.com";

        // Act
        await sut.SendEmailAsync(to, subject, body, attachmentPath, email);

        // Assert
        factory.CreatedHost.Should().Be("smtp.example.com");
        factory.Client.Port.Should().Be(587);
        factory.Client.EnableSsl.Should().BeTrue();
        factory.Client.Credentials.Should().BeOfType<NetworkCredential>();
        var nc = factory.Client.Credentials as NetworkCredential;
        nc.Should().NotBeNull();
        nc?.UserName.Should().Be("user");
        nc?.Password.Should().Be("pass");

        factory.Client.SentMessage.Should().NotBeNull();
        var msg = factory.Client.SentMessage;
        msg?.Subject.Should().Be(subject);
        msg?.Body.Should().Be(body);
        msg?.IsBodyHtml.Should().BeTrue();
        msg?.From?.Address.Should().Be("sender@example.com");
        msg?.To.Select(x => x.Address).Should().Contain([email, to]);
        msg?.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_AddsAttachment_WhenPathProvided()
    {
        // Arrange
        var config = BuildConfig();
        var factory = new FakeSmtpClientFactory();
        var sut = new EmailService(config, factory);

        const string to = "recipient@example.com";
        const string subject = "Subject";
        const string body = "Body";
        const string email = "to-in-ctor@example.com";

        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "Test attachment");

        try
        {
            // Act
            await sut.SendEmailAsync(to, subject, body, tempFile, email);

            // Assert
            factory.Client.SentMessage.Should().NotBeNull();
            var msg = factory.Client.SentMessage;
            msg?.Attachments.Should().HaveCount(1);
            msg?.Attachments[0].Name.Should().Be(Path.GetFileName(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SendEmailAsync_DoesNotAddAttachment_WhenPathIsNull()
    {
        // Arrange
        var config = BuildConfig();
        var factory = new FakeSmtpClientFactory();
        var sut = new EmailService(config, factory);

        // Act
        await sut.SendEmailAsync("to@example.com", "s", "b", null, "t2@example.com");

        // Assert
        factory.Client.SentMessage.Should().NotBeNull();
        factory.Client.SentMessage?.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_DoesNotAddAttachment_WhenPathIsEmpty()
    {
        // Arrange
        var config = BuildConfig();
        var factory = new FakeSmtpClientFactory();
        var sut = new EmailService(config, factory);

        // Act
        await sut.SendEmailAsync("to@example.com", "s", "b", string.Empty, "t2@example.com");

        // Assert
        factory.Client.SentMessage.Should().NotBeNull();
        factory.Client.SentMessage?.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task SendEmailAsync_RethrowsException_WhenSmtpFails()
    {
        // Arrange
        var config = BuildConfig();
        var factory = new FakeSmtpClientFactory
        {
            Client =
            {
                ThrowOnSend = true
            }
        };
        var sut = new EmailService(config, factory);

        // Act
        var act = () => sut.SendEmailAsync("to@example.com", "s", "b", null, "t2@example.com");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SMTP send failed");
    }
}