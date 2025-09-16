namespace Kommunist.Core.Services.Interfaces;

public interface ISmtpClientFactory
{
    ISmtpClient Create(string host);
}
