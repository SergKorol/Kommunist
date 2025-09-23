using System.Net;
using System.Net.Mail;

namespace Kommunist.Core.Services.Interfaces;

public interface ISmtpClient : IDisposable
{
    int Port { get; set; }
    ICredentialsByHost? Credentials { get; set; }
    bool EnableSsl { get; set; }
    Task SendMailAsync(MailMessage message);
}
