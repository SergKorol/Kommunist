using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public sealed class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create(string? host) => new SmtpClientWrapper(host);
}
