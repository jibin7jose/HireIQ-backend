using CareerConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Mock implementation: just log it nicely to the console
        // In a real production environment, we'd inject SendGridClient or SmtpClient here.
        
        _logger.LogInformation(@"
=======================================================
               EMAIL SENT (MOCK SMTP)
=======================================================
To:      {To}
Subject: {Subject}
-------------------------------------------------------
{Body}
=======================================================
", to, subject, body);

        return Task.CompletedTask;
    }
}
