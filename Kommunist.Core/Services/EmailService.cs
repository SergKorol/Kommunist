using System.Net;
using System.Net.Mail;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body, string attachmentPath)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp-relay.brevo.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("88fdc6001@smtp-brevo.com", "LCO1jMtG7Kv9FyID"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("korols83@gmail.com"),
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