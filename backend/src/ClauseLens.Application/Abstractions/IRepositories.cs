using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;

namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Repository contracts. Kept in Application/Abstractions per Clean Architecture
/// so the Application layer doesn't depend on EF Core. Implementations live in
/// Infrastructure/Persistence.
/// </summary>
public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task<Tenant?> FindByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken ct = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<User?> FindByEmailGlobalAsync(string email, CancellationToken ct = default);
    Task<User?> FindByVerificationTokenAsync(string token, CancellationToken ct = default);
    Task<bool> EmailExistsInTenantAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<bool> EmailExistsGloballyAsync(string email, CancellationToken ct = default);
}

public interface IPlaybookRepository
{
    Task AddAsync(Playbook playbook, CancellationToken ct = default);
    Task<Playbook?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Playbook>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PlaybookRule>> ListPublishedRulesForClauseTypeAsync(Guid tenantId, string clauseType, CancellationToken ct = default);
}

public interface IContractRepository
{
    Task AddAsync(Contract contract, CancellationToken ct = default);
    Task<Contract?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task ReplaceClausesAsync(Guid contractId, IReadOnlyCollection<Clause> clauses, CancellationToken ct = default);
    Task<IReadOnlyList<Contract>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken ct = default);
}

public interface IReviewTaskRepository
{
    Task AddAsync(ReviewTask task, CancellationToken ct = default);
    Task<ReviewTask?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReviewTask?> FindByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewTask>> ListStallingTasksAsync(DateTime now, CancellationToken ct = default);
}

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken ct = default);
    Task<string> GetLatestHashAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEntry>> ListAsync(Guid tenantId, Guid? contractId, string? actionType, int skip, int take, CancellationToken ct = default);
}

public interface IRiskFlagRepository
{
    Task AddRangeAsync(IEnumerable<RiskFlag> flags, CancellationToken ct = default);
    Task<IReadOnlyList<RiskFlag>> ListByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<(RiskFlag flag, PlaybookRule rule)>> ListByContractWithRulesAsync(Guid contractId, CancellationToken ct = default);
    Task<RiskFlag?> FindByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IRedlineRepository
{
    Task AddRangeAsync(IEnumerable<Redline> redlines, CancellationToken ct = default);
    Task<IReadOnlyList<Redline>> ListByContractAsync(Guid contractId, CancellationToken ct = default);
}

public interface IObligationRepository
{
    Task AddRangeAsync(IEnumerable<Obligation> obligations, CancellationToken ct = default);
    Task<IReadOnlyList<Obligation>> ListByContractAsync(Guid contractId, CancellationToken ct = default);
}

public interface IErasureRequestRepository
{
    Task AddAsync(ErasureRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ErasureRequest>> ListPendingPastSlaAsync(DateTime now, CancellationToken ct = default);
}

public interface IPlaybookTemplateSeeder
{
    IEnumerable<(string clauseType, string condition, Domain.Entities.RiskSeverity severity, string standardLanguage, string guideline)> LoadTemplate(string templateId);
}
