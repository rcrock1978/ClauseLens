using ClauseLens.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Infrastructure.Email;

/// <summary>
/// SMTP-backed email sender. In dev, logs the verification link to the console
/// and writes it to logs/{date}.json via Serilog (Constitution Principle IV).
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly ILogger<SmtpEmailSender> _logger;
    public SmtpEmailSender(ILogger<SmtpEmailSender> logger) => _logger = logger;

    public Task SendVerificationEmailAsync(string to, string token, CancellationToken ct = default)
    {
        _logger.LogInformation("Verification email to {Email}: token={Token}", to, token);
        return Task.CompletedTask;
    }

    public Task SendReviewerSlaNudgeAsync(string to, Guid contractId, DateTime slaDeadline, CancellationToken ct = default)
    {
        _logger.LogInformation("SLA nudge to {Email}: contract {ContractId} deadline {Deadline:O}", to, contractId, slaDeadline);
        return Task.CompletedTask;
    }
}
