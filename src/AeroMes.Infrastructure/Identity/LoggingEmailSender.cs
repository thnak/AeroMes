using AeroMes.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Identity;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[EMAIL] To={To} | Subject={Subject}\n{Body}",
            to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
