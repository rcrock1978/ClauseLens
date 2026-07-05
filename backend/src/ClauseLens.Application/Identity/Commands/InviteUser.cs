using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClauseLens.Application.Identity.Commands;

public sealed record InviteUserCommand(string Email, UserRole Role) : ICommand<Result<InviteUserResponse>>;
public sealed record InviteUserResponse(Guid UserId, DateTime InvitedAt);

public sealed class InviteUserValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Role).IsInEnum();
    }
}

public sealed class InviteUserHandler : IRequestHandler<InviteUserCommand, Result<InviteUserResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IEmailSender _email;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public InviteUserHandler(IUnitOfWork uow, IUserRepository users, ITokenService tokens,
                              IEmailSender email, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _users = users; _tokens = tokens; _email = email; _current = current; _audit = audit; }

    public async Task<Result<InviteUserResponse>> Handle(InviteUserCommand cmd, CancellationToken ct)
    {
        if (!_current.IsInRole(nameof(UserRole.Admin)))
            return Result<InviteUserResponse>.Failure("Only Admins may invite users.", "FORBIDDEN");
        if (_current.TenantId is null)
            return Result<InviteUserResponse>.Failure("Tenant context missing.", "NO_TENANT");

        if (await _users.EmailExistsInTenantAsync(_current.TenantId.Value, cmd.Email, ct))
            return Result<InviteUserResponse>.Failure("Email already in tenant.", "EMAIL_TAKEN");

        // First Admin is self-provisioned via SignupTenant. Admins may only
        // invite ContractOwner or Reviewer via this command.
        if (cmd.Role == UserRole.Admin)
            return Result<InviteUserResponse>.Failure("Admins cannot be invited via this command.", "ADMIN_INVITE_FORBIDDEN");

        var (token, _) = _tokens.IssueEmailVerificationToken();
        var invited = User.CreateInvited(_current.TenantId.Value, cmd.Email, _tokens.HashPassword(Guid.NewGuid().ToString("N")), cmd.Role, _current.UserId!.Value);
        invited.RecordEmailVerification(token);
        await _users.AddAsync(invited, ct);
        await _uow.SaveChangesAsync(ct);
        await _email.SendVerificationEmailAsync(cmd.Email, token, ct);
        await _audit.DispatchPendingAsync(_current, ct);

        return Result<InviteUserResponse>.Success(new InviteUserResponse(invited.Id, invited.CreatedAt));
    }
}
