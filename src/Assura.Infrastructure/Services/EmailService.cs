using System.Net;
using System.Net.Mail;
using Assura.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Assura.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("Smtp");
        var host = smtpSettings["Host"];
        var port = int.Parse(smtpSettings["Port"] ?? "587");
        var username = smtpSettings["Username"];
        var password = smtpSettings["Password"];
        var fromEmail = smtpSettings["FromEmail"] ?? "no-reply@assura.com";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}
