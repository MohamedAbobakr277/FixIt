using System.Net;
using System.Net.Mail;
using FixIt.BLL.Interfaces;
using FixIt.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FixIt.BLL.Services;

public class SmtpEmailSenderService : IEmailSenderService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSenderService> _logger;

    public SmtpEmailSenderService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSenderService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            // Re-throw or handle accordingly depending on requirements
            throw;
        }
    }
}
