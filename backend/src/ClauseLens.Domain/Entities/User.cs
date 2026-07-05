using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A tenant member. The first User in a Tenant MUST have InvitedById == null
/// (self-signup per FR-007c). Email verification is required before Active state.
/// </summary>
public class User : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.PendingInvite;
    public DateTime? EmailVerifiedAt { get; private set; }
    public Guid? InvitedById { get; private set; }
    public string? EmailVerificationToken { get; private set; }

    private User() { }

    public static User CreateFirstAdmin(Guid tenantId, string email, string passwordHash)
        => CreateInternal(tenantId, email, passwordHash, UserRole.Admin, invitedById: null);

    public static User CreateInvited(Guid tenantId, string email, string passwordHash, UserRole role, Guid invitedById)
    {
        if (role == UserRole.Admin)
            throw new DomainException("Only the first Admin may be self-provisioned. Invite additional Admins via a separate bootstrap flow.");
        return CreateInternal(tenantId, email, passwordHash, role, invitedById);
    }

    private static User CreateInternal(Guid tenantId, string email, string passwordHash, UserRole role, Guid? invitedById)
    {
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("Valid email is required.");
        if (string.IsNullOrEmpty(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            Status = UserStatus.PendingInvite,
            InvitedById = invitedById,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void RecordEmailVerification(string token)
    {
        if (Status == UserStatus.Active) return;
        EmailVerificationToken = token;
        EmailVerifiedAt = DateTime.UtcNow;
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = UserStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum UserRole
{
    Admin = 0,
    ContractOwner = 1,
    Reviewer = 2,
}

public enum UserStatus
{
    PendingInvite = 0,
    Active = 1,
    Disabled = 2,
}
