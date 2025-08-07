using System.Net;
using System.Net.Mail;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Task = System.Threading.Tasks.Task;

namespace Kommunist.Core.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body, string attachmentPath, string email)
    {
        try
        {
            var smtpClient = new SmtpClient(config["SmtpProvider:Host"])
            {
                Port = 587,
                Credentials = new NetworkCredential(config["SmtpProvider:UserName"], config["SmtpProvider:Password"]),
                EnableSsl = true
            };
            var mailMessage = new MailMessage(config["SmtpProvider:Sender"] ?? string.Empty, email)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(to));

            if (!string.IsNullOrEmpty(attachmentPath))
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