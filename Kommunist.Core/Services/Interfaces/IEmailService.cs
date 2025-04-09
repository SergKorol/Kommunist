using System.Threading.Tasks;

namespace Kommunist.Core.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, string attachmentPath, string email);
}