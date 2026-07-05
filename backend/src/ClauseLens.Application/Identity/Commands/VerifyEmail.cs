using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClauseLens.Application.Identity.Commands;

public sealed record VerifyEmailCommand(string Token) : ICommand<Result<VerifyEmailResponse>>;
public sealed record VerifyEmailResponse(Guid UserId, Guid TenantId, string Role);

public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator() => RuleFor(c => c.Token).NotEmpty();
}

public sealed class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Result<VerifyEmailResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public VerifyEmailHandler(IUnitOfWork uow, IUserRepository users, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _users = users; _current = current; _audit = audit; }

    public async Task<Result<VerifyEmailResponse>> Handle(VerifyEmailCommand cmd, CancellationToken ct)
    {
        var user = await _users.FindByVerificationTokenAsync(cmd.Token, ct);
        if (user is null) return Result<VerifyEmailResponse>.Failure("Invalid or expired token.", "INVALID_TOKEN");
        if (user.Status == UserStatus.Active) return Result<VerifyEmailResponse>.Failure("Email already verified.", "ALREADY_VERIFIED");

        user.RecordEmailVerification(cmd.Token);
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<VerifyEmailResponse>.Success(new VerifyEmailResponse(user.Id, user.TenantId, user.Role.ToString()));
    }
}
