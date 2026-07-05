using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using MediatR;

namespace ClauseLens.Application.Identity.Commands;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;
public sealed record LoginResponse(string AccessToken, Guid UserId, Guid TenantId, string Role);

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public LoginHandler(IUserRepository users, ITokenService tokens) { _users = users; _tokens = tokens; }

    public async Task<Result<LoginResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        // Search globally by email since login is the only path that knows
        // the email but not the tenant. The repo uses IgnoreQueryFilters.
        var user = await _users.FindByEmailGlobalAsync(cmd.Email, ct);
        if (user is null) return Result<LoginResponse>.Failure("Invalid credentials.", "INVALID_CREDENTIALS");
        if (user.Status != UserStatus.Active) return Result<LoginResponse>.Failure("Account not active. Verify your email first.", "ACCOUNT_INACTIVE");
        if (!_tokens.VerifyPassword(cmd.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid credentials.", "INVALID_CREDENTIALS");

        var token = _tokens.IssueAccessToken(user.Id, user.TenantId, user.Role.ToString());
        return Result<LoginResponse>.Success(new LoginResponse(token, user.Id, user.TenantId, user.Role.ToString()));
    }
}
