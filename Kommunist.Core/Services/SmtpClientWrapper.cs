using System.Net;
using System.Net.Mail;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public sealed class SmtpClientWrapper(string host) : ISmtpClient
{
    private readonly SmtpClient _inner = new(host);

    public int Port
    {
        get => _inner.Port;
        set => _inner.Port = value;
    }

    public ICredentialsByHost? Credentials
    {
        get => _inner.Credentials;
        set => _inner.Credentials = value;
    }

    public bool EnableSsl
    {
        get => _inner.EnableSsl;
        set => _inner.EnableSsl = value;
    }

    public Task SendMailAsync(MailMessage message) => _inner.SendMailAsync(message);

    public void Dispose() => _inner.Dispose();
}
