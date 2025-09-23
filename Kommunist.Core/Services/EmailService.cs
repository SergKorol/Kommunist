using System.Net;
using System.Net.Mail;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Task = System.Threading.Tasks.Task;

namespace Kommunist.Core.Services;

public class EmailService(IConfiguration config, ISmtpClientFactory? smtpClientFactory = null) : IEmailService
{
    private readonly ISmtpClientFactory _smtpClientFactory = smtpClientFactory ?? new SmtpClientFactory();

    public async Task SendEmailAsync(string to, string subject, string body, string? attachmentPath, string email)
    {
        try
        {
            using var smtpClient = _smtpClientFactory.Create(config["SmtpProvider:Host"] ?? string.Empty);
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential(config["SmtpProvider:UserName"], config["SmtpProvider:Password"]);
            smtpClient.EnableSsl = true;

            var mailMessage = new MailMessage(config["SmtpProvider:Sender"] ?? "", email)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(to));

            if (attachmentPath is not (null or ""))
            {
                mailMessage.Attachments.Add(new Attachment(attachmentPath));
            }

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}