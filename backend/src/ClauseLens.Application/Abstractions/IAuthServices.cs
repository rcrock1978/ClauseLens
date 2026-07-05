namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Auth and JWT issuance. Implementation in Infrastructure/Auth.
/// </summary>
public interface ITokenService
{
    string IssueAccessToken(Guid userId, Guid tenantId, string role);
    (string Token, DateTime ExpiresAt) IssueEmailVerificationToken();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string to, string token, CancellationToken ct = default);
    Task SendReviewerSlaNudgeAsync(string to, Guid contractId, DateTime slaDeadline, CancellationToken ct = default);
}
