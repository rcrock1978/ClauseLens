using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClauseLens.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace ClauseLens.Infrastructure.Auth;

/// <summary>
/// JWT issuance with HS256 + email-verification token factory.
/// Password hashing via BCrypt (per FR-007c — self-service signup).
/// Per Constitution Security Gates, the signing secret MUST come from a
/// configuration provider (env var, Key Vault, etc.). In Production, the
/// application fails fast at startup if the secret is missing or shorter
/// than 32 characters.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    public JwtTokenService(JwtSettings settings) => _settings = settings;

    public string IssueAccessToken(Guid userId, Guid tenantId, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, role),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, DateTime ExpiresAt) IssueEmailVerificationToken()
    {
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        return (token, DateTime.UtcNow.AddDays(7));
    }

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}

public sealed class JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "ClauseLens";
    public string Audience { get; init; } = "ClauseLens.Api";

    public void Validate(IHostEnvironment env)
    {
        if (env.IsProduction() && (string.IsNullOrEmpty(Secret) || Secret.Length < 32))
            throw new InvalidOperationException(
                "Jwt:Secret is missing or too short for Production. " +
                "Set the JWT__Secret environment variable or wire Key Vault.");
    }
}
