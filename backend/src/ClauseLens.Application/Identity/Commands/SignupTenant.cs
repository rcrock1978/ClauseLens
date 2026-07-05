using ClauseLens.Application.Abstractions;
using ClauseLens.Application.Behaviors;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using FluentValidation;
using MediatR;

namespace ClauseLens.Application.Identity.Commands;

public sealed record SignupTenantCommand(string TenantName, string AdminEmail, string Password) : ICommand<Result<SignupTenantResponse>>;

public sealed record SignupTenantResponse(Guid TenantId, Guid AdminUserId, DateTime VerificationEmailSentAt);

public sealed class SignupTenantValidator : AbstractValidator<SignupTenantCommand>
{
    public SignupTenantValidator()
    {
        RuleFor(c => c.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.AdminEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(12).MaximumLength(128);
    }
}

/// <summary>
/// Per FR-007c: self-service signup creates the tenant and the first Admin in
/// PendingInvite state with an email verification token. Admin becomes Active
/// only after the token is confirmed.
/// </summary>
public sealed class SignupTenantHandler : IRequestHandler<SignupTenantCommand, Result<SignupTenantResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokens;
    private readonly IEmailSender _email;
    private readonly IAuditEventDispatcher _audit;
    private readonly ITenantRepository _tenants;
    private readonly IUserRepository _users;

    public SignupTenantHandler(IUnitOfWork uow, ITokenService tokens, IEmailSender email,
                                IAuditEventDispatcher audit, ITenantRepository tenants, IUserRepository users)
    { _uow = uow; _tokens = tokens; _email = email; _audit = audit; _tenants = tenants; _users = users; }

    public async Task<Result<SignupTenantResponse>> Handle(SignupTenantCommand cmd, CancellationToken ct)
    {
        if (await _users.EmailExistsGloballyAsync(cmd.AdminEmail, ct))
            return Result<SignupTenantResponse>.Failure("Email already in use.", "EMAIL_TAKEN");

        var tenant = Tenant.Create(cmd.TenantName);
        await _tenants.AddAsync(tenant, ct);

        var (token, expires) = _tokens.IssueEmailVerificationToken();
        var admin = User.CreateFirstAdmin(tenant.Id, cmd.AdminEmail, _tokens.HashPassword(cmd.Password));
        admin.RecordEmailVerification(token); // mark verification token on user
        await _users.AddAsync(admin, ct);

        tenant.RaiseDomainEvent(new TenantSignedUpDomainEvent(tenant.Id, admin.Id));

        await _uow.SaveChangesAsync(ct);
        await _email.SendVerificationEmailAsync(cmd.AdminEmail, token, ct);
        await _audit.DispatchPendingAsync(new SystemCurrentUser { UserId = admin.Id, TenantId = tenant.Id, Role = nameof(UserRole.Admin) }, ct);

        return Result<SignupTenantResponse>.Success(new SignupTenantResponse(tenant.Id, admin.Id, expires));
    }
}
